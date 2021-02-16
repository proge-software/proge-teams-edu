using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Proge.Teams.Edu.Esse3.Models;


namespace Proge.Teams.Edu.Esse3
{
    /// <summary>
    /// Esse3 HTTP Client interface
    /// </summary>
    public interface IEsse3Client
    {
        /// <summary>
        /// Offerta didattica dei corsi di studio in un dato anno.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <returns></returns>
        Task<IEnumerable<Offerta>> GetOfferte(int aaOffId);
        /// <summary>
        /// Attività didattiche esposte nell'offerta didattica di un dato anno per un dato corso di studio.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <returns></returns>
        Task<IEnumerable<ADContestualizzata>> GetADOfferta(int aaOffId, Int64 cdsOffId);
        /// <summary>
        /// Recupera le informazioni sulle Ablitazioni dei docenti.
        /// </summary>
        /// <param name="cdsAbilId">Id del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="cdsAbilCod">Codice del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="adAbilId">Id dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="adAbilCod">Codice dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="aaOffAbilId">Anno di definizione dell'esame comune.</param>
        /// <returns></returns>
        Task<IEnumerable<AbilitazioniDocente>> GetAbilitazioni(Int64? cdsAbilId = null, string cdsAbilCod = null, Int64? adAbilId = null, string adAbilCod = null, int? aaOffAbilId = null);
        /// <summary>
        /// Restituisce la lista degli appelli
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id dell'attività didattica di erogazione dell'appello</param>
        /// <param name="aaCalId">anno di definizione del calendario esami</param>
        /// <returns></returns>
        Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string adId, string aaCalId = null, DateTime? minDate = null);
        Task<AppelloCustom> GetAppello(int cdsId, int addId, int appId);
        Task<IEnumerable<AppelloIscritto>> GetAppelloIscritti(int cdsId, int adId, int appId);
        /// <summary>
        /// Recupera le sessioni associate all'appello
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">progressivo dell'appello all'interno della coppa CDS/AD di definizione; è parte della chiave (cds_id,ad_id,app_id)</param>
        /// <returns></returns>
        Task<IEnumerable<Sessione>> GetSessioniAppello(long cdsId, long adId, long appId);
        /// <summary>
        /// Recupera i turni associati all'appello
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">progressivo dell'appello all'interno della coppa CDS/AD di definizione; è parte della chiave (cds_id,ad_id,app_id)</param>
        /// <returns></returns>
        Task<IEnumerable<Turno>> GetTurniAppello(long cdsId, long adId, long appId);
        Task<IEnumerable<AppelloCommissione>> GetAppelloCommissione(int cdsId, int adId, int appId);
        Task<AppelloCustom> GetAppelloIscrittiPrenotazioneStudente(int cdsId, int adId, int appId, int stuId);
        Task<IEnumerable<Docente>> GetDocente(int docenteId);
        Task<Persona> GetPersona(string personaId);
        Task<AppelloCustom> GetSessione(int aaSesId);
        Task<IEnumerable<Utente>> GetUtente(string personaId);
        /// <summary>
        /// Dettagli della logistica per anno di offerta, corso di studio, ordinamento, percorso di studio ed attività didattica.
        /// </summary>
        /// <param name="aaOffId">Id dell'anon di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <param name="aaOrdId">Id dell'ordinamento.</param>
        /// <param name="pdsId">Id del percorso di studio.</param>
        /// <param name="adId">Id dell'attività didattica.</param>
        /// <returns></returns>
        Task<IEnumerable<DettaglioLogistica>> GetDettagliLogistica(int aaOffId, Int64 cdsOffId, int? aaOrdId = null, Int64? pdsId = null, Int64? adId = null, string adCod = null);
        /// <summary>
        /// Tratti carriera che contengono libretti.
        /// </summary>
        /// <param name="cdsStuId">Id del corso di studio di appartenenza dello studente.</param>
        /// <param name="staStuCod">Codice dello stato della carriera a cui si riferisce il libretto.</param>
        /// <returns></returns>
        Task<IEnumerable<TrattoCarriera>> GetTrattiCarriera(Int64? cdsStuId = null, string staStuCod = null);
        /// <summary>
        /// Tutte le attività del libretto del tratto di carriera selezionato.
        /// </summary>
        /// <param name="matId">Id del tratto di carriera per cui recuperare il libretto.</param>
        /// <returns></returns>
        Task<IEnumerable<RigaLibretto>> GetDettaglioADStudente(Int64 matId);
        Task<Login> Login();
        Task Logout();
    }

    /// <summary>
    /// Esse3 HTTP Client
    /// </summary>
    public class Esse3Client : IEsse3Client
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Esse3Settings _esse3Settings;
        private readonly IRetryManager _retryManager;
        private string jSession { get; set; }

        public Esse3Client(IOptions<Esse3Settings> unimoresettings, IRetryManager retryManager)
        {
            _esse3Settings = unimoresettings.Value;
            _retryManager = retryManager;
        }

        public async Task<Login> Login()
        {
            string loginUrl = "login";
            string url = $"{_esse3Settings.WsBaseUrl}/api/{loginUrl}";
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<Login>(responseBody, DefaultSerializerOption);
                jSession = result.authToken;
                return result;
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task Logout()
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/logout";
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);            
        }

        #region Offerta V1
        /// <summary>
        /// Offerta didattica dei corsi di studio in un dato anno.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Offerta>> GetOfferte(int aaOffId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/offerta-service-v1/offerte";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Offerta>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Attività didattiche esposte nell'offerta didattica di un dato anno per un dato corso di studio.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ADContestualizzata>> GetADOfferta(int aaOffId, Int64 cdsOffId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/offerta-service-v1/offerte/{aaOffId}/{cdsOffId}/attivita";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<ADContestualizzata>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        #endregion

        #region Calesa V1
        /// <summary>
        /// Recupera le informazioni sulle Ablitazioni dei docenti.
        /// </summary>
        /// <param name="cdsAbilId">Id del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="cdsAbilCod">Codice del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="adAbilId">Id dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="adAbilCod">Codice dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="aaOffAbilId">Anno di definizione dell'esame comune.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AbilitazioniDocente>> GetAbilitazioni(Int64? cdsAbilId = null, string cdsAbilCod = null, Int64? adAbilId = null, string adAbilCod = null, int? aaOffAbilId = null)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/abilitazioni";

            var requestMessage = new HttpRequestMessage();

            List<string> qString = new List<string>();
            if (cdsAbilId.HasValue)
                qString.Add($"cdsAbilId={cdsAbilId.Value}");
            if (!string.IsNullOrWhiteSpace(cdsAbilCod))
                qString.Add($"cdsAbilCod={cdsAbilCod}");
            if (adAbilId.HasValue)
                qString.Add($"adAbilId={adAbilId.Value}");
            if (!string.IsNullOrWhiteSpace(adAbilCod))
                qString.Add($"adAbilCod={adAbilCod}");
            if (aaOffAbilId.HasValue)
                qString.Add($"aaOffAbilId={aaOffAbilId.Value}");

            HttpResponseMessage response;
            if (qString.Any())
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());
            else
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<AbilitazioniDocente>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Restituisce la lista degli appelli
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id dell'attività didattica di erogazione dell'appello</param>
        /// <param name="aaCalId">anno di definizione del calendario esami</param>
        /// <returns></returns>
        public async Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string adId, string aaCalId = null, DateTime? mindate = null)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}";
            string queryString = $"aaCalId={aaCalId}";
            if (mindate.HasValue)
                queryString += $"&minDataApp={mindate.Value.ToString("dd/MM/yyyy")}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, queryString);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<AppelloCustom>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {                
                throw ex;
            }
        }
        
        public async Task<IEnumerable<Sessione>> GetSessioniAppello(long cdsId, long adId, long appId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/sessioni";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Sessione>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {                
                throw ex;
            }
        }

        public async Task<IEnumerable<Turno>> GetTurniAppello(long cdsId, long adId, long appId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/turni";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Turno>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {                
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cdsId"></param>
        /// <param name="addId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<AppelloCustom> GetAppello(int cdsId, int addId, int appId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{addId}/{appId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<AppelloCustom>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<AppelloCustom> GetSessione(int aaSesId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/sessioni/{aaSesId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<AppelloCustom>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<IEnumerable<AppelloIscritto>> GetAppelloIscritti(int cdsId, int adId, int appId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/iscritti";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession, "attoreCod=DOC");

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<AppelloIscritto>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<IEnumerable<AppelloCommissione>> GetAppelloCommissione(int cdsId, int adId, int appId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/comm";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<AppelloCommissione>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<AppelloCustom> GetAppelloIscrittiPrenotazioneStudente(int cdsId, int adId, int appId, int stuId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/iscritti/{stuId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<AppelloCustom>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        #endregion

        #region Anagrafica v2
        public async Task<IEnumerable<Docente>> GetDocente(int docenteId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/docenti/{docenteId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Docente>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<Persona> GetPersona(string personaId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/persone/{personaId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<Persona>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<IEnumerable<Utente>> GetUtente(string personaId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/utenti/{personaId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Utente>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        #endregion

        #region Logistica V1
        /// <summary>
        /// Dettagli della logistica per anno di offerta, corso di studio, ordinamento, percorso di studio ed attività didattica.
        /// </summary>
        /// <param name="aaOffId">Id dell'anon di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <param name="aaOrdId">Id dell'ordinamento.</param>
        /// <param name="pdsId">Id del percorso di studio.</param>
        /// <param name="adId">Id dell'attività didattica.</param>
        /// <returns></returns>
        public async Task<IEnumerable<DettaglioLogistica>> GetDettagliLogistica(int aaOffId, Int64 cdsOffId,
            int? aaOrdId = null, Int64? pdsId = null, Int64? adId = null, string adCod = null)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/logistica-service-v1/logisticaPerOdFull/{aaOffId}/{cdsOffId}/";

            List<string> qString = new List<string>();
            if (aaOrdId.HasValue)
                qString.Add($"aaOrdId={aaOrdId.Value}");
            if (pdsId.HasValue)
                qString.Add($"pdsId={pdsId.Value}");
            if (adId.HasValue)
                qString.Add($"adId={adId.Value}");
            if (!string.IsNullOrWhiteSpace(adCod))
                qString.Add($"adCod={adCod}");

            HttpResponseMessage response;
            if (qString.Any())
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession, qString.ToArray());
            else
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<DettaglioLogistica>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        #endregion

        #region Libretto V2
        /// <summary>
        /// Tratti carriera che contengono libretti.
        /// </summary>
        /// <param name="cdsStuId">Id del corso di studio di appartenenza dello studente.</param>
        /// <param name="staStuCod">Codice dello stato della carriera a cui si riferisce il libretto.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TrattoCarriera>> GetTrattiCarriera(Int64? cdsStuId = null, string staStuCod = null)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/libretto-service-v2/libretti/";

            List<string> qString = new List<string>();
            if (cdsStuId.HasValue)
                qString.Add($"cdsStuId={cdsStuId.Value}");
            if (!string.IsNullOrWhiteSpace(staStuCod))
                qString.Add($"staStuCod={staStuCod}");

            HttpResponseMessage response;
            if (qString.Any())
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession, qString.ToArray());
            else
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<TrattoCarriera>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Tutte le attività del libretto del tratto di carriera selezionato.
        /// </summary>
        /// <param name="matId">Id del tratto di carriera per cui recuperare il libretto.</param>
        /// <returns></returns>
        public async Task<IEnumerable<RigaLibretto>> GetDettaglioADStudente(Int64 matId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/libretto-service-v2/libretti/{matId}/righe";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url,  jSession);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<RigaLibretto>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        #endregion

        private HttpRequestMessage RequestMessageFactory(HttpMethod httpMethod, string url, params string[] queryString)
        {
            var authenticationString = $"{_esse3Settings.Username}:{_esse3Settings.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(httpMethod, PostPendSessionId(url, queryString));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            return requestMessage;
        }

        private static JsonSerializerOptions DefaultSerializerOption = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        private string PostPendSessionId(string url, params string[] queryString)
        {
            return $"{url}/;{jSession}{(queryString == null || !queryString.Any() ? string.Empty : "?" + string.Join("&", queryString))}";
        }
    }
}
