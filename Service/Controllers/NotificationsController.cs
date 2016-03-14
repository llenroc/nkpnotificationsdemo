using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using nkpnotificationsdemoService.Models;

namespace nkpnotificationsdemoService.Controllers
{
    public class NotificationsController : ApiController
    {
        private NotificationHubClient hub;

        public NotificationsController()
        {
            hub = NotificationHubClient.CreateClientFromConnectionString(
                ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString, 
                ConfigurationManager.AppSettings["MS_NotificationHubName"]);
        }

        public async Task<string> Get()
        {
            return "Hello!";
        }

        // POST api/register
        // This creates a registration id
        public async Task<string> Post()
        {
            return await hub.CreateRegistrationIdAsync();
        }

        // PUT api/register/5
        // This creates or updates a registration (with provided PNS handle) at the specified id
        public async Task<IHttpActionResult> Put(string id, DeviceRegistration deviceUpdate)
        {
            // IMPORTANT: add logic to make sure that caller is allowed to register for the provided tags

            RegistrationDescription registration = null;
            switch (deviceUpdate.Platform)
            {
                case "mpns":
                    registration = new MpnsRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "wns":
                    registration = new WindowsRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "apns":
                    registration = new AppleRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "gcm":
                    registration = new GcmRegistrationDescription(deviceUpdate.Handle);
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            registration.RegistrationId = id;

            if (deviceUpdate.Tags != null)
                registration.Tags = new HashSet<string>(deviceUpdate.Tags);

            try
            {
                await hub.CreateOrUpdateRegistrationAsync(registration);
            }
            catch (MessagingException e)
            {
                ReturnGoneIfHubResponseIsGone(e);
            }

            return Ok();
        }

        // DELETE api/register/5
        public async void Delete(string id)
        {
            await hub.DeleteRegistrationAsync(id);
        }


        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }
    }
}
