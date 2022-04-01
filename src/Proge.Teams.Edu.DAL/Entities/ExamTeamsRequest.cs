using System;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class ExamTeamsRequest
    {
        /// <summary>
        /// Chiave assoluta che identifca un singolo ExamTeamsRequest.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Chiave assoluta che identifca un singolo appello.
        /// </summary>

        public Int64? App_appelloId { get; set; }

        /// <summary>
        /// Data fine iscrizioni (DD/MM/YYYY).
        /// </summary>
        public DateTime? App_dataFineIscr { get; set; }

        /// <summary>
        /// Data inizio appello (DD/MM/YYYY).
        /// </summary>
        public DateTime? App_dataInizioApp { get; set; }

        /// <summary>
        /// Data inizio iscrizioni (DD/MM/YYYY).
        /// </summary>
        public DateTime? App_dataInizioIscr { get; set; }

        /// <summary>
        /// Id del corso di studio di erogazione dell'appello.
        /// </summary>
        public Int64? App_cdsId { get; set; }

        /// <summary>
        /// Codice del corso di studio di erogazione dell'appello.
        /// </summary>
        public string App_cdsCod { get; set; }

        /// <summary>
        /// Codice del corso di studio di erogazione dell'appello.
        /// </summary>
        public string App_cdsDes { get; set; }

        /// <summary>
        /// Richiesta di creazione del team in Teams.
        /// </summary>
        public bool TeamRequested { get; set; }
        public DateTime? TeamRequestDate { get; set; }
        public string TeamRequestUser { get; set; }

        /// <summary>
        /// Notifica di avvenuta creazione del team in Teams.
        /// </summary>
        public bool TeamCreated { get; set; }

        /// <summary>
        /// Eventuale data di creazione del team in Teams.
        /// </summary>
        public DateTime? TeamCreationDate { get; set; }

        /// <summary>
        /// JoinUrl del team in Teams.
        /// </summary>
        public string TeamJoinUrl { get; set; }
        public string TeamId { get; set; }
        public string Owners { get; set; }
        public string Members { get; set; }
        public string InternalId { get; set; }
        public bool IsMembershipLimitedToOwners { get; set; }
        public bool? MailSent { get; set; }
        public string Ins_aa_offerta { get; set; }
        public string Ins_cds_cod { get; set; }
        public string Ins_aa_ord { get; set; }
        public string Ins_pds_cod { get; set; }
        public string Ins_ad_cod { get; set; }
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string DipCod { get; set; }

        public string AdditionalDataString { get; set; }

        /// <summary>
        /// Identifica se un team e gia archivato in Microsoft 365 Data Center
        /// Attualmente viene fatta anche il archivazione in SQL
        /// </summary>
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }
    }
}
