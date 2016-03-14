using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nkpnotificationsdemo
{
    public class NotificationsClient
    {
        private string POST_URL = Constants.ApplicationURL + "/api/notifications";

        public class DeviceRegistration
        {
            public string Platform { get; set; }
            public string Handle { get; set; }
            public string[] Tags { get; set; }

            public DeviceRegistration()
            {

            }
        }

        public async Task RegisterAsync(string handle, string platform, IEnumerable<string> tags)
        {
            var regId = await RetrieveRegistrationIdOrRequestNewOneAsync();

            var deviceRegistration = new DeviceRegistration
            {
                Platform = platform,
                Handle = handle,
                Tags = tags.ToArray<string>()
            };

            var statusCode = await UpdateRegistrationAsync(regId, deviceRegistration);

            if (statusCode == HttpStatusCode.Gone)
            {
                // regId is expired, deleting from local storage & recreating
                //var settings = ApplicationData.Current.LocalSettings.Values;
                //settings.Remove("__NHRegistrationId");
                regId = await RetrieveRegistrationIdOrRequestNewOneAsync();
                statusCode = await UpdateRegistrationAsync(regId, deviceRegistration);
            }

            if (statusCode != HttpStatusCode.Accepted)
            {
                // log or throw
            }
        }

        private async Task<HttpStatusCode> UpdateRegistrationAsync(string regId, DeviceRegistration deviceRegistration)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");

                var putUri = POST_URL + "/" + regId;
                var request = CreateJsonRequest(HttpMethod.Put, putUri, deviceRegistration);

                try
                {
                    var response = await httpClient.SendAsync(request);
                    return response.StatusCode;
                }
                catch (Exception exc)
                {
                    throw exc;
                }

            }
        }

        private HttpRequestMessage CreateJsonRequest<T>(HttpMethod method, string uri, T obj)
        {
            var stream = new MemoryStream();
            new DataContractJsonSerializer(typeof(T)).WriteObject(stream, obj);
            var content = new StreamContent(stream);
            content.Headers.Add("Content-Type", "application/json");

            var request = new HttpRequestMessage(method, uri) { Content = content };

            return request;
        }

        private async Task<string> RetrieveRegistrationIdOrRequestNewOneAsync()
        {
            //var settings = ApplicationData.Current.LocalSettings.Values;
            //if (!settings.ContainsKey("__NHRegistrationId"))
            //{
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(POST_URL, new StringContent(""));
                    if (response.IsSuccessStatusCode)
                    {
                        string regId = await response.Content.ReadAsStringAsync();
                        regId = regId.Substring(1, regId.Length - 2);

                        //settings.Add("__NHRegistrationId", regId);
                        return regId;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            //}
            //return (string)settings["__NHRegistrationId"];

        }
    }
}
