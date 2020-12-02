using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ChiavePartizione
    {
        public int aaOffId { get; set; }
        public string fatPartCod { get; set; }
        public string fatPartDes { get; set; }
        public string fatPartDesEng { get; set; }
        public string domPartCod { get; set; }
        public string domPartDes { get; set; }
        public string domPartDesEng { get; set; }
        public string partCod { get; set; }
        public string partDes { get; set; }
        public string partDesEng { get; set; }
        public Int64 adLogId { get; set; }
    }
    public class ChiavePartizioneRidotta
    {
        public int? aaOffId { get; set; }
        public string fatPartCod { get; set; }
        public string domPartCod { get; set; }
        public string partCod { get; set; }
        public Int64? adLogId { get; set; }
    }

    public class CaricoDocentiOpz
    {
        public Int64 adLogId { get; set; }
        public Int64 udLogId { get; set; }
        public ChiavePartizioneRidotta chiavePartizione { get; set; }
        public string tipoCreCod { get; set; }
        public Int64 docenteId { get; set; }
        public string codFis { get; set; }
        public string eMail { get; set; }
        public string settCod { get; set; }
        public int? titolareFlg { get; set; }
        public int? respDidFlg { get; set; }
        public string userId { get; set; }
        public string dataModDoc { get; set; }
        public string dataMod { get; set; }
        public Int64? ugovCoperId { get; set; }
    }

    public class CaricoDocenti
    {
        public Int64 adLogId { get; set; }
        public Int64 udLogId { get; set; }
        public ChiavePartizione chiavePartizione { get; set; }
        public string tipoCreCod { get; set; }
        public string tipoCreDes { get; set; }
        public string tipoCreDesEng { get; set; }
        public Int64 docenteId { get; set; }
        public string docenteMatricola { get; set; }
        public string docenteNome { get; set; }
        public string docenteCognome { get; set; }
        //public int ore { get; set; }
        public float? ore { get; set; }
        //public int frazioneCarico { get; set; }
        public float? frazioneCarico { get; set; }
        //public int valDidFlg { get; set; }
        public int? valDidFlg { get; set; }
        //public int uGovCoperId { get; set; }
        public int? uGovCoperId { get; set; }
        public List<CaricoDocentiOpz> CaricoDocentiOpz { get; set; }
    }

    public class ChiaveUDContestualizzata
    {
        public ChiaveAdContestualizzata chiaveAdContestualizzata { get; set; }
        public Int64 udId { get; set; }
        public string udCod { get; set; }
        public string udDes { get; set; }
    }

    public class SyllabusUD
    {
        public Int64 adLogId { get; set; }
        public Int64 udLogId { get; set; }
        public ChiavePartizione chiavePartizione { get; set; }
        public ChiaveUDContestualizzata chiaveUDContestualizzata { get; set; }
        public int desUdPubblFlg { get; set; }
        public int? masterFlg { get; set; }
        public string contenuti { get; set; }
        public string contenutiEng { get; set; }
        public string obiettiviFormativi { get; set; }
        public string obiettiviFormativiEng { get; set; }
        public string prerequisiti { get; set; }
        public string prerequisitiEng { get; set; }
        public string testiRiferimento { get; set; }
        public string testiRiferimentoEng { get; set; }
        //public Int64 uGovAfId { get; set; }
        public Int64? uGovAfId { get; set; }
        //public Int64 uGovArId { get; set; }
        public Int64? uGovArId { get; set; }
    }

    //public class ChiaveUDMaster
    //{
    //    public ChiaveAdContestualizzata chiaveAdContestualizzata { get; set; }
    //    public Int64 udId { get; set; }
    //    public string udCod { get; set; }
    //    public string udDes { get; set; }
    //}

    public class UdLogConDettagli
    {
        public List<CaricoDocenti> CaricoDocenti { get; set; }
        public List<SyllabusUD> SyllabusUD { get; set; }
        public ChiavePartizione chiavePartizione { get; set; }
        //public ChiaveUDMaster chiaveUDMaster { get; set; }
        public ChiaveUDContestualizzata chiaveUDMaster { get; set; }
        //public Int64 udLogId { get; set; }
        public Int64? udLogId { get; set; }
        //public Int64 uGovArId { get; set; }
        public Int64? uGovArId { get; set; }
    }

    //public class ChiaveADContestualizzata4
    //{
    //    public int cdsId { get; set; }
    //    public int aaOrdId { get; set; }
    //    public int pdsId { get; set; }
    //    public int aaOffId { get; set; }
    //    public int adId { get; set; }
    //}

    public class AdLogOpz
    {
        //public ChiaveADContestualizzata4 chiaveADContestualizzata { get; set; }
        //public ChiaveAdContestualizzata chiaveADContestualizzata { get; set; }
        public ChiaveAdContestualizzataRidotta chiaveADContestualizzata { get; set; }
        //public ChiavePartizione chiavePartizione { get; set; }
        public ChiavePartizioneRidotta chiavePartizione { get; set; }
        //public Int64 facId { get; set; }
        public Int64? facId { get; set; }
        public string facCod { get; set; }
        public string facDes { get; set; }
        public string facDesEng { get; set; }
        public string areaDiscCod { get; set; }
        public string areaDiscDes { get; set; }
        public string areaDiscDesEng { get; set; }
        //public int integratoFlg { get; set; }
        public int? integratoFlg { get; set; }
        public string tipoCorsoCod { get; set; }
        public string tipoCorsoDes { get; set; }
        public string tipoCorsoDesEng { get; set; }
    }

    public class SyllabusAD
    {
        public Int64 adLogId { get; set; }
        public ChiavePartizione chiavePartizione { get; set; }
        public ChiaveAdContestualizzata chiaveADContestualizzata { get; set; }
        public int desAdPubblFlg { get; set; }
        //public int fisicaFlg { get; set; }
        public int? fisicaFlg { get; set; }
        public string contenuti { get; set; }
        public string contenutiEng { get; set; }
        public string obiettiviFormativi { get; set; }
        public string obiettiviFormativiEng { get; set; }
        public string prerequisiti { get; set; }
        public string prerequisitiEng { get; set; }
        public string metodiDidattici { get; set; }
        public string metodiDidatticiEng { get; set; }
        public string modalitaVerificaApprendimento { get; set; }
        public string modalitaVerificaApprendimentoEng { get; set; }
        public string altreInfo { get; set; }
        public string altreInfoEng { get; set; }
        public string testiRiferimento { get; set; }
        public string testiRiferimentoEng { get; set; }
        public List<AdLogOpz> adLogOpz { get; set; }
    }

    //public class ChiaveADFisica
    //{
    //    public int cdsId { get; set; }
    //    public string cdsCod { get; set; }
    //    public string cdsDes { get; set; }
    //    public int aaOrdId { get; set; }
    //    public string aaOrdCod { get; set; }
    //    public string aaOrdDes { get; set; }
    //    public int pdsId { get; set; }
    //    public string pdsCod { get; set; }
    //    public string pdsDes { get; set; }
    //    public int aaOffId { get; set; }
    //    public int adId { get; set; }
    //    public string adCod { get; set; }
    //    public string adDes { get; set; }
    //    //public int afId { get; set; }
    //    public int? afId { get; set; }
    //}

    public class DettaglioLogistica
    {
        public List<UdLogConDettagli> UdLogConDettagli { get; set; }
        public List<SyllabusAD> SyllabusAD { get; set; }
        public string dataModLog { get; set; }
        public string sedeDesEng { get; set; }
        public string sedeDes { get; set; }
        //public Int64 sedeId { get; set; }
        public Int64? sedeId { get; set; }
        public string tipoDidDesEng { get; set; }
        public string tipoDidDes { get; set; }
        public string tipoDidCod { get; set; }
        public string linguaDidDes { get; set; }
        public string linguaDidCod { get; set; }
        //public Int64 linguaDidId { get; set; }
        public Int64? linguaDidId { get; set; }
        public string partEffDes { get; set; }
        public string partEffCod { get; set; }
        public string domPartEffDes { get; set; }
        public string domPartEffCod { get; set; }
        public string fatPartEffDes { get; set; }
        public string fatPartEffCod { get; set; }
        public string dataFinValDid { get; set; }
        public string dataIniValDid { get; set; }
        public string dataFine { get; set; }
        public string dataInizio { get; set; }
        //public ChiaveADFisica chiaveADFisica { get; set; }
        public ChiaveAdContestualizzata chiaveADFisica { get; set; }
        public ChiavePartizione chiavePartizione { get; set; }
    }
}
