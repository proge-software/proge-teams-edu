using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class Turno
    {
        public string domPartCod { get; set; }
        public string fatPartCod { get; set; }
        public string aulaDes { get; set; }
        public string extAulaCod { get; set; }
        public string aulaCod { get; set; }
        public string edificioDes { get; set; }
        public string edificioCod { get; set; }
        public string des { get; set; }
        public string dataOraEsa { get; set; }
        public int? edificioId { get; set; }
        public int? aulaId { get; set; }
        public int? appLogId { get; set; }
        public int? appId { get; set; }
        public int? adId { get; set; }
        public int? cdsId { get; set; }
    }


}
