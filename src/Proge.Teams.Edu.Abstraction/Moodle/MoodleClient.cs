using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Proge.Teams.Edu.Abstraction.Moodle
{
    public interface IBaseMoodleClient
    {
        Task<TokenResponse> GetMoodleToken(string baseUrl, string username, string password, string service, CancellationToken cancellationToken = default);
        Task<CalendarEventResponse> CreateOrUpdateCalendarEvent(CreateEventFormData createEventPayload, CancellationToken cancellationToken = default);
        Task DeleteCalendarEvent(int eventId, CancellationToken cancellationToken = default);
        Task<CalendarEventResponse> GetCalendarEvent(int eventId, CancellationToken cancellationToken = default);
        Task<CalendarEventsResponse> GetCalendarEventByDate(DateTime date, CancellationToken cancellationToken = default);
    }

    public class MoodleClient : IBaseMoodleClient
    {
        protected static readonly HttpClient unpClient = new(new HttpClientHandler() { SslProtocols = SslProtocols.Tls12 });
        protected readonly string token;
        protected readonly string baseurl;
        public MoodleClient(string baseUrl, string username, string password, string service)
        {
            baseurl = baseUrl;
            var tokenResponse = GetMoodleToken(baseUrl, username, password, service).Result;
            if (string.IsNullOrEmpty(tokenResponse.token))
                throw new UnauthorizedAccessException($"Moodle Login failed for user {username}. Error info: {JsonSerializer.Serialize(tokenResponse.exception)}");
            token = tokenResponse.token;
        }

        public MoodleClient()
        {

        }

        public async Task<CalendarEventResponse> CreateOrUpdateCalendarEvent(CreateEventFormData createEventPayload, CancellationToken cancellationToken = default)
        {
            string url = $"{baseurl}webservice/rest/server.php?wstoken={token}&moodlewsrestformat=json&wsfunction=core_calendar_submit_create_update_form&moodlewssettingfilter=true&moodlewssettingfileurl=true";
            url += "&formdata=" + createEventPayload.Serialize();
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
          
            using var ajaxResponde = await unpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
            ajaxResponde.EnsureSuccessStatusCode();
            var responseStream = await ajaxResponde.Content.ReadAsStringAsync().
                ConfigureAwait(false);
            var obj = JsonNode.Parse(responseStream);
            var ajaxException = obj["exception"];
            var eventResponse = new CalendarEventResponse();
            if (ajaxException != null)
            {
                eventResponse.validationerror = true;
                eventResponse.exception = JsonSerializer.Deserialize<MoodleExceptionData>(responseStream);
            }
            else
            {
                eventResponse = JsonSerializer.Deserialize<CalendarEventResponse>(responseStream);
            }

            return eventResponse;
        }

        public async Task DeleteCalendarEvent(int eventId, CancellationToken cancellationToken = default)
        {
            string url = $"{baseurl}webservice/rest/server.php?wstoken={token}&moodlewsrestformat=json&wsfunction=core_calendar_delete_calendar_events";
            url += $"&events[0][eventid]={eventId}";
            url += $"&events[0][repeat]=0";
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            using var ajaxResponse = await unpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            ajaxResponse.EnsureSuccessStatusCode();
            var responseStream = await ajaxResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JsonNode.Parse(responseStream);
        }

        public async Task<CalendarEventResponse> GetCalendarEvent(int eventId, CancellationToken cancellationToken = default)
        {
            string url = $"{baseurl}webservice/rest/server.php?wstoken={token}&moodlewsrestformat=json&wsfunction=core_calendar_get_calendar_event_by_id";
            url += $"&eventid={eventId}";
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            using var ajaxResponse = await unpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            ajaxResponse.EnsureSuccessStatusCode();
            var responseStream = await ajaxResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JsonNode.Parse(responseStream);

            var ajaxException = obj["exception"];
            var eventResponse = new CalendarEventResponse();
            if (ajaxException != null)
            {
                eventResponse.validationerror = true;
                eventResponse.exception = JsonSerializer.Deserialize<MoodleExceptionData>(responseStream);
            }
            else
            {
                eventResponse = JsonSerializer.Deserialize<CalendarEventResponse>(responseStream);
            }
            return eventResponse;
        }

        public async Task<CalendarEventsResponse> GetCalendarEventByDate(DateTime date, CancellationToken cancellationToken = default)
        {
            string url = $"{baseurl}webservice/rest/server.php?wstoken={token}&moodlewsrestformat=json&wsfunction=core_calendar_get_calendar_day_view";
            url += $"&year={date.Year}";
            url += $"&month={date.Month}";
            url += $"&day={date.Day}";
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            using var ajaxResponse = await unpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            ajaxResponse.EnsureSuccessStatusCode();
            var responseStream = await ajaxResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JsonNode.Parse(responseStream);

            var ajaxException = obj["exception"];
            var eventResponse = new CalendarEventsResponse();
            if (ajaxException != null)
            {
                eventResponse.validationerror = true;
                eventResponse.exception = JsonSerializer.Deserialize<MoodleExceptionData>(responseStream);
            }
            else
            {
                var ajaxEvents = obj["events"];
                eventResponse.Events = JsonSerializer.Deserialize<IEnumerable<CalendarEvent>>(ajaxEvents);
            }
            return eventResponse;
        }

        public virtual async Task<TokenResponse> GetMoodleToken(string baseUrl, string username, string password, string service = "moodle_mobile_app", CancellationToken cancellationToken = default)
        {
            string tokenurl = $"{baseUrl}login/token.php?username={username}&password={password}&service={service}";            
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(tokenurl));
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");

            using var ajaxResponse = await unpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            ajaxResponse.EnsureSuccessStatusCode();
            var responseStream = await ajaxResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JsonNode.Parse(responseStream);
            var token = obj["token"];            

            var tokenResponse = new TokenResponse();
            if (token == null)
            {
                tokenResponse.validationerror = true;
                tokenResponse.exception = JsonSerializer.Deserialize<MoodleExceptionData>(responseStream);
            }
            else
            {
                tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseStream);
            }

            return tokenResponse;
        }
    }
}
