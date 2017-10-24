using Com.WaitWha.ThreadFix.REST.Domain;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST
{
    /// <summary>
    /// A class to work with the ThreadFix REST API.
    /// </summary>
    public class ThreadFixService
    {
        static ILog Log = LogManager.GetLogger(typeof(ThreadFixService));
        public static readonly string REST = "rest";
        public const string DefaultUserAgent = "TFLIB.Net v1.0";
        public static readonly long DEFAULT_MAX_RESPONSE_CONTENT_BUFFER_SIZE = 65535 * 4;

        public Uri BaseUri { get; private set; }
        public string ApiKey { get; private set; }
        public string UserAgent { get; private set; }

        HttpClient Client { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUri">Base URI of the ThreadFix REST API (i.e. http://localhost:8080/threadfix) </param>
        /// <param name="apiKey">API Key; generate one from (gear icon) -> Administration -> API Keys within ThreadFix.</param>
        /// <param name="userAgent">User Agent (if you would like to use a custom one)</param>
        public ThreadFixService(Uri baseUri, string apiKey, string userAgent = DefaultUserAgent)
        {
            BaseUri = baseUri;
            ApiKey = apiKey;
            UserAgent = userAgent;

            Client = new HttpClient() { MaxResponseContentBufferSize = DEFAULT_MAX_RESPONSE_CONTENT_BUFFER_SIZE };
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        /// <summary>
        /// Constructor for http://localhost:8080/threadfix. 
        /// </summary>
        /// <param name="apiKey">API Key to use.</param>
        public ThreadFixService(string apiKey) : this(new Uri("http://localhost:8080/threadfix"), apiKey) { }

        /// <summary>
        /// Uses the given path with the BaseUri and ApiKey (given in the constructor) to make a call
        /// to the ThreadFix REST API using HTTP verb GET. 
        /// </summary>
        /// <param name="path">Path of the REST API to call (i.e. teams/3)</param>
        /// <param name="data">Data to include in the request (i.e. name=TEST123)</param>
        /// <returns>JsonResponse</returns>
        async Task<JsonResponse> Get(string path, Type objectType, NameValuePairs nvPairs = null)
        {
            path = "threadfix/" + REST + "/" + path;
            if (nvPairs != null)
            {
                path += "?" + nvPairs + "&";
            }
            else
            {
                path += "?";
            }

            path += String.Format("apiKey={0}", ApiKey);

            Uri endpoint = new Uri(BaseUri, path);
            Log.Debug(String.Format("Sending GET request to {0}", endpoint));
            string json = await Client.GetStringAsync(endpoint);
            return JsonResponse.GetInstance(json, objectType);
        }

        /// <summary>
        /// Uses the given path with the BaseUri and ApiKey to make a call to the
        /// ThreadFix REST API using the HTTP verb POST. 
        /// </summary>
        /// <param name="path">Path to the resources to post to (i.e. teams/new)</param>
        /// <param name="data">Data to post</param>
        /// <returns>JsonResponse</returns>
        async Task<JsonResponse> Post(string path, NameValuePairs nvPairs, Type objectType)
        {
            path = "threadfix/"+ REST + "/" + path;
            path += String.Format("?apiKey={0}", ApiKey);

            //JSON content
            //StringContent content = new StringContent(nvPairs.ToJson(), Encoding.UTF8, "application/json");

            //Query content
            StringContent content = new StringContent(nvPairs.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");

            Uri uri = new Uri(BaseUri, path);
            Log.Debug(String.Format("Sending HTTP POST request to {0} containing data: {1}", uri, nvPairs));
            HttpResponseMessage response = await Client.PostAsync(
                uri,
                content);
            Log.Debug(String.Format("Received HTTP response from {1}: {0}", response.StatusCode, uri));

            string json = await response.Content.ReadAsStringAsync();
            return JsonResponse.GetInstance(json, objectType);
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="path">Path on the REST API to send the POST</param>
        /// <param name="pathToFile">Local path to the file to upload</param>
        /// <returns>JsonResponse</returns>
        async Task<JsonResponse> Upload(string path, string pathToFile)
        {
            path = "threadfix/" + REST + "/" + path;
            path += String.Format("?apiKey={0}", ApiKey);

            Log.Debug(String.Format("Caching file: {0}", pathToFile));
            MultipartContent content = new MultipartContent();
            using (FileStream stream = new FileStream(pathToFile, FileMode.Open))
            {
                content.Add(new StreamContent(stream));
            }

            Uri uri = new Uri(BaseUri, path);
            Log.Debug(String.Format("Uploading file to ThreadFix service: {0}", uri));
            HttpResponseMessage response = await Client.PostAsync(uri, content);
            Log.Debug(String.Format("Received HTTP Response: {0}", response.StatusCode));

            return JsonResponse.GetInstance(response, null);
        }

        #region Teams

        /// <summary>
        /// Adds a new team by the given name.
        /// </summary>
        /// <param name="name">Name of the team to add.</param>
        /// <returns>int; newly created team id or -1 for an error</returns>
        public async Task<int> CreateTeam(string name)
        {
            JsonResponse response = await Post("teams/new", new NameValuePairs("name", name), typeof(Team));
            if (response.Success)
                return response.Object.Id;
            else
                return response.Code;

        }

        /// <summary>
        /// Returns a Team by the given ID.
        /// </summary>
        /// <param name="id">ID of the team to return.</param>
        /// <returns>Team</returns>
        public async Task<Team> GetTeam(int id)
        {
            JsonResponse response = await Get(String.Format("teams/{0}", id), typeof(Team));
            return response.Object;
        }

        /// <summary>
        /// Returns a Team by the given name.
        /// </summary>
        /// <param name="name">Name of the team to return.</param>
        /// <returns>Team</returns>
        public async Task<Team> GetTeam(string name)
        {
            JsonResponse response = await Get("teams/lookup", typeof(Team), new NameValuePairs("name", name));
            return response.Object;
        }

        /// <summary>
        /// Returns all teams within ThreadFix.
        /// </summary>
        /// <returns>List&lt;Team&gt;</returns>
        public async Task<List<Team>> GetAllTeams()
        {
            Log.Debug("Getting list of teams from ThreadFix.");
            JsonResponse response = await Get("teams", typeof(List<Team>));
            return response.Object;
        }

        #endregion

        #region Applications

        /// <summary>
        /// Adds an application.
        /// </summary>
        /// <param name="teamId">Team Id</param>
        /// <param name="name">Name of the application</param>
        /// <param name="url">URL of the application</param>
        /// <returns>Id of the newly added application.</returns>
        public async Task<int> AddApplication(int teamId, string name, string url)
        {
            NameValuePairs nvPairs = new NameValuePairs("name", name);
            nvPairs.Add("url", url);

            JsonResponse response = await Post(String.Format("teams/{0}/applications/new", teamId), nvPairs, typeof(Application));
            if (response.Success)
                return response.Object.Id;
            else
                return response.Code;
        }

        /// <summary>
        /// Returns an application from ThreadFix by the given Id.
        /// </summary>
        /// <param name="id">Application Id</param>
        /// <returns></returns>
        public async Task<Application> GetApplication(int id)
        {
            JsonResponse response = await Get(String.Format("applications/{0}", id), typeof(Application));
            return response.Object;
        }

        /// <summary>
        /// Returns an application by the given name and team.
        /// </summary>
        /// <param name="teamName">Team Name</param>
        /// <param name="appName">Application Name</param>
        /// <returns></returns>
        public async Task<Application> GetApplication(string teamName, string appName)
        {
            JsonResponse response = await Get(String.Format("applications/{0}/lookup", teamName), typeof(Application), new NameValuePairs("name", appName));
            return response.Object;
        }

        /// <summary>
        /// Updates an application.
        /// </summary>
        /// <param name="appId">Application Id</param>
        /// <param name="frameworkType">FrameworkType (e.g. DETECT)</param>
        /// <param name="repositoryUrl">Repository URL</param>
        /// <returns></returns>
        public async Task<bool> SetApplicationParameters(int appId, FrameworkTypes frameworkType, string repositoryUrl)
        {
            NameValuePairs nvPairs = new NameValuePairs("frameworkType", frameworkType.ToString().ToUpper());
            nvPairs.Add("repositoryUrl", repositoryUrl);

            JsonResponse response = await Post(
                String.Format("applications/{0}/setParameters", appId), 
                nvPairs,
                typeof(Application));
            return response.Success;
        }

        /// <summary>
        /// Sets an application's Web Application Firewall (WAF).
        /// </summary>
        /// <param name="appId">Application Id</param>
        /// <param name="wafId">WAF Id</param>
        /// <returns></returns>
        public async Task<bool> SetApplicationWAF(int appId, int wafId)
        {
            JsonResponse response = await Post(String.Format("applications/{0}/setWaf", appId), new NameValuePairs("wafId", wafId +""), typeof(Waf));
            return response.Success;
        }

        /// <summary>
        /// Sets the application's URL.
        /// </summary>
        /// <param name="appId">Application Id</param>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public async Task<bool> SetApplicationUrl(int appId, string url)
        {
            JsonResponse response = await Post(String.Format("applications/{0}/setUrl", appId), new NameValuePairs("url", url), typeof(Application));
            return response.Success;
        }

        /// <summary>
        /// Uploads a scan file (probably XML formatted) for an application. ThreadFix must be able to support the 
        /// scan file via an importer implementation. For example, Arachni reports exported to XML must be edited to 
        /// remove the <seed></seed> element which will allow the importer implementation to parse the file properly.
        /// </summary>
        /// <param name="appId">Application Id</param>
        /// <param name="pathToFile">Path to the file on disk to upload.</param>
        /// <returns></returns>
        public async Task<bool> AddApplicationScan(int appId, string pathToFile)
        {
            JsonResponse response = await Upload(String.Format("applications/{0}/upload", appId), pathToFile);
            return response.Success;
        }

        #endregion

        #region Web Application Firewalls (WAFs)

        /// <summary>
        /// Adds a Web Application Firewall (WAF)
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type (e.g. mod_security)</param>
        /// <returns></returns>
        public async Task<int> AddWaf(string name, string type)
        {
            NameValuePairs nvPairs = new NameValuePairs("name", name);
            nvPairs.Add("type", type);

            JsonResponse response = await Post("wafs/new", nvPairs, typeof(Waf));
            if (response.Success)
                return response.Object.Id;
            else
                return response.Code;
        }

        /// <summary>
        /// Returns a Web Application Firewall (WAF) by the given Id.
        /// </summary>
        /// <param name="id">WAF Id</param>
        /// <returns></returns>
        public async Task<Waf> GetWaf(int id)
        {
            JsonResponse response = await Get(String.Format("wafs/{0}", id), typeof(Waf));
            return response.Object;
        }

        /// <summary>
        /// Returns a Web Application Firewall by the given name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public async Task<Waf> GetWaf(string name)
        {
            JsonResponse response = await Get("wafs/lookup", typeof(Waf), new NameValuePairs("name", name));
            return response.Object;
        }

        /// <summary>
        /// Returns a List containing all of the Web Application Firewalls within ThreadFix.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Waf>> GetAllWafs()
        {
            JsonResponse response = await Get("wafs", typeof(Waf));
            return response.Object;
        }

        /// <summary>
        /// Returns the rules defined for a given Web Application Firewall (WAF)
        /// </summary>
        /// <param name="wafId">WAF Id</param>
        /// <param name="appId">Application Id</param>
        /// <returns></returns>
        public async Task<string> GetWafRules(int wafId, int appId)
        {
            JsonResponse response = await Get(String.Format("wafs/{0}/rules/app/{1}", wafId, appId), typeof(Waf));
            return response.Object;
        }

        /// <summary>
        /// Uploads a log from a Web Application Firewall (WAF) within ThreadFix.
        /// </summary>
        /// <param name="wafId">WAF Id</param>
        /// <param name="pathToFile">Path to the log file from the WAF</param>
        /// <returns></returns>
        public async Task<bool> UploadWafLog(int wafId, string pathToFile)
        {
            JsonResponse response = await Upload(String.Format("wafs/{0}/uploadlog", wafId), pathToFile);
            return response.Success;
        }

        #endregion

        /// <summary>
        /// Adds a manual scan finding to an application by the given appId. 
        /// </summary>
        /// <param name="appId">Application ID</param>
        /// <param name="vulnType">Name of CWE vulnerability.</param>
        /// <param name="longDescription">General description of the issue.</param>
        /// <param name="severity">Severity level from 0-</param>
        /// <param name="isStatic">Whether the finding is static or <i>dynamic</i>.</param>
        /// <param name="nativeId">Specific identifier for vulnerability.</param>
        /// <param name="parameter">Request parameter for vulnerability.</param>
        /// <param name="filePath">(Static only)Location of source file.</param>
        /// <param name="column">(Static only)Column number for finding vulnerability source.</param>
        /// <param name="lineText">(Static only)Specific line text to finding vulnerability.</param>
        /// <param name="lineNumber">(Static only)Specific source line number to find vulnerability.</param>
        /// <param name="fullUrl">(Dynamic only)Absolute URL to the page with the vulnerability.</param>
        /// <param name="path">(Dynamic only)Relative path to vulnerability page.</param>
        /// <returns></returns>
        public async Task<bool> AddManualFinding(
            int appId,
            string vulnType,
            string longDescription,
            int severity,
            bool isStatic = false,
            int nativeId = -1,
            string parameter = "",
            string filePath = "",
            int column = -1,
            string lineText = "",
            int lineNumber = 0,
            string fullUrl = "",
            string path = "")
        {
            ManualFinding finding = new ManualFinding(-1, vulnType, longDescription, severity, isStatic, nativeId, parameter, filePath, column, lineText, lineNumber, fullUrl, path);
            JsonResponse response = await Post(String.Format("applications/{0}/addFinding", appId), finding.ToNameValuePairs(), typeof(ManualFinding));
            return response.Success;
        }


    }
}
