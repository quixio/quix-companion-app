
using Microsoft.AspNetCore.SignalR.Client;
using QuixCompanionApp.Models;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace QuixCompanionApp.Services
{

    public class QuixReaderService : QuixSignalRService
    {
        private readonly ConnectionService connectionService;

        public event EventHandler<EventDataDTO> EventDataRecieved;

        public QuixReaderService(ConnectionService connectionService) : base("reader", connectionService)
        {
            this.connectionService = connectionService;
        }

        public async Task SubscribeToEvent(string streamId, string eventId)
        {
            await this.Connection.InvokeAsync("SubscribeToEvent", connectionService.Settings.NotificationsTopic, streamId, eventId);
        }

        public override async Task StartConnection()
        {
            await base.StartConnection();

            this.Connection.On<ParameterDataDTO>("ParameterDataReceived", data =>
            {

            });

            this.Connection.On<EventDataDTO>("EventDataReceived", data =>
            {
                this.EventDataRecieved?.Invoke(this, data);
            });
        }
    }
}
