using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class EsameComuneAbilitazione
    {
        //public int cdsId { get; set; }
        public Int64 cdsId { get; set; }
        public int adId { get; set; }
        public int aaOffId { get; set; }
        //public int cdsFiglioId { get; set; }
        public Int64 cdsFiglioId { get; set; }
        //public int adFiglioId { get; set; }
        public Int64 adFiglioId { get; set; }
        public string cdsFiglioCod { get; set; }
        public string cdsFiglioDes { get; set; }
        public string adFiglioCod { get; set; }
        public string adFiglioDes { get; set; }
        //public int docenteId { get; set; }
        public Int64 docenteId { get; set; }
    }

    public class AbilitazioniDocente
    {
        public int aaAbilDocId { get; set; }
        //public int cdsId { get; set; }
        public Int64 cdsId { get; set; }
        public string cdsDefAppCod { get; set; }
        public int adId { get; set; }
        public string adDefAppCod { get; set; }
        //public int docenteId { get; set; }
        public Int64 docenteId { get; set; }
        public string docenteCognome { get; set; }
        public string docenteNome { get; set; }
        public int defApp { get; set; }
        public int visApp { get; set; }
        //public int minAaSesId { get; set; }
        public int? minAaSesId { get; set; }
        //public int maxAaSesId { get; set; }
        public int? maxAaSesId { get; set; }
        public string gruppoGiudCod { get; set; }
        public string gruppoVotoCod { get; set; }
        public List<EsameComuneAbilitazione> figliEsacom { get; set; }
    }
}
