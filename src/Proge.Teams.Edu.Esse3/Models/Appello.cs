using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Proge.Teams.Edu.Esse3.Models
{
    #region Basato su docs swagger
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TipoGestPren
    {
        public string tipoGestPrenCod { get; set; }
        public string des { get; set; }
        public int listaStudentiFlg { get; set; }
        public int regAppFlg { get; set; }
        public int chkCancPren { get; set; }
    }

    public class TipoGestPrenAttore
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Config
    {
        public TipoGestPren tipoGestPren { get; set; }
        public List<TipoGestPrenAttore> tipoGestPrenAttore { get; set; }
    }

    public class Appello
    {
        public Config config { get; set; }
        public int datacalId { get; set; }
        public int capostipiteId { get; set; }
        public int commPianId { get; set; }
        public int indexId { get; set; }
        public int periodoId { get; set; }
        public int numVerbaliGen { get; set; }
        public int numVerbaliCar { get; set; }
        public int numPubblicazioni { get; set; }
        public int numIscritti { get; set; }
        public string statoLog { get; set; }
        public string statoAperturaApp { get; set; }
        public string statoVerb { get; set; }
        public string statoPubblEsiti { get; set; }
        public string statoInsEsiti { get; set; }
        public string statoDes { get; set; }
        public string stato { get; set; }
        public string presidenteNome { get; set; }
        public string presidenteCognome { get; set; }
        public int presidenteId { get; set; }
        public string tipoGestPrenDes { get; set; }
        public string tipoGestAppDes { get; set; }
        public string tipoDefAppDes { get; set; }
        public string adDes { get; set; }
        public string adCod { get; set; }
        public string cdsDes { get; set; }
        public string cdsCod { get; set; }
        public string tipoAppCod { get; set; }
        public int appId { get; set; }
        public int appelloId { get; set; }
        public int aaCalId { get; set; }
        public string noteSistLog { get; set; }
        public string note { get; set; }
        public int tagTemplId { get; set; }
        public int sedeId { get; set; }
        public int gruppoVotoId { get; set; }
        public int condId { get; set; }
        public int tipoSceltaTurno { get; set; }
        public int riservatoFlg { get; set; }
        public string dataInizioApp { get; set; }
        public string dataFineIscr { get; set; }
        public string dataInizioIscr { get; set; }
        public string tipoEsaCod { get; set; }
        public string tipoIscrCod { get; set; }
        public string tipoGestPrenCod { get; set; }
        public string tipoGestAppCod { get; set; }
        public string tipoDefAppCod { get; set; }
        public string desApp { get; set; }
        public int adId { get; set; }
        public int cdsId { get; set; }
    }
    #endregion

    #region Basato su esempio call
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class StatoInsEsiti
    {
        public string value { get; set; }
    }

    public class StatoPubblEsiti
    {
        public string value { get; set; }
    }

    public class StatoVerb
    {
        public string value { get; set; }
    }

    public class TipoEsaCod
    {
        public string value { get; set; }
    }

    public class TipoIscrCod
    {
        public string value { get; set; }
    }

    public class AppelloCustom
    {
        public int aaCalId { get; set; }
        public string adCod { get; set; }
        public string adDes { get; set; }
        public int adId { get; set; }
        public int appId { get; set; }
        public int appelloId { get; set; }
        public string cdsCod { get; set; }
        public string cdsDes { get; set; }
        public int cdsId { get; set; }
        public int? condId { get; set; }
        public string dataFineIscr { get; set; }
        public string dataInizioApp { get; set; }
        public string dataInizioIscr { get; set; }
        public string desApp { get; set; }
        public string note { get; set; }
        public int numIscritti { get; set; }
        public int numPubblicazioni { get; set; }
        public int numVerbaliCar { get; set; }
        public int numVerbaliGen { get; set; }
        public string presidenteCognome { get; set; }
        public int? presidenteId { get; set; }
        public string presidenteNome { get; set; }
        public int riservatoFlg { get; set; }
        public string stato { get; set; }
        public string statoAperturaApp { get; set; }
        public string statoDes { get; set; }
        public StatoInsEsiti statoInsEsiti { get; set; }
        public string statoLog { get; set; }
        public StatoPubblEsiti statoPubblEsiti { get; set; }
        public StatoVerb statoVerb { get; set; }
        public string tipoAppCod { get; set; }
        public string tipoDefAppCod { get; set; }
        public string tipoDefAppDes { get; set; }
        public TipoEsaCod tipoEsaCod { get; set; }
        public string tipoGestAppCod { get; set; }
        public string tipoGestAppDes { get; set; }
        public string tipoGestPrenCod { get; set; }
        public string tipoGestPrenDes { get; set; }
        public TipoIscrCod tipoIscrCod { get; set; }
        public int tipoSceltaTurno { get; set; }

        [JsonIgnore]
        public DateTime? DataInizioApp
        {
            get
            {
                if (DateTime.TryParse(dataInizioApp, System.Globalization.CultureInfo.GetCultureInfo("it-IT"),
                    System.Globalization.DateTimeStyles.None, out DateTime dataapp))
                    return dataapp;
                else
                    return null;

            }
        }
    }
    #endregion


}
