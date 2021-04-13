using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ChiaveADFisica
    {
        public int? aaOffId { get; set; }
        public string aaOrdCod { get; set; }
        public string aaOrdDes { get; set; }
        public int? aaOrdId { get; set; }
        public string adCod { get; set; }
        public string adDes { get; set; }
        public int? adId { get; set; }
        public int? afId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        public int? cdsId { get; set; }
        public string pdsCod { get; set; }
        public string pdsDes { get; set; }
        public int? pdsId { get; set; }
    }

    public class ChiavePartizioneLogisitca
    {
        public int? aaOffId { get; set; }
        public int? adLogId { get; set; }
        public string domPartCod { get; set; }
        public string domPartDes { get; set; }
        public string domPartDesEng { get; set; }
        public string fatPartCod { get; set; }
        public string fatPartDes { get; set; }
        public string fatPartDesEng { get; set; }
        public string partCod { get; set; }
        public string partDes { get; set; }
        public string partDesEng { get; set; }
    }

    public class Logistica
    {
        public ChiaveADFisica chiaveADFisica { get; set; }
        public ChiavePartizioneLogisitca chiavePartizione { get; set; }
        public string dataFinValDid { get; set; }
        public string dataFine { get; set; }
        public string dataIniValDid { get; set; }
        public string dataInizio { get; set; }
        public string dataModLog { get; set; }
        public string domPartEffCod { get; set; }
        public string domPartEffDes { get; set; }
        public string fatPartEffCod { get; set; }
        public string fatPartEffDes { get; set; }
        public string linguaDidCod { get; set; }
        public string linguaDidDes { get; set; }
        public int? linguaDidId { get; set; }
        public string partEffCod { get; set; }
        public string partEffDes { get; set; }
        public string sedeDes { get; set; }
        public string sedeDesEng { get; set; }
        public int? sedeId { get; set; }
        public string tipoDidCod { get; set; }
        public string tipoDidDes { get; set; }
        public string tipoDidDesEng { get; set; }
    }


}
