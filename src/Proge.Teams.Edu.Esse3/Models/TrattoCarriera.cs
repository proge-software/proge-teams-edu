using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TrattoCarriera
    {
        public Int64? persId { get; set; }
        public Int64? stuId { get; set; }
        public Int64 matId { get; set; }
        public string cognome { get; set; }
        public string nome { get; set; }
        public string codiceFiscale { get; set; }
        public string matricola { get; set; }
        public Int64 cdsId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        public int aaOrdId { get; set; }
        public Int64 pdsId { get; set; }
        public string pdsCod { get; set; }
        public string pdsDes { get; set; }
        public Int64? aaRegId { get; set; }
        public string staStuCod { get; set; }
        public string staStuDes { get; set; }
        public string staMatCod { get; set; }
        public string staMatDes { get; set; }
        public string umPesoCod { get; set; }
        public string umPesoDes { get; set; }
        public string codiceLettore { get; set; }
        public Int64? titoloStudio { get; set; }
        public string tipoLettore { get; set; }
        public string autDatiPersonali { get; set; }
        public Int64? statoTasse { get; set; }
    }
}
