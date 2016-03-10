using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nkpnotificationsdemo
{
    public class NotificationsClient
    {
        private string POST_URL = Constants.ApplicationURL + "/api/notifications";

        private class DeviceRegistration
        {
            public string Platform { get; set; }
            public string Handle { get; set; }
            public string[] Tags { get; set; }
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
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var putUri = POST_URL + "/" + regId;
                var json = JsonConvert.SerializeObject(deviceRegistration);
                var response = await httpClient.PutAsync(putUri, new StringContent(json));
                //var response = await httpClient.PutAsJsonAsync<DeviceRegistration>(putUri, deviceRegistration);
                return response.StatusCode;
            }
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
