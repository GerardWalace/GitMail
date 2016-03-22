using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMail
{

    class MailStruct
    {
        public MailStruct()
        {
            Fichiers = new List<MailStructFichier>();
            CommitsMerged = new List<string>();
            BranchesAhead = new List<MailStructBranch>();
            BranchesBefore = new List<MailStructBranch>();
        }

        public string Objet { get; set; }

        public string Destinataire { get; set; }

        public string BranchInto { get; set; }

        public string BranchInto_LastCommit { get; set; }

        public string BranchFrom { get; set; }

        public string BranchFrom_LastCommit { get; set; }

        public List<string> CommitsMerged { get; private set; }

        public List<MailStructBranch> BranchesAhead { get; private set; }

        public List<MailStructBranch> BranchesBefore { get; private set; }

        public List<MailStructFichier> Fichiers { get; private set; }

        /// <summary>
        /// Le but de cette méthode est de génrer un message HTML de compte rendu de merge
        /// </summary>
        /// <returns>Un string contenant la page HTML</returns>
        public String GetMailBody()
        {
            // On ordonne les listes puisqu'elles sont remplies par des thread en parallele
            var branchesAhead = BranchesAhead.OrderBy(b => b.BranchName).ToList();
            var branchesBefore = BranchesBefore.OrderBy(b => b.BranchName).ToList();
            var commitsMerged = CommitsMerged; // On ordonne pas les commits puisqu'ils sont classés par date
            var fichiers = Fichiers.OrderBy(f => f.FichierPath).ToList();

            string html = String.Empty;

            html += "<html>";
            html += "<body>";
            html += String.Format("<p><b><u><span style='font-size:14.0pt'>Compte-rendu de la simulation de merge de <span style='color:red'>{0}</span> ({1}) vers <span style='color:red'>{2}</span> ({3})</span></u></b></p>", BranchFrom, BranchFrom_LastCommit, BranchInto, BranchInto_LastCommit);
            //html += "<p>&nbsp;</p>";

            // S'il y eu des conflits
            if (fichiers.Any())
            {
                html += String.Format("<p>Il y a eu <b><span style='font-size:14.0pt;color:red'>{0}</span></b> conflits !!!</p>", fichiers.Count);

                //html += "<p>&nbsp;</p>";
                html += "<p><b><u>En voici la liste :</u></b></p>";
                //html += "<p>&nbsp;</p>";

                html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50px'>";
                html += "<tr>";
                html += "<td style='background:#BFBFBF;padding:5px'>";
                html += "<p style='text-align:center'><b>Fichiers en conflits</b></p>";
                html += "</td>";
                html += "<td style='background:#BFBFBF;padding:5px'>";
                html += String.Format("<p style='text-align:center'><b>Commits sur la branche <span style='color:red'>{0}</span> ({1})</b></p>", BranchFrom, BranchFrom_LastCommit); ;
                html += "</td>";
                html += "<td style='background:#BFBFBF;padding:5px'>";
                html += String.Format("<p style='text-align:center'><b>Commits sur la branche <span style='color:red'>{0}</span> ({1})</b></p>", BranchInto, BranchInto_LastCommit); ;
                html += "</td>";
                html += "</tr>";

                // Une ligne pour chaque fichier
                foreach (var fichier in fichiers)
                {
                    html += "<tr>";
                    html += "<td style='padding:5px'>";
                    html += String.Format("<p>{0}</p>", fichier.FichierPath);
                    html += "</td>";
                    html += "<td style='padding:5px'>";
                    html += "<p>";
                    foreach (var log in fichier.LogBranchFrom)
                    {
                        html += String.Format("{0}<br>", log);
                    }
                    html += "</p>";
                    html += "</td>";
                    html += "<td style='padding:5px'>";
                    html += "<p>";
                    foreach (var log in fichier.LogBranchInto)
                    {
                        html += String.Format("{0}<br>", log);
                    }
                    html += "</p>";
                    html += "</td>";
                    html += "</tr>";
                }
                html += "</table>";
            }
            else
            {
                html += "<p>Il n'y a pas eu de conflits, <b><span style='font-size:14.0pt;color:green'>BRAVO</span></b> !!!</p>";
            }

            //html += "<p>&nbsp;</p>";
            html += String.Format("<p><b><u>La listes des <span style='font-size:14.0pt;color:red'>{0}</span> commits qui peuvent être mergés de {1} vers {2} est la suivante :</u></b></p>", commitsMerged.Count, BranchFrom, BranchInto);
            //html += "<p>&nbsp;</p>";

            html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50px'>";
            foreach (var commit in commitsMerged)
            {
                html += "<tr>";
                html += "<td style='padding:5px'>";
                html += String.Format("<p>{0}</p>", commit);
                html += "</td>";
                html += "</tr>";
            }
            html += "</table>";

            //html += "<p>&nbsp;</p>";
            html += String.Format("<p><b><u>La listes des <span style='font-size:14.0pt;color:red'>{0}</span> branches \"filles\" qui peuvent être mergés vers {1} est la suivante :</u></b></p>", branchesAhead.Count, BranchInto);
            //html += "<p>&nbsp;</p>";

            html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50px'>";
            foreach (var branch in branchesAhead)
            {
                html += "<tr>";
                html += "<td style='padding:5px'>";
                html += String.Format("<p>{0}</p>", branch.BranchName);
                html += "</td>";
                html += "<td style='padding:5px'>";
                html += String.Format("<p>{0}</p>", branch.LastUpdate);
                html += "</td>";
                html += "</tr>";
            }
            html += "</table>";

            //html += "<p>&nbsp;</p>";
            html += String.Format("<p><b><u>Certaines branches \"filles\" (<span style='font-size:14.0pt;color:red'>{0}</span>) de {1} pourraient être supprimées :</u></b></p>", branchesBefore.Count, BranchInto);
            //html += "<p>&nbsp;</p>";

            html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50px'>";
            foreach (var branch in branchesBefore)
            {
                html += "<tr>";
                html += "<td style='padding:5px'>";
                html += String.Format("<p>{0}</p>", branch.BranchName);
                html += "</td>";
                html += "<td style='padding:5px'>";
                html += String.Format("<p>{0}</p>", branch.LastUpdate);
                html += "</td>";
                html += "</tr>";
            }
            html += "</table>";

            //html += "<p>&nbsp;</p>";
            html += "<p>En vous souhaitant une git journée.</p>";
            html += "<p>A demain !</p>";
            //html += "<p>&nbsp;</p>";

            html += "</body>";
            html += "</html>";

            return html;
        }
    }

    class MailStructFichier
    {
        public MailStructFichier()
        {
            LogBranchInto = new List<string>();
            LogBranchFrom = new List<string>();
        }

        public string FichierPath { get; set; }

        public List<string> LogBranchInto { get; private set; }

        public List<string> LogBranchFrom { get; private set; }
    }
    
    class MailStructBranch
    {
        public String BranchName { get; set; }
        public String LastUpdate { get; set; }
    }
}
