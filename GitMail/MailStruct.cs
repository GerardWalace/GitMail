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
        }

        public string Objet { get; set; }

        public string Destinataire { get; set; }

        public string BranchInto { get; set; }

        public string BranchFrom { get; set; }

        public List<string> CommitsMerged { get; private set; }

        public List<MailStructFichier> Fichiers { get; private set; }

        /// <summary>
        /// Le but de cette méthode est de génrer un message HTML de compte rendu de merge
        /// </summary>
        /// <returns>Un string contenant la page HTML</returns>
        public String GetMailBody()
        {
            string html = String.Empty;

            html += "<html>";
            html += "<body>";
            html += String.Format("<p><b><u><span style='font-size:14.0pt'>Compte-rendu de la simulation de merge de <span style='color:red'>{0}</span> vers <span style='color:red'>{1}</span></span></u></b></p>", BranchFrom, BranchInto);
            html += "<p>&nbsp;</p>";

            // S'il y eu des conflits
            if (Fichiers.Any())
            {
                html += String.Format("<p>Il y a eu <b><span style='font-size:14.0pt;color:red'>{0}</span></b> conflits !!!</p>", Fichiers.Count);

                html += "<p>&nbsp;</p>";
                html += "<p><b><u>En voici la liste :</u></b></p>";
                html += "<p>&nbsp;</p>";

                html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50pt'>";
                html += "<tr>";
                html += "<td style='background:#BFBFBF;padding:0cm 5.4pt 0cm 5.4pt'>";
                html += "<p style='text-align:center'><b>Fichiers en conflits</b></p>";
                html += "</td>";
                html += "<td style='background:#BFBFBF;padding:0cm 5.4pt 0cm 5.4pt'>";
                html += String.Format("<p style='text-align:center'><b>Commits sur la branche <span style='color:red'>{0}</span></b></p>", BranchFrom); ;
                html += "</td>";
                html += "<td style='background:#BFBFBF;padding:0cm 5.4pt 0cm 5.4pt'>";
                html += String.Format("<p style='text-align:center'><b>Commits sur la branche <span style='color:red'>{0}</span></b></p>", BranchInto); ;
                html += "</td>";
                html += "</tr>";

                // Une ligne pour chaque fichier
                foreach (var fichier in Fichiers)
                {
                    html += "<tr>";
                    html += "<td style='padding:0cm 5.4pt 0cm 5.4pt'>";
                    html += String.Format("<p>{0}</p>", fichier.FichierPath);
                    html += "</td>";
                    html += "<td style='padding:0cm 5.4pt 0cm 5.4pt'>";
                    foreach (var log in fichier.LogBranchFrom)
                    {
                        html += String.Format("<p>{0}</p>", log);
                    }
                    html += "</td>";
                    html += "<td style='padding:0cm 5.4pt 0cm 5.4pt'>";
                    foreach (var log in fichier.LogBranchInto)
                    {
                        html += String.Format("<p>{0}</p>", log);
                    }
                    html += "</td>";
                    html += "</tr>";
                }
                html += "</table>";
            }
            else
            {
                html += "<p>Il n'y a pas eu de conflits, <b><span style='font-size:14.0pt;color:green'>BRAVO</span></b> !!!</p>";
            }

            html += "<p>&nbsp;</p>";
            html += String.Format("<p><b><u>La listes des <span style='font-size:14.0pt;color:red'>{0}</span> commits qui peuvent être mergés de {1} vers {2} est la suivante :</u></b></p>", CommitsMerged.Count, BranchFrom, BranchInto);
            html += "<p>&nbsp;</p>";

            html += "<table border=1 cellspacing=0 cellpadding=0 style='margin-left:50pt'>";
            foreach (var commit in CommitsMerged)
            {
                html += "<tr>";
                html += "<td style='padding:0cm 5.4pt 0cm 5.4pt'>";
                html += String.Format("<p>{0}</p>", commit);
                html += "</td>";
                html += "</tr>";
            }
            html += "</table>";

            html += "<p>&nbsp;</p>";
            html += "<p>En vous souhaitant une git journée.</p>";
            html += "<p>A demain !</p>";
            html += "<p>&nbsp;</p>";

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
}
