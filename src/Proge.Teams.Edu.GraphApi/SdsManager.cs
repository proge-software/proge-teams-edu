extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;


namespace Proge.Teams.Edu.GraphApi
{
    public interface ISdsManager
    {
        Task<bool> ConnectWithUnp();
        Task<Beta.EducationSynchronizationProfile> CreateEduSyncProfile(Beta.EducationSynchronizationProfile syncProfile);
        Task DeleteEduSyncProfile(string syncProfileId);
        Task<Beta.EducationSynchronizationProfile> GetEduSyncProfile(string syncProfileId);
        Task<string> GetSyncProfileIdByDisplayName(string profileName);
        Task<IEnumerable<Beta.EducationSynchronizationProfile>> GetEduSyncProfiles();
        Task<Beta.EducationSynchronizationProfileStatus> GetEduSyncProfileStatus(string syncProfileId);
        Task<string> GetEduSyncProfileUploadURL(string syncProfileId);
        Task<IEnumerable<Beta.EducationSynchronizationError>> GetSyncErrors(string syncProfileId);
        Task PauseEduSyncProfile(string syncProfileId);
        Task ResetEduSyncProfile(string syncProfileId);
        Task ResumeEduSyncProfile(string syncProfileId);
        Task<(bool IsSuccess, string RetMessage)> StartEduSyncProfileSynchronization(string syncProfileId);
        Task<(bool IsSuccess, string RetMessage)> UploadCsvFiles(string uploadUrl, string[] fileFullPaths);
        Beta.EducationSynchronizationProfile DefaultSyncProfileFactory(string profileName, string newProfileIdentitySyncConfType);
    }

    public class SdsManager : ISdsManager
    {
        #region Variables and properties
        private string _unpToken { get; set; }

        private IPublicClientApplication unpApp { get; set; }
        private UsernamePasswordProvider authProvider { get; set; }
        private Beta.GraphServiceClient graphClient { get; set; }
        AuthenticationConfig _authenticationConfig { get; set; }

        private static DateTime dtFirstStart;
        private static DateTime dtNextConnRefresh;
        private static int connRefrSeconds = 600;

        private static readonly HttpClient unpClient = new HttpClient();
        #endregion

        public SdsManager(IOptions<AuthenticationConfig> authCfg)
        {
            dtFirstStart = DateTime.Now;
            
            //_mapper = mapper;
            _authenticationConfig = authCfg.Value;
            unpApp = PublicClientApplicationBuilder.Create(_authenticationConfig.ClientId)
                  .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
                  //.WithAuthority(config.Authority)                  
                  .Build();
            authProvider = new UsernamePasswordProvider(unpApp);
            graphClient = new Beta.GraphServiceClient(authProvider);
        }

        #region Public functions 
        /// <summary>
        /// Connect with username and pwd.
        /// </summary>
        /// <param name="config">Mapping of appsettings.json containing authentication input data.</param>
        /// <returns></returns>
        public async Task<bool> ConnectWithUnp()
        {
            try
            {
                dtNextConnRefresh = DateTime.Now.AddSeconds(connRefrSeconds);

                // Login Azure AD
                var accounts = await unpApp.GetAccountsAsync();

                AuthenticationResult authResult = null;
                if (accounts.Any())
                {
                    authResult = await unpApp.AcquireTokenSilent(_authenticationConfig.ScopeList, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                else
                {
                    try
                    {
                        authResult = await unpApp.AcquireTokenByUsernamePassword(_authenticationConfig.ScopeList, _authenticationConfig.Username, _authenticationConfig.SecurePassword)
                            .ExecuteAsync();
                    }
                    catch (MsalException ex)
                    {
                        throw ex;
                    }
                }

                // Setting client default request headers
                _unpToken = authResult.AccessToken;
                if (unpClient.DefaultRequestHeaders.Contains("Authentication"))
                {
                    unpClient.DefaultRequestHeaders.Remove("Authentication");
                }
                unpClient.DefaultRequestHeaders.Add("Authentication", $"Bearer {this._unpToken}");
                if (unpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    unpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                unpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this._unpToken}");

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create a request for a new school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfile">Object that represents the synchronization profile to create.</param>
        /// <returns>The new education synchronization profile (Microsoft.Graph.EducationSynchronizationProfile object).</returns>
        public async Task<Beta.EducationSynchronizationProfile> CreateEduSyncProfile(Beta.EducationSynchronizationProfile syncProfile)
        {
            try
            {
                await EnsureUnpToken();
                HttpContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(syncProfile), Encoding.Default, "application/json");
                //HttpResponseMessage response = await graphClient.PostAsync("https://graph.microsoft.com/beta/education/synchronizationProfiles", content);
                var addedSyncProfile = await graphClient.Education
                    .SynchronizationProfiles
                    .Request()
                    .AddAsync(syncProfile);
                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
                //response.EnsureSuccessStatusCode();

                return addedSyncProfile;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Retrieve the collection of school data synchronization profiles in the tenant.
        /// </summary>
        /// <returns>Collection of Microsoft.Graph.EducationSynchronizationProfile objects.</returns>
        public async Task<IEnumerable<Beta.EducationSynchronizationProfile>> GetEduSyncProfiles()
        {
            try
            {
                await EnsureUnpToken();
                //HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles");
                var response = await graphClient.Education.SynchronizationProfiles.Request().GetAsync();

                List<Beta.EducationSynchronizationProfile> profiles = response.ToList();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}

                return profiles;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Retrieve a school data synchronization profile in the tenant based on the identifier.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns>The education synchronization profile (Microsoft.Graph.EducationSynchronizationProfile object).</returns>
        public async Task<Beta.EducationSynchronizationProfile> GetEduSyncProfile(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}");
                Beta.EducationSynchronizationProfile profile = await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Request()
                    .GetAsync();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}

                return profile;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the id of a sync profile by DisplayName.
        /// </summary>
        /// <returns>The education synchronization profile id.</returns>
        public async Task<string> GetSyncProfileIdByDisplayName(string profileName)
        {
            string sId = string.Empty;
            //List<Beta.EducationSynchronizationProfile> profiles = await GetEduSyncProfiles();
            var profiles = await GetEduSyncProfiles();
            if (profiles.Any(p => p.DisplayName == profileName))
            {
                sId = profiles.Single(p => p.DisplayName == profileName).Id;
            }
            else
            {
                throw new Exception($"Sync profile with display name '{profileName}' not found.");
            }

            return sId;
        }

        /// <summary>
        /// Retrieve a shared access signature (SAS) for uploading source files to 
        /// Azure blob storage for a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns>The url where the csv files can be uploaded.</returns>
        public async Task<string> GetEduSyncProfileUploadURL(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/uploadUrl");

                string res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //return (true, res);
                    string sUrl = res.Substring(res.IndexOf("value") + 8);
                    sUrl = sUrl.Substring(0, sUrl.IndexOf("\""));
                    return sUrl;
                }
                else
                {
                    //return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                    throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                }
                // C# Type request: currently not working due to an sdk issue, a patch should be released
                //var sUrl = await graphClient.Education.SynchronizationProfiles[syncProfileId].UploadUrl().Request().GetAsync();
                //return sUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Upload files to a given url.
        /// </summary>
        /// <param name="uploadUrl">Url where the files must be upload.</param>
        /// <param name="fileFullPaths">List of the full local path of the files to upload (array of strings).</param>
        /// <returns>True with no message if the upload succesfully completes, false with an error message otherwise. </returns>
        public async Task<(bool IsSuccess, string RetMessage)> UploadCsvFiles(string uploadUrl, string[] fileFullPaths)
        {
            try
            {
                CloudBlobContainer container = new CloudBlobContainer(new Uri(uploadUrl));
                foreach (string filePath in fileFullPaths)
                {
                    string sFileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                    var blb = container.GetBlockBlobReference(sFileName);
                    await blb.UploadFromFileAsync(filePath);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Start a synchronization attempt.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns>True with a confirmation message if the sync attempt succesfully starts, false with a status code otherwise.</returns>
        public async Task<(bool IsSuccess, string RetMessage)> StartEduSyncProfileSynchronization(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpContent emptyContent = new StringContent(string.Empty, Encoding.Default, "application/json");
                //HttpResponseMessage response = await unpClient.PostAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/start", emptyContent);

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}

                var res = await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Start()
                    .Request()
                    .PostAsync();

                string statusCode = res.AdditionalData["statusCode"].ToString().Trim().ToUpper();
                if (statusCode == "OK")
                {
                    return (true, "Sync succesfully started");
                }
                else
                {
                    return (false, statusCode);
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the status of a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns>The status of the education sync profile (Microsoft.Graph.EducationSynchronizationProfileStatus object).</returns>
        public async Task<Beta.EducationSynchronizationProfileStatus> GetEduSyncProfileStatus(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/profileStatus");
                Beta.EducationSynchronizationProfileStatus status = await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .ProfileStatus
                    .Request()
                    .GetAsync();

                return status;

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete a school data synchronization profile in the tenant based on the identifier.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns></returns>
        public async Task DeleteEduSyncProfile(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpContent emptyContent = new StringContent(string.Empty, Encoding.Default, "application/json");
                //HttpResponseMessage response = await unpClient.DeleteAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}");
                await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Request()
                    .DeleteAsync();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reset the sync of a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns></returns>
        public async Task ResetEduSyncProfile(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpContent emptyContent = new StringContent(string.Empty, Encoding.Default, "application/json");
                //HttpResponseMessage response = await unpClient.PostAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/Reset", emptyContent);
                await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Reset()
                    .Request()
                    .PostAsync();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Pause the sync of a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns></returns>
        public async Task PauseEduSyncProfile(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpContent emptyContent = new StringContent(string.Empty, Encoding.Default, "application/json");
                //HttpResponseMessage response = await unpClient.PostAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/Pause", emptyContent);
                await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Pause()
                    .Request()
                    .PostAsync();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Resume the sync of a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns></returns>
        public async Task ResumeEduSyncProfile(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                //HttpContent emptyContent = new StringContent(string.Empty, Encoding.Default, "application/json");
                //HttpResponseMessage response = await unpClient.PostAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/Resume", emptyContent);
                await graphClient.Education
                    .SynchronizationProfiles[syncProfileId]
                    .Resume()
                    .Request()
                    .PostAsync();

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the errors generated during validation and/or during a sync of a specific school data synchronization profile in the tenant.
        /// </summary>
        /// <param name="syncProfileId">Synchronization profile identifier.</param>
        /// <returns>Collection of errors (collection of Microsoft.Graph.EducationSynchronizationError).</returns>
        public async Task<IEnumerable<Beta.EducationSynchronizationError>> GetSyncErrors(string syncProfileId)
        {
            try
            {
                await EnsureUnpToken();
                HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/errors");

                var errors = await graphClient.Education
                    //.SynchronizationProfiles[$"{syncProfileId}"]
                    .SynchronizationProfiles[syncProfileId]
                    .Errors
                    .Request()
                    .GetAsync();

                List<Beta.EducationSynchronizationError> errorList = errors.ToList();

                return errorList;

                //string res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                //    return (true, res);
                //}
                //else
                //{
                //    return (false, $"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{res}");
                //}




                ////*** ToTest: Output built with NextPageRequest code implementation start
                //await EnsureUnpToken();
                ////HttpResponseMessage response = await unpClient.GetAsync($"https://graph.microsoft.com/beta/education/synchronizationProfiles/{syncProfileId}/errors");
                //List<Beta.EducationSynchronizationError> errorList = new List<Beta.EducationSynchronizationError>();

                //var errors = await graphClient.Education
                //    .SynchronizationProfiles[syncProfileId]
                //    .Errors
                //    .Request()
                //    .GetAsync();

                //while (errors.Count > 0)
                //{
                //    errorList.AddRange(errors);
                //    if (errors.NextPageRequest != null)
                //    {
                //        errors = await errors.NextPageRequest
                //            .GetAsync();
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}

                //return errorList;
                //*** Output built with NextPageRequest code implementation end
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Moke Data functions
        /// <summary>
        /// Build an object of type Beta.EducationSynchronizationProfile with moke data.
        /// </summary>
        /// <param name="profileName">Name for the profile</param>
        /// <param name="newProfileIdentitySyncConfType">'Create' or 'Match', depending if the profile must be created for not exisintg or existing users.</param>
        /// <returns>A Microsoft.Graph.EducationSynchronizationProfile object.</returns>
        public Beta.EducationSynchronizationProfile DefaultSyncProfileFactory(string profileName, string newProfileIdentitySyncConfType)
        {
            Beta.EducationSynchronizationProfile retProfile = null;

            switch (newProfileIdentitySyncConfType)
            {
                case "Create":
                    retProfile = new Beta.EducationSynchronizationProfile()
                    {
                        DisplayName = profileName,
                        DataProvider = new Beta.EducationCsvDataProvider()
                        {
                            ODataType = "#Microsoft.Graph.educationCsvDataprovider",
                            Customizations = new Beta.EducationSynchronizationCustomizations()
                            {
                                School = null,
                                Section = null,
                                Student = null,
                                StudentEnrollment = null,
                                Teacher = null,
                                TeacherRoster = null
                            }
                        },
                        IdentitySynchronizationConfiguration = new Beta.EducationIdentityCreationConfiguration()
                        {
                            ODataType = "#Microsoft.Graph.educationIdentityCreationConfiguration",
                            UserDomains = new List<Beta.EducationIdentityDomain>()
                            {
                                new Beta.EducationIdentityDomain()
                                {
                                    AppliesTo = Beta.EducationUserRole.Student,
                                    Name = "uniproge.onmicrosoft.com"
                                },
                                new Beta.EducationIdentityDomain()
                                {
                                    AppliesTo = Beta.EducationUserRole.Teacher,
                                    Name = "uniproge.onmicrosoft.com"
                                }
                            }
                        },
                        LicensesToAssign = new List<Beta.EducationSynchronizationLicenseAssignment>()
                        {
                            new Beta.EducationSynchronizationLicenseAssignment()
                            {
                                //ODataType = "",
                                AppliesTo = Beta.EducationUserRole.Teacher,
                                SkuIds = new List<string>() { "94763226-9b3c-4e75-a931-5c89701abe66" }
                            },
                            new Beta.EducationSynchronizationLicenseAssignment()
                            {
                                //ODataType = "",
                                AppliesTo = Beta.EducationUserRole.Student,
                                SkuIds = new List<string>() { "314c4481-f395-4525-be8b-2ec4bb1e9d91" }
                            }
                        }
                    };
                    break;

                case "Match":
                    retProfile = new Beta.EducationSynchronizationProfile()
                    {
                        DisplayName = profileName,
                        DataProvider = new Beta.EducationCsvDataProvider()
                        {
                            ODataType = "#Microsoft.Graph.educationCsvDataprovider",
                            Customizations = new Beta.EducationSynchronizationCustomizations()
                            {
                                School = null,
                                Section = null,
                                Student = null,
                                StudentEnrollment = null,
                                Teacher = null,
                                TeacherRoster = null
                            }
                        },
                        IdentitySynchronizationConfiguration = new Beta.EducationIdentityMatchingConfiguration()
                        {
                            ODataType = "#Microsoft.Graph.EducationIdentityMatchingConfiguration",
                            MatchingOptions = new List<Beta.EducationIdentityMatchingOptions>()
                            {
                                new Beta.EducationIdentityMatchingOptions()
                                {
                                    AppliesTo = Beta.EducationUserRole.Student,
                                    SourcePropertyName = "Username",
                                    TargetDomain = "uniproge.onmicrosoft.com",
                                    TargetPropertyName = "userPrincipalName"
                                },
                                new Beta.EducationIdentityMatchingOptions()
                                {
                                    AppliesTo = Beta.EducationUserRole.Teacher,
                                    SourcePropertyName = "Username",
                                    TargetDomain = "uniproge.onmicrosoft.com",
                                    TargetPropertyName = "userPrincipalName"
                                }
                            }
                        }
                    };
                    break;
            }

            return retProfile;
        }
        #endregion
        #endregion

        #region Private functions
        private async Task<bool> EnsureUnpToken()
        {
            if (string.IsNullOrWhiteSpace(_unpToken))
            {
                throw new System.Security.Authentication.AuthenticationException("'Username & pwd' token is empty");
            }
            else
            {
                if (DateTime.Now < dtFirstStart.AddSeconds(3600) && DateTime.Now > dtNextConnRefresh)
                {
                    await this.ConnectWithUnp();
                    dtNextConnRefresh = dtNextConnRefresh.AddSeconds(connRefrSeconds);
                }
            }

            return true;
        }
        #endregion
    }
}
