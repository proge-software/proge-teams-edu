using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class StatoAttCod
    {
        public string value { get; set; }
    }

    public class Offerta
    {
        public int aaOffId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        public Int64 cdsOffId { get; set; }
        public string dataModOd { get; set; }
        public int? logisticaExistsFlg { get; set; }
        public int? offertaExistsFlg { get; set; }
        public StatoAttCod statoAttCod { get; set; }
        public string tipiCorsoCod { get; set; }
    }
}
