using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class Docente
    {
        public string cellulare { get; set; }
        public string codFis { get; set; }
        public string dataFinAtt { get; set; }
        public string dataIniAtt { get; set; }
        public string dataMod { get; set; }
        public string dataModDoc { get; set; }
        public string dataNascita { get; set; }
        public string dipCod { get; set; }
        public string dipDes { get; set; }
        //public int dipId { get; set; }
        public Int64? dipId { get; set; }
        public string docenteCognome { get; set; }
        //public int docenteId { get; set; }
        public Int64? docenteId { get; set; }
        public string docenteMatricola { get; set; }
        public string docenteNome { get; set; }
        public string eMail { get; set; }
        public string facCod { get; set; }
        public string facDes { get; set; }
        //public int facId { get; set; }
        public Int64? facId { get; set; }
        public string hyperlink { get; set; }
        //public int idAb { get; set; }
        public Int64? idAb { get; set; }
        public string noteBiografiche { get; set; }
        public string noteCurriculum { get; set; }
        public string noteDocente { get; set; }
        public string notePubblicazioni { get; set; }
        public string profilo { get; set; }
        public string ruoloDocCod { get; set; }
        public string ruoloDocDes { get; set; }
        public string sesso { get; set; }
        public string settCod { get; set; }
        public string settDes { get; set; }
        public string userId { get; set; }
    }
}
