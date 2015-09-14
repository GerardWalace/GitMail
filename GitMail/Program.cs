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
                    ExecuteCommand(repoConf.DirectoryPath, "git fetch");

                // On nettoie le repo pour supprimer tous les commit temporaires effectué par un precedent traitement
                ExecuteCommand(repoConf.DirectoryPath, "git gc --auto");

                // Pour chacun des couples de branche on lance le traitement de test de merge
                var mergeConfs = from MergeConfiguration mergeConf in repoConf.Merges select mergeConf;
                foreach (var mergeConf in mergeConfs)
                {
                    var mailStruct = new MailStruct();
                    mailStruct.Objet = mergeConf.MailObject;
                    mailStruct.Destinataire = mergeConf.MailsDesReferents;
                    mailStruct.BranchInto = mergeConf.IntoBranch;
                    mailStruct.BranchFrom = mergeConf.FromBranch;

                    // On indique le dernier commit sur chaque branche
                    mailStruct.BranchInto_LastCommit = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log -1 --pretty=format:\"%h\" origin/{0}", mergeConf.IntoBranch));;
                    mailStruct.BranchFrom_LastCommit = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log -1 --pretty=format:\"%h\" origin/{0}", mergeConf.FromBranch)); ;

                    // On liste les commits qui vont être mergés
                    string commitsAMerger = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --oneline origin/{0}..origin/{1}", mergeConf.IntoBranch, mergeConf.FromBranch));
                    mailStruct.CommitsMerged.AddRange(SplitResult(commitsAMerger));

                    // On effectue un Checkout de la branch 1 (en detach pour ne pas avoir d'impact sur celle-ci)
                    ExecuteCommand(repoConf.DirectoryPath, String.Format("git checkout origin/{0} --detach", mergeConf.IntoBranch));

                    // On effectue le merge avec la branch 2
                    ExecuteCommand(repoConf.DirectoryPath, String.Format("git merge --quiet --stat origin/{0}", mergeConf.FromBranch));

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
                            mailStructFichier.LogBranchFrom.AddRange(SplitResult(ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --follow origin/{0}..origin/{1} --pretty=format:\"%h %an\" -- {2}", mergeConf.IntoBranch, mergeConf.FromBranch, fichier))));
                            mailStructFichier.LogBranchInto.AddRange(SplitResult(ExecuteCommand(repoConf.DirectoryPath, String.Format("git log --follow origin/{0}..origin/{1} --pretty=format:\"%h %an\" -- {2}", mergeConf.FromBranch, mergeConf.IntoBranch, fichier))));

                            mailStruct.Fichiers.Add(mailStructFichier);
                        }

                        // On annule le merge en erreur
                        ExecuteCommand(repoConf.DirectoryPath, "git merge --abort");
                    }

                    // On prépare le mail de récapitulatif
                    string body = mailStruct.GetMailBody();

                    // On envoi le mail
                    SendMail(mailStruct.Objet, mailStruct.Destinataire, body);            
                }
            }     
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
            Console.WriteLine();
            Console.WriteLine(command);

            // Le /c signifie que l'on execute la command et que l'on quitte ensuite
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

            procStartInfo.WorkingDirectory = workingDirectory;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            try
            {
                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                Console.WriteLine(output);
                Console.WriteLine(error);
                return output;
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
