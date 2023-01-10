
using Microsoft.AspNetCore.SignalR.Client;
using QuixTracker.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuixTracker.Services
{
    public class FirmwareService
    {
        public FirmwareService()
        {

        }
    }

    public class QuixWriterService : QuixSignalRService
    {
        private readonly ConnectionService connectionService;


        public QuixWriterService(ConnectionService connectionService)
            : base("writer", connectionService)
        {
            this.connectionService = connectionService;
        }

        public async Task CloseStream(string streamId)
        {
            await this.Connection.InvokeAsync("CloseStream", this.connectionService.Settings.Topic, streamId);
        }

        public async Task<string> CreateStream(string deviceId, string rider, string team, string sesionName)
        {
            var streamId = $"{rider}-{deviceId}-{Guid.NewGuid().ToString().Substring(0, 6)}";
            if (string.IsNullOrEmpty(sesionName))
            {
                sesionName = streamId;
            }

            var streamDetails = new
            {
                Name = sesionName,
                Location = team + "/" + rider,
                Metadata = new
                {
                    Rider = rider,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            await this.Connection.InvokeAsync("UpdateStream", this.connectionService.Settings.Topic, streamId, streamDetails);


            return streamId;
        }

        public async Task SendParameterData(string streamId, ParameterDataDTO data)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await this.Connection.InvokeAsync("SendParameterData", this.connectionService.Settings.Topic, streamId, data);

            LoggingService.Instance.LogTrace("SEND in " + stopwatch.ElapsedMilliseconds);

        }

        public async Task SendEventData(string streamId, EventDataDTO[] data)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await this.Connection.InvokeAsync("SendEventData", this.connectionService.Settings.Topic, streamId, data);
            LoggingService.Instance.LogTrace("SEND in " + stopwatch.ElapsedMilliseconds);
        }
    }
}
