using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Com.WaitWha.ThreadFix.REST
{
    /// <summary>
    /// Represents a response from the REST API.
    /// </summary>
    [Serializable]
    [DataContract]
    public class JsonResponse
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(JsonResponse));

        /// <summary>
        /// Message
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Whether or not the response is successful.
        /// </summary>
        [DataMember(Name="success")]
        public bool Success { get; set; }

        /// <summary>
        /// Return Code
        /// </summary>
        [DataMember(Name="responseCode")]
        public int Code { get; set; }

        /// <summary>
        /// Object (if returned); may be NULL
        /// </summary>
        /// <see cref="GetObjectAs{T}"/>
        public dynamic Object { get; set; }

        public DateTime CreatedDate { get; private set; }

        public JsonResponse()
        {
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Returns a JsonResponse for the given string JSON data.
        /// </summary>
        /// <param name="json">JSON data to serialize</param>
        /// <param name="objectType">Object type for the embedded object returned by the server.</param>
        /// <returns></returns>
        /// <example>
        /// string json = await HttpClient.GetStringAsync(new Uri(...));
        /// JsonResponse response = JsonRespose.GetInstance(json, typeof(Team));
        /// </example>
        public static JsonResponse GetInstance(string json, Type objectType)
        {
            Log.Debug(String.Format("Parsing JSON (string) to JsonResponse: {0}", json));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, dynamic> jsonResponse = 
                serializer.Deserialize<Dictionary<string, dynamic>>(json);
            JsonResponse response = new JsonResponse()
            {
                Message = jsonResponse["message"],
                Success = jsonResponse["success"],
                Code = jsonResponse["responseCode"]
            };

            if (response.Success && objectType != null)
            {
                /*
                 * 1. Get the returned object as a string (JSON). JavaScriptSerializer is not good at dynamic types.
                 * 2. Use DataContractJsonSerializer to serialize the JSON (string).
                 * 3. Profit?
                 */
                string serializedObject = serializer.Serialize(jsonResponse["object"]);
                if (!serializedObject.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                {
                    Log.Debug(String.Format("Serializing response object to type {0}: {1}", objectType.Name, serializedObject));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(objectType);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        stream.Write(Encoding.UTF8.GetBytes(serializedObject), 0, Encoding.UTF8.GetByteCount(serializedObject));
                        stream.Flush();
                        stream.Position = 0;

                        response.Object = jsonSerializer.ReadObject(stream);
                    }
                }
            }
            
            Log.Debug(String.Format("JsonResponse.Success = {0}", response.Success));
            if (!response.Success)
                Log.Error(String.Format("JsonResponse Error {1}: {0}", response.Message, response.Code));
            
            return response;
        }
        
        /// <summary>
        /// Returns a JsonResponse for the given HttpResponseMessage (contents) and serialize the 
        /// embedded object as the given objectType.
        /// </summary>
        /// <param name="msg">HttpResponseMessage to serialize information from using the contents.</param>
        /// <param name="objectType">Type of object we should serialize within the response.</param>
        /// <returns></returns>
        public static JsonResponse GetInstance(HttpResponseMessage msg, Type objectType)
        {
            return GetInstance(msg.Content.ReadAsStringAsync().GetAwaiter().GetResult(), objectType);
        }

    }
}
