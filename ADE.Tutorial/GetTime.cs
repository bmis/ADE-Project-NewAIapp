using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ADE.Tutorial
{
    public class GetTime
    {
        private readonly ILogger logger;

        private static readonly List<TimeZoneInfo> timeZones = new List<TimeZoneInfo>
        {
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
            TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
            TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
            TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),
            TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time")
        };

        public GetTime(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<GetTime>();
        }

        [Function("GetTime")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            // Get a random joke from the API
            var client = new HttpClient();
            var jokeResponse = await client.GetAsync("https://official-joke-api.appspot.com/random_joke");

            if (jokeResponse.IsSuccessStatusCode)
            {
                var jokeJson = await jokeResponse.Content.ReadAsStringAsync();
                var joke = JsonSerializer.Deserialize<Joke>(jokeJson);

                var now = DateTime.UtcNow;

                response.WriteString($"UTC Time: {now}\n\n");

                if (joke != null)
                {
                    response.WriteString($"Here's a joke for you:\n{joke.setup}\n{joke.punchline}\n\n");
                }
                else
                {
                    response.WriteString("Sorry, I couldn't find a joke right now. Please try again later.\n\n");
                }

                foreach (var timeZone in timeZones)
                    response.WriteString($"{(timeZone.IsDaylightSavingTime(now) ? timeZone.DaylightName : timeZone.StandardName)}: {TimeZoneInfo.ConvertTimeFromUtc(now, timeZone)}\n");
            }
            else
            {
                response.WriteString("Sorry, I couldn't get a joke right now. Please try again later.\n\n");
            }

            return response;
        }
    }

    public class Joke
    {
        public string setup { get; set; }
        public string punchline { get; set; }
    }
}