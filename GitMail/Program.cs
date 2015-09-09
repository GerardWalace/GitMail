using GitMail.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            // TODO rajouter le path de git au path de la présente application

            // On lance le traitement pour chaque repo
            var repoConfs = from RepositoryConfiguration repoConf in conf.Repositories select repoConf;
            foreach (var repoConf in repoConfs)
            {
                // On verifie si le clone a déjà été effectué, si ce n'est pas le cas on le lance
                // TODO Test
                ExecuteCommand(String.Format("git clone {0} {1}", repoConf.RepositoryPath, repoConf.DirectoryPath));

                // On nettoie le repo pour supprimer tous les commit temporaires effectué par un precedent traitement
                ExecuteCommand(repoConf.DirectoryPath, "git gc");

                // Pour chacun des couples de branche on lance le traitement de test de merge
                var mergeConfs = from MergeConfiguration mergeConf in repoConf.Merges select mergeConf;
                foreach (var mergeConf in mergeConfs)
                {
                    // On effectue un Checkout de la branch 1 (en detach pour ne pas avoir d'impact sur celle-ci)
                    ExecuteCommand(repoConf.DirectoryPath, String.Format("git checkout origin/{0} --detach", mergeConf.IntoBranch));

                    // On effectue le merge avec la branch 2
                    ExecuteCommand(repoConf.DirectoryPath, String.Format("git merge --quiet --stat origin/{0}", mergeConf.FromBranch));

                    // On les fichiers en conflits
                    string fichiersEnConflit = ExecuteCommand(repoConf.DirectoryPath, "git ls-files --unmerged | cut --fields 2 | uniq");
                    var listFichiers = fichiersEnConflit.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                    if (listFichiers.Count > 0)
                    {
                        foreach (var fichier in listFichiers)
                        {
                            // Pour chacun des fichiers en conflit, on récupère des informations sur les derniers commits
                            string CommitsBranchFrom = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log origin/{0}..origin/{1} --pretty=format:\"Commit: %h%nAuthor: %an%nDate:   %ad%nTitle:  %s\" -- {3}", mergeConf.IntoBranch, mergeConf.FromBranch, fichier));
                            string CommitsBranchInto = ExecuteCommand(repoConf.DirectoryPath, String.Format("git log origin/{0}..origin/{1} --pretty=format:\"Commit: %h%nAuthor: %an%nDate:   %ad%nTitle:  %s\" -- {3}", mergeConf.FromBranch, mergeConf.IntoBranch, fichier));
                        }

                        // On annule le merge en erreur
                        ExecuteCommand(repoConf.DirectoryPath, "git merge --abort");
                    }
                    // On prépare le mail de récapitulatif

                    // On envoi le mail
                }
            }     
        }

        static String ExecuteCommand(String command)
        {
            return ExecuteCommand(Directory.GetCurrentDirectory(), command);
        }

        static String ExecuteCommand(String workingDirectory, String command)
        {
            try
            {
                // Le /c signifie que l'on execute la command et que l'on quitte ensuite
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

                procStartInfo.WorkingDirectory = workingDirectory;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                return proc.StandardOutput.ReadToEnd();
            }
            catch (Exception e)
            {
                // TODO Log
                return String.Empty;
            }
        }
    }
}
