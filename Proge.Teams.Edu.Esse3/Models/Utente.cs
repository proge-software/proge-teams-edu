using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{   


   
    public class Utente
    {
        public int adId { get; set; }
        public string adStuCod { get; set; }
        public int adregId { get; set; }
        public int adsceId { get; set; }
        public int appId { get; set; }
        public int appLogId { get; set; }
        public int applistaId { get; set; }
        public string cdsAdStuCod { get; set; }
        public int cdsId { get; set; }
        public string cdsStuCod { get; set; }
        public string codFisStudente { get; set; }
        public string cognomeStudente { get; set; }
        public string dataEsa { get; set; }
        public string dataIns { get; set; }
        public string dataNascitaStudente { get; set; }
        public string dataRifEsitoStu { get; set; }
        public string domandeEsame { get; set; }
        public Esito esito { get; set; }
        public string gruppoVotoCod { get; set; }
        public int gruppoVotoMaxPunti { get; set; }
        public string matricola { get; set; }
        public string nomeStudente { get; set; }
        public double pesoAd { get; set; }
        public PresaVisione presaVisione { get; set; }
        public int? pubblId { get; set; }
        public StatoAdsce statoAdsce { get; set; }
        public int stuId { get; set; }
        public string userId { get; set; }
    }
}
