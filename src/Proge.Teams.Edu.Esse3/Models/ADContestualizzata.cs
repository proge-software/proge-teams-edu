using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ChiaveAdContestualizzata
    {
        public Int64 cdsId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        //public int aaOrdId { get; set; }
        public int? aaOrdId { get; set; }
        public string aaOrdCod { get; set; }
        public string aaOrdDes { get; set; }
        //public Int64 pdsId { get; set; }
        public Int64? pdsId { get; set; }
        public string pdsCod { get; set; }
        public string pdsDes { get; set; }
        //public Int64 aaOffId { get; set; }
        public Int64? aaOffId { get; set; }
        //public Int64 adId { get; set; }
        public Int64? adId { get; set; }
        public string adCod { get; set; }
        public string adDes { get; set; }
        //public int afId { get; set; }
        public Int64? afId { get; set; }
    }

    public class ChiaveAdContestualizzataRidotta
    {
        public Int64? cdsId { get; set; }
        public int? aaOrdId { get; set; }
        public Int64? pdsId { get; set; }
        public int? aaOffId { get; set; }
        public Int64? adId { get; set; }
    }

    public class ChiaveAdContCapoGruppo
    {
        public Int64 cdsId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        public int aaOrdId { get; set; }
        public string aaOrdCod { get; set; }
        public string aaOrdDes { get; set; }
        public Int64 pdsId { get; set; }
        public string pdsCod { get; set; }
        public string pdsDes { get; set; }
        public Int64 aaOffId { get; set; }
        public Int64 adId { get; set; }
        public string adCod { get; set; }
        public string adDes { get; set; }
        //public Int64 afId { get; set; }
        public Int64? afId { get; set; }
    }

    public class ADContestualizzata
    {
        public ChiaveAdContestualizzata chiaveAdContestualizzata { get; set; }
        public string adDesEng { get; set; }
        public string cdsDesEng { get; set; }
        public string aaOrdDesEng { get; set; }
        public string pdsDesEng { get; set; }
        public string linguaInsDes { get; set; }
        public string linguaInsDesEng { get; set; }
        public int? nonErogabileOdFlg { get; set; }
        public string tipoEsaCod { get; set; }
        public string tipoEsaDes { get; set; }
        public string tipoEsaDesEng { get; set; }
        public string tipoValCod { get; set; }
        public string tipoValDes { get; set; }
        public string tipoValDesEng { get; set; }
        public string tipoInsCod { get; set; }
        public string tipoInsDes { get; set; }
        public string gruppoGiudCod { get; set; }
        public string gruppoGiudDes { get; set; }
        public int? reiterabile { get; set; }
        public string urlSitoWeb { get; set; }
        public string urlCorsoMoodle { get; set; }
        public ChiaveAdContCapoGruppo chiaveAdContCapoGruppo { get; set; }
        public int? capoGruppoFlg { get; set; }
    }
}
