using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix
{
    /// <summary>
    /// Specialized NameValueCollection implementation to ensure that values are URL encoded.
    /// </summary>
    public class NameValuePairs : NameValueCollection
    {

        public NameValuePairs() : base() { }

        /// <summary>
        /// Constructor to initially seed the name-value pairs. 
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public NameValuePairs(string key, string value) : base()
        {
            this.Add(key, value);
        }

        /// <summary>
        /// Adds a name value pair. The value is URL encoded.
        /// </summary>
        /// <param name="name">Name/Key</param>
        /// <param name="value">Value</param>
        public override void Add(string name, string value)
        {
            base.Add(name, WebUtility.UrlEncode(value));
        }

        /// <summary>
        /// Returns this NameValuePairs object as a query string.
        /// </summary>
        /// <returns>Query string representation of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string key in this.Keys)
            {
                sb.Append(String.Format("{0}={1}&", key, this[key]));
            }

            return sb.ToString().Substring(0, sb.Length - 1);
        }

        /// <summary>
        /// Returns the NameValuePairs as a query string.
        /// </summary>
        /// <returns></returns>
        public string ToQueryString()
        {
            return this.ToString();
        }

        /// <summary>
        /// Returns the NameValuePairs as JSON. The values will be URL decoded. 
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach(string key in this.Keys)
            {
                sb.Append(String.Format("\"{0}\": \"{1}\", ", key, WebUtility.UrlDecode(this[key])));
            }

            return sb.ToString().Substring(0, sb.Length - 2) + "}";
        }

    }
}
