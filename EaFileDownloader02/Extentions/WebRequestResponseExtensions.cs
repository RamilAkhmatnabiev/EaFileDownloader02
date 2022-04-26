using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace EaFileDownloader02
{
    public static class WebRequestResponseExtensions
    {
        /// <summary>
        /// Получить результат запроса в виде строки
        /// </summary>
        public static string GetResponseString(this WebRequest webRequest)
        {
            if (webRequest == null)
            {
                return default(string);
            }

            using (var webResponse = webRequest.GetResponse())
            using (var responseStream = webResponse.GetResponseStream())
            using (var streamReader = new StreamReader(responseStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Запрос на запись объекта
        /// </summary>
        public static void SetRequestJsonBody(this HttpWebRequest endpointRequest, object obj)
        {
            endpointRequest.Method = "POST";
            endpointRequest.ContentType = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(endpointRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(obj));
            }
        }
    }
}
