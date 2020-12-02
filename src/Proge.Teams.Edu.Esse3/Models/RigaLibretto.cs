using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    public class Stato
    {
        public string value { get; set; }
    }

    public class RaggEsaTipo
    {
        public string value { get; set; }
    }

    public class EsitoLibretto
    {
        public ModValCod modValCod { get; set; }
        public int supEsaFlg { get; set; }
        //public int voto { get; set; }
        public float? voto { get; set; }
        //public int lodeFlg { get; set; }
        public int? lodeFlg { get; set; }
        public string tipoGiudCod { get; set; }
        public string tipoGiudDes { get; set; }
        public string dataEsa { get; set; }
        //public int aaSupId { get; set; }
        public int? aaSupId { get; set; }
    }

    public class RigaLibretto
    {
        //public int matId { get; set; }
        public Int64 matId { get; set; }
        public int ord { get; set; }
        public int adsceId { get; set; }
        //public int stuId { get; set; }
        public Int64 stuId { get; set; }
        //public int pianoId { get; set; }
        public Int64? pianoId { get; set; }
        //public int itmId { get; set; }
        public Int64? itmId { get; set; }
        //public int ragId { get; set; }
        public Int64? ragId { get; set; }
        public RaggEsaTipo raggEsaTipo { get; set; }
        public string adCod { get; set; }
        public string adDes { get; set; }
        public int annoCorso { get; set; }
        public Stato stato { get; set; }
        public string statoDes { get; set; }
        public ChiaveAdContestualizzata chiaveADContestualizzata { get; set; }
        public string tipoEsaCod { get; set; }
        public string tipoEsaDes { get; set; }
        public string tipoInsCod { get; set; }
        public string tipoInsDes { get; set; }
        public int ricId { get; set; }
        //public int peso { get; set; }
        public float peso { get; set; }
        //public int aaFreqId { get; set; }
        public int? aaFreqId { get; set; }
        public string dataFreq { get; set; }
        public int freqUffFlg { get; set; }
        //public int freqObbligFlg { get; set; }
        public int? freqObbligFlg { get; set; }
        public string dataScadIscr { get; set; }
        //public int gruppoVotoId { get; set; }
        public int? gruppoVotoId { get; set; }
        //public int gruppoVotoMinVoto { get; set; }
        public int? gruppoVotoMinVoto { get; set; }
        //public int gruppoVotoMaxVoto { get; set; }
        public int? gruppoVotoMaxVoto { get; set; }
        //public int gruppoVotoLodeFlg { get; set; }
        public int? gruppoVotoLodeFlg { get; set; }
        public string gruppoGiudCod { get; set; }
        public string gruppoGiudDes { get; set; }
        public EsitoLibretto esito { get; set; }
        public int sovranFlg { get; set; }
        public string note { get; set; }
        public int debitoFlg { get; set; }
        //public int ofaFlg { get; set; }
        public int? ofaFlg { get; set; }
        //public int annoCorsoAnticipo { get; set; }
        public int? annoCorsoAnticipo { get; set; }
        public int genAutoFlg { get; set; }
        public int genRicSpecFlg { get; set; }
        //public int tipoOrigEvecar { get; set; }
        public int? tipoOrigEvecar { get; set; }
        //public int numAppelliPrenotabili { get; set; }
        public int? numAppelliPrenotabili { get; set; }
        //public int superataFlg { get; set; }
        public int? superataFlg { get; set; }
        //public int numPrenotazioni { get; set; }
        public int? numPrenotazioni { get; set; }
    }
}
