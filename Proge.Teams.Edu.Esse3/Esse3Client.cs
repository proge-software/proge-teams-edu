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
        /// Restituisce la lista degli appelli
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="addId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">anno di definizione del calendario esami</param>
        /// <returns></returns>
        Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string addId, string aaCalId = null);
        Task<AppelloCustom> GetAppello(int cdsId, int addId, int appId);
        Task<IEnumerable<AppelloIscritto>> GetAppelloIscritti(int cdsId, int adId, int appId);
        Task<IEnumerable<AppelloCommissione>> GetAppelloCommissione(int cdsId, int adId, int appId);
        Task<AppelloCustom> GetAppelloIscrittiPrenotazioneStudente(int cdsId, int adId, int appId, int stuId);
        Task<IEnumerable<Docente>> GetDocente(int docenteId);
        Task<Persona> GetPersona(string personaId);
        Task<AppelloCustom> GetSessione(int aaSesId);
        Task<IEnumerable<Utente>> GetUtente(string personaId);
        Task<Login> Login();
    }

    /// <summary>
    /// Esse3 HTTP Client
    /// </summary>
    public class Esse3Client : IEsse3Client
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Esse3Settings _esse3Settings;
        private string jSession { get; set; }

        public Esse3Client(IOptions<Esse3Settings> unimoresettings)
        {
            _esse3Settings = unimoresettings.Value;
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

        #region Calesa
        /// <summary>
        /// Restituisce la lista degli appelli
        /// </summary>
        /// <param name="cdsId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="addId">id del corso di studio di erogazione dell'appello</param>
        /// <param name="aaCalId">anno di definizione del calendario esami</param>
        /// <returns></returns>
        public async Task<IEnumerable<AppelloCustom>> GetAppelli(string cdsId, string addId, string aaCalId = null)
        {
            string url = $"{_esse3Settings.WsBaseUrl}/api/calesa-service-v1/appelli/{cdsId}/{addId}";
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url, $"aaCalId={aaCalId}");
            var response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<AppelloCustom>>(res, DefaultSerializerOption);
            }
            catch (JsonException ex) // Invalid JSON
            {
                //Console.WriteLine(ex.ToString());
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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url, "attoreCod=DOC");
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
            var requestMessage = RequestMessageFactory(HttpMethod.Get, url);
            var response = await client.SendAsync(requestMessage);

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
