using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
namespace NewEmployeeApp
{
    public static class NewEmployeeHandler
    {
        public class GridEvent<T> where T : class
        {
            public string Id { get; set; }
            public string EventType { get; set; }
            public string Subject { get; set; }
            public DateTime EventTime { get; set; }
            public T Data { get; set; }
            public string Topic { get; set; }
        }

        [FunctionName("newemployeehandler")]
        public static async Task<HttpResponseMessage> Run(
                    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
                    HttpRequestMessage req,
                    TraceWriter log)
        {
            log.Info("New Employee Handler Triggered");

            // Retrieve the contents of the request and
            // deserialize it into a grid event object.
            var jsonContent = await req.Content.ReadAsStringAsync();
            var gridEvent = JsonConvert.DeserializeObject<List<GridEvent<Dictionary<string, string>>>>(jsonContent)?.SingleOrDefault();

            // Check to see if the event is available and
            // return an error response if its missing.
            if (gridEvent == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $@"Missing event details");
            }
            
            // Check the header to identify the type of request
            // from Event Grid. A subscription validation request
            // must echo back the validation code.
            var gridEventType = req.Headers.GetValues("Aeg-Event-Type").FirstOrDefault();

            if (gridEventType == "SubscriptionValidation")
            {
                var code = gridEvent.Data["validationCode"];
                return req.CreateResponse(HttpStatusCode.OK,
                new { validationResponse = code });
            }
            else if (gridEventType == "Notification")
            {
                // Pseudo code: place message into a queue
                // for further processing.
                return req.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest,
                $@"Unknown request type");
            }
        }
    }
}
