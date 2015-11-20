using GitMail.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitMail
{
    class Program
    {
        static void Main(string[] args)
        {
            // On récupère la conf
            var conf = GitMailConfiguration.Instance;

            // Si un path specifique de git est donné, on le rajoute au path
            // TODO Rajouter le path de git au path de la présente application

            // On lance le traitement pour chaque repo
            var repoConfs = from RepositoryConfiguration repoConf in conf.Repositories select repoConf;
            foreach (var repoConf in repoConfs)
            {
                // On verifie si le clone a déjà été effectué, si ce n'est pas le cas on le lance, si c'est le cas on fetch
                if (!Directory.Exists(repoConf.DirectoryPath))
                    ExecuteCommand(String.Format("git clone {0} {1}", repoConf.RepositoryPath, repoConf.DirectoryPath));
                else
                    ExecuteCommand(repoConf.DirectoryPath, "git fetch --prune");

                // On nettoie le repo pour supprimer tous les commit temporaires effectué par un precedent traitement
                ExecuteCommand(repoConf.DirectoryPath, "git gc --auto");

                // Pour chacun des couples de branche on lance le traitement de test de merge
                var mergeConfs = from MergeConfiguration mergeConf in repoConf.Merges select mergeConf;
                foreach (var mergeConf in mergeConfs)
                {
                    List<string> listFromBranches = new List<string>();
                    if (mergeConf.IsMultiBranch)
                    {
                        string fromBranches = ExecuteCommand(repoConf.DirectoryPath, String.Format("git branch -r | grep \"{0}\" | ForEach-Object {{ $_.Trim() }}", mergeConf.FromBranch));
                        listFromBranches.AddRange(SplitResult(fromBranches));
                    }
                    else
                    {
                        listFromBranches.Add(mergeConf.FromBranch);
                    }

                    foreach (var fromBranch in listFromBranches)
                    {
                        MergeAndMail(repoConf,
                            mergeConf.MailsDesReferents,
                            mergeConf.IntoBranch,
                            fromBranch);
                    }
                }
            }

            // On fait une pause afin de laisser l'utilisateur checker que tout va bien.
            Console.WriteLine("End of GitMail... Press Enter...");
            Console.ReadLine();
        }

        private static void MergeAndMail(RepositoryConfiguration repoConf, string destinataire, string branchInto, string branchFrom)
        {
            var mailStruct = new MailStruct();
            mailStruct.Objet = String.Format("[GitMail] Simulation de merge {0} vers {1}", branchFrom, branchInto);
            mailStruct.Destinataire = destinataire;
            mailStruct.BranchInto = branchInto;
            mailStruct.BranchFrom = branchFrom;

            // On indique le dernier commit sur chaque branche
            mailStruct.BranchInto_LastCommit = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log -1 --pretty=format:\"%h\" {0}", branchInto)); ;
            mailStruct.BranchFrom_LastCommit = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log -1 --pretty=format:\"%h\" {0}", branchFrom)); ;

            // On liste les commits qui vont être mergés
            string commitsAMerger = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --oneline {0}..{1}", branchInto, branchFrom));
            mailStruct.CommitsMerged.AddRange(SplitResult(commitsAMerger));

            // On effectue un Checkout de la branch 1 (en detach pour ne pas avoir d'impact sur celle-ci)
            ExecuteCommand(repoConf.DirectoryPath, String.Format("git checkout {0} --detach", branchInto));

            // On recupere la liste de toutes les branches "filles" qui pourraient etre recuperees
            string branchesFillesAhead = ExecuteCommand(repoConf.DirectoryPath, string.Format("git branch -r --no-merged | grep \"${0}-\" | ForEach-Object {{ $_.Trim() }}", branchInto));
            foreach (string branchName in SplitResult(branchesFillesAhead))
            {
                string lastUpdate = ExecuteCommand(repoConf.DirectoryPath, string.Format("git log -1 --pretty=format:%cr {0}", branchName));
                mailStruct.BranchesAhead.Add(new MailStructBranch() { BranchName = branchName, LastUpdate = lastUpdate });
            }

            // On recupere la liste de toutes les branches "filles" qui peuvent etre supprimees
            string branchesFillesBefore = ExecuteCommand(repoConf.DirectoryPath, string.Format("git branch -r --merged | grep \"${0}-\" | ForEach-Object {{ $_.Trim() }}", branchInto));
            foreach (string branchName in SplitResult(branchesFillesBefore))
            {
                string lastUpdate = ExecuteCommand(repoConf.DirectoryPath, string.Format("git log -1 --pretty=format:%cr {0}", branchName));
                mailStruct.BranchesBefore.Add(new MailStructBranch() { BranchName = branchName, LastUpdate = lastUpdate });
            }

            // On effectue le merge avec la branch 2
            ExecuteCommand(repoConf.DirectoryPath, String.Format("git merge --quiet --stat {0}", branchFrom));

            // On les fichiers en conflits
            string fichiersEnConflit = ExecuteCommand(repoConf.DirectoryPath, "git ls-files --unmerged | cut --fields 2 | uniq");
            var listFichiers = SplitResult(fichiersEnConflit);

            if (listFichiers.Any())
            {
                foreach (var fichier in listFichiers)
                {
                    var mailStructFichier = new MailStructFichier();
                    mailStructFichier.FichierPath = fichier;

                    // Pour chacun des fichiers en conflit, on récupère des informations sur les derniers commits
                    mailStructFichier.LogBranchFrom.AddRange(SplitResult(ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --follow {0}..{1} --pretty=format:\\\"%h %an\\\" -- {2}", branchInto, branchFrom, fichier))));
                    mailStructFichier.LogBranchInto.AddRange(SplitResult(ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --follow {0}..{1} --pretty=format:\\\"%h %an\\\" -- {2}", branchFrom, branchInto, fichier))));

                    mailStruct.Fichiers.Add(mailStructFichier);
                }
            }

            if (File.Exists(Path.Combine(repoConf.DirectoryPath, ".git", "MERGE_HEAD")))
            {
                // On annule le merge en erreur
                ExecuteCommand(repoConf.DirectoryPath, "git merge --abort");
            }

            // On prépare le mail de récapitulatif
            string body = mailStruct.GetMailBody();

            // On envoi le mail si un merge est possible (s'il y a des commits "Ahead")
            if (mailStruct.CommitsMerged.Any() || mailStruct.BranchesAhead.Any() || mailStruct.BranchesBefore.Any())
                SendMail(mailStruct.Objet, mailStruct.Destinataire, body);
        }

        static List<string> SplitResult(string output)
        {
            if (output != null)
                return output.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Where(s => !String.IsNullOrEmpty(s)).ToList();
            else
                return null;
        }

        static string ExecuteCommand(string command)
        {
            return ExecuteCommand(Directory.GetCurrentDirectory(), command);
        }

        static string ExecuteCommand(string workingDirectory, string command)
        {
            int timeout = 600000;// 10 minutes de timeout...

            //Console.WriteLine();
            Console.WriteLine(command);

            // Le /c signifie que l'on execute la command et que l'on quitte ensuite
            ProcessStartInfo procStartInfo = new ProcessStartInfo("powershell", "-Command " + command);

            procStartInfo.WorkingDirectory = workingDirectory;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo = procStartInfo;
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        proc.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };
                        proc.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        proc.Start();

                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        if (proc.WaitForExit(timeout) &&
                            outputWaitHandle.WaitOne(timeout) &&
                            errorWaitHandle.WaitOne(timeout))
                        {
                            Console.WriteLine(error.ToString());
                            return output.ToString();
                        }
                        else
                        {
                            throw new TimeoutException(String.Format("Bon... Ca fait {0} minutes qu'on attend, il est peut être temps d'arreter !", timeout / 60000));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return String.Empty;
            }
        }

        static void SendMail(string subject, string destinataires, string body)
        {
            MailMessage mail = new MailMessage("test.git@gmail.com", destinataires);
            SmtpClient client = new SmtpClient();
            
            client.Port = 25;

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("test.git@gmail.com", "password");
            
            client.Host = "smtp.gmail.com";
            
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;

            try
            {
                client.Send(mail);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
