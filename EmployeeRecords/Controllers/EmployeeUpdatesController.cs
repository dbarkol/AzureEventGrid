using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace EmployeeRecords.Controllers
{
    public class GridEvent<T> where T : class
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public T Data { get; set; }
        public DateTime EventTime { get; set; }
    }
    [Produces("application/json")]
    [Route("api/EmployeeUpdates")]
    public class EmployeeUpdatesController : Controller
    {
        private bool EventTypeSubcriptionValidation
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
        "SubscriptionValidation";
        private bool EventTypeNotification
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
        "Notification";
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonContent = await reader.ReadToEndAsync();
                var gridEvent = JsonConvert.DeserializeObject<List<GridEvent<Dictionary<string, string>>>>(jsonContent).SingleOrDefault();

                if (gridEvent == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
                }
                
                // Check the event type from Event Grid.
                if (EventTypeSubcriptionValidation)
                {
                    // Retrieve the validation code and echo back.
                    var validationCode = gridEvent.Data["validationCode"];
                    var validationResponse = JsonConvert.SerializeObject(new
                    {
                        validationResponse = validationCode
                    });

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(validationResponse)
                    };
                }
                else if (EventTypeNotification)
                {
                    // Pseudo code: Update records
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                }
                else
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
                }
            }
        }
    }
}