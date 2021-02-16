using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class Sessione
    {
        public string dataFine { get; set; }
        public string dataInizio { get; set; }
        public string sesDes { get; set; }
        public int? sesId { get; set; }
        public int? aaSesId { get; set; }
        public int? appId { get; set; }
        public int? adId { get; set; }
        public int? cdsId { get; set; }
    }


}
