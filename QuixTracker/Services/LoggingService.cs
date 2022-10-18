using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuixTracker.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QuixTracker.Services
{
    public class LoggingService 
    {
        private static LoggingService instance;
        public static LoggingService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LoggingService(ConnectionService.Instance.Settings);
                }

                return instance;
            }
        }

        private HttpClient client;
        private readonly string streamId;

        public LoggingService(Models.Settings settings)
        {
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IlBpQTRNeXRBRFg3Q19DdnlqZFJuaCJ9.eyJodHRwczovL3F1aXguYWkvb3JnX2lkIjoicXVpeGludGVybmFsIiwiaHR0cHM6Ly9xdWl4LmFpL293bmVyX2lkIjoiYXV0aDB8ZDZmZmRjODYtMWQ2NC00ZDM0LTgwY2YtMmM1YWU5ZGQ2ZGZhIiwiaHR0cHM6Ly9xdWl4LmFpL3Rva2VuX2lkIjoiMGIzZDU2NWUtNTIyZS00YjAwLTgzNGYtOTJlMDVkYjkzMzMwIiwiaHR0cHM6Ly9xdWl4LmFpL2V4cCI6IjE3MTUyOTIwMDAiLCJpc3MiOiJodHRwczovL3F1aXgtaW50ZXJuYWwuZXUuYXV0aDAuY29tLyIsInN1YiI6IjZVOXJNSzhXYzk4SXJHUlNHcGNKM2FIMWkyVVF5RG16QGNsaWVudHMiLCJhdWQiOiJodHRwczovL3BvcnRhbC1hcGkuaW50ZXJuYWwucXVpeC5haS8iLCJpYXQiOjE2NjI5OTc2OTQsImV4cCI6MTY2MzA4NDA5NCwiYXpwIjoiNlU5ck1LOFdjOThJckdSU0dwY0ozYUgxaTJVUXlEbXoiLCJndHkiOiJjbGllbnQtY3JlZGVudGlhbHMifQ.DVGMyop6V3G8Y5q6HOMI5J10Vhuf8Hx1jHdml4RkoEcPZKfF7JQ7F6e4MThQ9s-Uc3cSENlyILdEa4IHqd_QPe_yuunoE_B9IhI1rfwh2RLtBiQ626B2sij2bTQWJ2204pr0vmPmBr0Aaq-ATaRZiFAUu5qNqzhvL34-Zc_xGQqxJHE6Xmh0sZb9Wht-Dtj85OU-Kx3rRrvYMyAuIY60Qn5gCCEqEAIxwzsmg_rf5bm64WU2jZ_m6dxIEzTK-nxRV-FNQ0aHRDOd6ySLxbZfSIAJFBHlo-69LDqcRSz5_8PPqrWJzrVJHL05sth3pUZpwvcATsWoYPiy1xUk2aZ6Ng");
            this.client.BaseAddress = new Uri("https://writer-quixinternal-quixtrackerlogs.internal.quix.ai");
            this.streamId = $"{settings.Rider}-{settings.DeviceId}-{Guid.NewGuid().ToString().Substring(0, 6)}";
        }

        public void LogInformation(string message)
        {
            SendEvent("Information", message);
        }

        public void LogError(string message, Exception ex = null)
        {
            SendEvent("Error", message + "\n" +  ex?.ToString());
        }

        private void SendEvent(string id, string value)
        {
            var evt = new EventDataDTO
            {
                Id = id,
                Timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000,
                Value = value
            };

            this.client.PostAsync($"topics/quix-tracker-logs/streams/{this.streamId}/events/data", new StringContent(JsonSerializer.Serialize(new List<EventDataDTO> { evt }), Encoding.UTF8, "application/json"));


        }
    }
}
