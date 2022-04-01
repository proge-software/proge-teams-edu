using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Proge.Teams.Edu.Esse3.Models;
using System.Threading;

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
        Task<IEnumerable<Offerta>> GetOfferte(int aaOffId, int? start = 0, int? limit = 100);
        /// <summary>
        /// Attività didattiche esposte nell'offerta didattica di un dato anno per un dato corso di studio.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <returns></returns>
        Task<IEnumerable<ADContestualizzata>> GetADOfferta(int aaOffId, Int64 cdsOffId, int? start = 0, int? limit = 100);
        /// <summary>
        /// Recupera le informazioni sulle Ablitazioni dei docenti.
        /// </summary>
        /// <param name="cdsAbilId">Id del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="cdsAbilCod">Codice del corso di studio di erogazione dell'esame comune.</param>
        /// <param name="adAbilId">Id dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="adAbilCod">Codice dell'attività didattica di erogazione dell'esame comune.</param>
        /// <param name="aaOffAbilId">Anno di definizione dell'esame comune.</param>
        /// <returns></returns>
        Task<IEnumerable<AbilitazioniDocente>> GetAbilitazioni(Int64? cdsAbilId = null, string cdsAbilCod = null, Int64? adAbilId = null, string adAbilCod = null, int? aaOffAbilId = null, int? start = 0, int? limit = 100);
        /// <summary>
        /// Restituisce la lista degli appelli
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id dell'attività didattica di erogazione dell'appello</param>
        /// <param name="aaCalId">anno di definizione del calendario esami</param>
        /// <returns></returns>
        Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string adId, string aaCalId = null, DateTime? minDate = null, DateTime? maxdate = null, int? start = 0, int? limit = 100);
        Task<AppelloCustom> GetAppello(int cdsId, int addId, int appId);
        Task<IEnumerable<AppelloIscritto>> GetAppelloIscritti(int cdsId, int adId, int appId, int? start = 0, int? limit = 100);
        /// <summary>
        /// Recupera le sessioni associate all'appello
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">progressivo dell'appello all'interno della coppa CDS/AD di definizione; è parte della chiave (cds_id,ad_id,app_id)</param>
        /// <returns></returns>
        Task<IEnumerable<Sessione>> GetSessioniAppello(long cdsId, long adId, long appId, int? start = 0, int? limit = 100);
        /// <summary>
        /// Recupera i turni associati all'appello
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="adId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">progressivo dell'appello all'interno della coppa CDS/AD di definizione; è parte della chiave (cds_id,ad_id,app_id)</param>
        /// <returns></returns>
        Task<IEnumerable<Turno>> GetTurniAppello(long cdsId, long adId, long appId, int? start = 0, int? limit = 100);
        Task<IEnumerable<AppelloCommissione>> GetAppelloCommissione(int cdsId, int adId, int appId, int? start = 0, int? limit = 100);
        Task<AppelloCustom> GetAppelloIscrittiPrenotazioneStudente(int cdsId, int adId, int appId, int stuId);
        Task<IEnumerable<Docente>> GetDocente(long docenteId, int? start = 0, int? limit = 100);
        Task<IEnumerable<Docente>> GetDocente(int docenteId, int? start = 0, int? limit = 100);
        Task<IEnumerable<Persona>> GetPersone(string codiceFiscale = null, string cognome = null, string nome = null, int? start = 0, int? limit = 100);
        Task<Persona> GetPersona(string personaId);
        Task<AppelloCustom> GetSessione(int aaSesId);
        Task<IEnumerable<Utente>> GetUtente(string personaId, int? start = 0, int? limit = 100);
        /// <summary>
        /// Dettagli della logistica per anno di offerta, corso di studio, ordinamento, percorso di studio ed attività didattica.
        /// </summary>
        /// <param name="aaOffId">Id dell'anon di offerta.</param>
        /// <param name="cdsOffId">Id del corso di studio.</param>
        /// <param name="aaOrdId">Id dell'ordinamento.</param>
        /// <param name="pdsId">Id del percorso di studio.</param>
        /// <param name="adId">Id dell'attività didattica.</param>
        /// <returns></returns>
        Task<IEnumerable<DettaglioLogistica>> GetDettagliLogistica(int aaOffId, Int64 cdsOffId, int? aaOrdId = null, Int64? pdsId = null, Int64? adId = null, string adCod = null, int? start = 0, int? limit = 100);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adId"></param>
        /// <returns></returns>
        Task<Logistica> GetLogistica(long adId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Tratti carriera che contengono libretti.
        /// </summary>
        /// <param name="cdsStuId">Id del corso di studio di appartenenza dello studente.</param>
        /// <param name="staStuCod">Codice dello stato della carriera a cui si riferisce il libretto.</param>
        /// <returns></returns>
        Task<IEnumerable<TrattoCarriera>> GetTrattiCarriera(Int64? cdsStuId = null, string staStuCod = null, int? start = 0, int? limit = 100);
        /// <summary>
        /// Tutte le attività del libretto del tratto di carriera selezionato.
        /// </summary>
        /// <param name="matId">Id del tratto di carriera per cui recuperare il libretto.</param>
        /// <returns></returns>
        Task<IEnumerable<RigaLibretto>> GetDettaglioADStudente(Int64 matId, int? start = 0, int? limit = 100);
        Task<IEnumerable<Logistica>> GetAllLogistiche(int? aaOffId, uint limit = 100);
        Task<Login> Login(CancellationToken cancellationToken = default);
        Task Logout();
        Task<IEnumerable<Persona>> GetClasseStudenti(long adLogId, string adCod, string cdsStuCod, int? start = 0, int? limit = 100);
    }

    /// <summary>
    /// Esse3 HTTP Client
    /// </summary>
    public class Esse3Client : IEsse3Client
    {
        private static readonly HttpClient client = new();
        private readonly Esse3Settings _esse3Settings;
        private readonly IRetryManager _retryManager;
        private string jSession { get; set; }

        public Esse3Client(IOptions<Esse3Settings> esse3Settings, IRetryManager retryManager)
        {
            _esse3Settings = esse3Settings.Value;
            _retryManager = retryManager;
        }

        public async Task<Login> Login(CancellationToken cancellationToken = default)
        {
            string loginUrl = "login";
            string url = $"{_esse3Settings.WsBaseUrl}/api/{loginUrl}";
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage, cancellationToken);
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
            await client.SendAsync(requestMessage);
        }

        #region Offerta V1
        /// <summary>
        /// Offerta didattica dei corsi di studio in un dato anno.
        /// </summary>
        /// <param name="aaOffId">Id dell'anno di offerta.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Offerta>> GetOfferte(int aaOffId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/offerta-service-v1/offerte";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            qString.Add($"aaOffId={aaOffId}");
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
        public async Task<IEnumerable<ADContestualizzata>> GetADOfferta(int aaOffId, Int64 cdsOffId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/offerta-service-v1/offerte/{aaOffId}/{cdsOffId}/attivita";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
        public async Task<IEnumerable<AbilitazioniDocente>> GetAbilitazioni(Int64? cdsAbilId = null, string cdsAbilCod = null, Int64? adAbilId = null, string adAbilCod = null, int? aaOffAbilId = null, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/abilitazioni";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
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
        public async Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string adId, string aaCalId = null, DateTime? mindate = null, DateTime? maxdate = null, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}", $"aaCalId={aaCalId}" };
            if (mindate.HasValue)
                qString.Add($"&minDataApp={mindate.Value:dd/MM/yyyy}");
            if (maxdate.HasValue)
                qString.Add($"&maxDataApp={maxdate.Value:dd/MM/yyyy})");
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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

        public async Task<IEnumerable<Sessione>> GetSessioniAppello(long cdsId, long adId, long appId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/sessioni";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());
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

        public async Task<IEnumerable<Turno>> GetTurniAppello(long cdsId, long adId, long appId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/turni";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

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
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

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

        public async Task<IEnumerable<AppelloIscritto>> GetAppelloIscritti(int cdsId, int adId, int appId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/iscritti";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            qString.Add("attoreCod=DOC");
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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

        public async Task<IEnumerable<AppelloCommissione>> GetAppelloCommissione(int cdsId, int adId, int appId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{adId}/{appId}/comm";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

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
        public async Task<IEnumerable<Docente>> GetDocente(int docenteId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/docenti/{docenteId}";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Docente>>(res, DefaultSerializerOption);
        }

        public async Task<IEnumerable<Docente>> GetDocente(long docenteId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/docenti/{docenteId}";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Docente>>(res, DefaultSerializerOption);
        }

        public async Task<Persona> GetPersona(string personaId)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/persone/{personaId}";
            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

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

        public async Task<IEnumerable<Persona>> GetPersone(string codiceFiscale = null, string cognome = null, string nome = null, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/persone";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(cognome))
                qString.Add($"cognome={cognome}");
            if (!string.IsNullOrWhiteSpace(nome))
                qString.Add($"nome={nome}");
            if (!string.IsNullOrWhiteSpace(codiceFiscale))
                qString.Add($"codFis={codiceFiscale}");

            HttpResponseMessage response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<IEnumerable<Persona>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task<IEnumerable<Utente>> GetUtente(string personaId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/anagrafica-service-v2/utenti/{personaId}";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };

            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
            int? aaOrdId = null, Int64? pdsId = null, Int64? adId = null, string adCod = null, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/logistica-service-v1/logisticaPerOdFull/{aaOffId}/{cdsOffId}/";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
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
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());
            else
                response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession);

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

        public async Task<IEnumerable<Logistica>> GetAllLogistiche(int? aaOffId, uint limit = 100)
        {
            if (limit == 0)
                throw new ArgumentException("'limit' must be a positive integer");

            List<IEnumerable<Logistica>> ll = new List<IEnumerable<Logistica>>();
            IEnumerable<Logistica> l = Enumerable.Empty<Logistica>();
            uint cursor = 0;
            do
            {
                l = await GetLogistica(aaOffId, cursor, limit);
                ll.Add(l);
                cursor += limit;
            } while (l.Count() == limit);

            IEnumerable<Logistica> logistiche = ll.SelectMany(x => x);
            return logistiche;
        }

        public async Task<Logistica> GetLogistica(long adId, CancellationToken cancellationToken = default)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/logistica-service-v1/logistica";
            var qString = new string[1] { $"adId={adId}" };

            HttpResponseMessage response = await _retryManager
                .DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString, cancellationToken);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            var logistiche = JsonSerializer.Deserialize<IEnumerable<Logistica>>(res, DefaultSerializerOption);
            return logistiche.FirstOrDefault();
        }
        private async Task<IEnumerable<Logistica>> GetLogistica(int? aaOffId = null, uint start = 0, uint limit = 100
            //Int64? cdsOffId= null,int? aaOrdId = null, Int64? pdsId = null, Int64? adId = null, string adCod = null
            )
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/logistica-service-v1/logistica";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            if (aaOffId.HasValue)
                qString.Add($"aaOffId={aaOffId.Value}");

            HttpResponseMessage response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Logistica>>(res, DefaultSerializerOption);
        }
        #endregion

        #region Libretto V2
        /// <summary>
        /// Tratti carriera che contengono libretti.
        /// </summary>
        /// <param name="cdsStuId">Id del corso di studio di appartenenza dello studente.</param>
        /// <param name="staStuCod">Codice dello stato della carriera a cui si riferisce il libretto.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TrattoCarriera>> GetTrattiCarriera(Int64? cdsStuId = null, string staStuCod = null, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/libretto-service-v2/libretti/";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            if (cdsStuId.HasValue)
                qString.Add($"cdsStuId={cdsStuId.Value}");
            if (!string.IsNullOrWhiteSpace(staStuCod))
                qString.Add($"staStuCod={staStuCod}");

            HttpResponseMessage response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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
        public async Task<IEnumerable<RigaLibretto>> GetDettaglioADStudente(Int64 matId, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/libretto-service-v2/libretti/{matId}/righe";
            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };

            var response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

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

        /// <summary>
        /// Lista degli studenti iscritti a una classe
        /// </summary>
        /// <param name="adLogId">id univoco della condivisione logistica</param>
        /// <param name="adCod">codice attività della riga di libretto da ricercare</param>
        /// <param name="cdsStuCod">codice del corso di studio di appartenenza dello studente</param>
        /// <returns></returns>
        public async Task<IEnumerable<Persona>> GetClasseStudenti(long adLogId, string adCod, string cdsStuCod, int? start = 0, int? limit = 100)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/libretto-service-v2/libretti/classe-studenti/{adLogId}";

            List<string> qString = new List<string> { $"start={start}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(adCod))
                qString.Add($"adCod={adCod}");
            if (!string.IsNullOrWhiteSpace(cdsStuCod))
                qString.Add($"cdsStuCod={cdsStuCod}");

            HttpResponseMessage response = await _retryManager.DoSendAsyncRequest<HttpResponseMessage>(client, HttpMethod.Get, url, jSession, qString.ToArray());

            response.EnsureSuccessStatusCode();

            string res = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Persona>>(res, DefaultSerializerOption);
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

        private readonly static JsonSerializerOptions DefaultSerializerOption = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private string PostPendSessionId(string url, params string[] queryString)
        {
            return $"{url}/;{jSession}{(queryString == null || !queryString.Any() ? string.Empty : "?" + string.Join("&", queryString))}";
        }
    }
}
