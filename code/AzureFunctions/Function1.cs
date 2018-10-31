using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctions
{
    public static class Function1
    {
        [FunctionName("AddFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            var numbers = req.GetQueryNameValuePairs()
                .Where(q => string.Compare(q.Key, "one", true) == 0
                    || string.Compare(q.Key, "two", true) == 0)
                .Select(i => i.Value).ToList();

            var one = Int32.Parse(numbers[0]);
            var two = Int32.Parse(numbers[1]);
            var sum = one + two;

            return req.CreateResponse(HttpStatusCode.OK, new { Sum = sum });
        }
    }
}
