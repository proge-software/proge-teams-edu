using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class ModValCod
    {
        public string value { get; set; }
    }


    public class Esito
    {
        public int? assenteFlg { get; set; }
        public ModValCod modValCod { get; set; }
        public int? ritiratoFlg { get; set; }
        public int? superatoFlg { get; set; }
        public string tipoGiudCod { get; set; }
        public int? votoEsa { get; set; }
    }

    public class PresaVisione
    {
        public string value { get; set; }
    }

    public class StatoAdsce
    {
        public string value { get; set; }
    }


}
