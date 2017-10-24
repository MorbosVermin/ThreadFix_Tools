using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST.Domain
{
    [Serializable]
    public class ManualFinding
    {
        public int Id { get; set; }
        public string vulnType { get; set; }
        public string longDescription { get; set; }
        public int severity { get; set; }
        public bool isStatic { get; set; }
        public int nativeId { get; set; }
        public string parameter { get; set; }
        public string filePath { get; set; }
        public int column { get; set; }
        public string lineText { get; set; }
        public int lineNumber { get; set; }
        public string fullUrl { get; set; }
        public string path { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID of the finding within ThreadFix</param>
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
        public ManualFinding(
            int id, 
            string vulnType, 
            string longDescription, 
            int severity, 
            bool isStatic, 
            int nativeId, 
            string parameter, 
            string filePath, 
            int column, 
            string lineText, 
            int lineNumber, 
            string fullUrl, 
            string path)
        {
            Id = id;
            this.vulnType = vulnType;
            this.longDescription = longDescription;
            this.severity = severity;
            this.isStatic = isStatic;
            this.nativeId = nativeId;
            this.parameter = parameter;
            this.filePath = filePath;
            this.column = column;
            this.lineText = lineText;
            this.lineNumber = lineNumber;
            this.fullUrl = fullUrl;
            this.path = path;
        }

        public ManualFinding() : this(-1, "", "", 0, false, 0, "", "", 0, "", 0, "", "") { }

        /// <summary>
        /// Returns the finding in query string format.
        /// </summary>
        /// <returns>string; query string to use when adding a finding via the REST API.</returns>
        public string ToQueryString()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("id", Id +"");
            parameters.Add("vulnType", vulnType);
            parameters.Add("longDescription", longDescription);
            parameters.Add("severity", severity +"");
            parameters.Add("isStatic", isStatic.ToString());
            parameters.Add("nativeId", nativeId + "");
            parameters.Add("parameter", parameter);
            parameters.Add("filePath", filePath);
            parameters.Add("column", column + "");
            parameters.Add("lineText", lineText);
            parameters.Add("lineNumber", lineNumber + "");
            parameters.Add("fullUrl", fullUrl);
            parameters.Add("path", path);

            StringBuilder sb = new StringBuilder();
            foreach(string key in parameters.Keys) 
            {
                sb.Append(String.Format("{0}={1}&", key, WebUtility.UrlEncode(parameters[key])));
            }

            return sb.ToString().Substring(0, sb.Length - 1);
        }

        public NameValuePairs ToNameValuePairs()
        {
            NameValuePairs nv = new NameValuePairs();
            nv.Add("id", Id +"");
            nv.Add("vulnType", vulnType);
            nv.Add("longDescription", longDescription);
            nv.Add("severity", severity +"");
            nv.Add("isStatic", (isStatic) ? "1" : "0");
            nv.Add("nativeId", nativeId + "");
            nv.Add("parameter", parameter);
            nv.Add("filePath", filePath);
            nv.Add("column", column + "");
            nv.Add("lineText", lineText);
            nv.Add("lineNumber", lineNumber + "");
            nv.Add("fullUrl", fullUrl);
            nv.Add("path", path);

            return nv;
        }
    }
}
