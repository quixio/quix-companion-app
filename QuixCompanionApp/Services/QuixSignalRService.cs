
using Microsoft.AspNetCore.SignalR.Client;
using QuixCompanionApp.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuixCompanionApp.Services
{
    public abstract class QuixSignalRService : IAsyncDisposable
    {
        private HubConnection connection;
        private readonly string service;
        private readonly ConnectionService connectionService;

        protected HubConnection Connection => this.connection;

        public QuixSignalRService(string service, ConnectionService connectionService)
        {
            this.service = service;
            this.connectionService = connectionService;
        }

        public virtual async Task StartConnection()
        {
            this.connection = CreateWebSocketConnection(this.service);

            this.connection.Reconnecting += (e) =>
            {
                this.connectionService.OnOutputConnectionChanged(ConnectionState.Reconnecting);

                return Task.CompletedTask;
            };

            this.connection.Reconnected += (e) =>
            {
                this.connectionService.OnOutputConnectionChanged(ConnectionState.Connected);

                return Task.CompletedTask;
            };

            this.connection.Closed += (e) =>
            {
                this.connectionService.OnOutputConnectionChanged(ConnectionState.Disconnected);

                return Task.CompletedTask;
            };

            try
            {
                await this.connection.StartAsync();
            }
            catch (Exception e)
            {
                LoggingService.Instance.LogError("SignalR connection failed", e);
            }
        }

        public async Task StopAsync()
        {
            await this.connection.StopAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await this.connection.DisposeAsync();
        }

        private HubConnection CreateWebSocketConnection(string service)
        {
            var url = $"https://{service}-{this.connectionService.Settings.WorkspaceId}" +
                      $".{this.connectionService.Settings.SubDomain}.quix.ai/hub";


            return new HubConnectionBuilder()
                .WithAutomaticReconnect(Enumerable.Repeat(5, 10000).Select(s => TimeSpan.FromSeconds(s)).ToArray())
              .WithUrl(url, options =>
              {
                  options.AccessTokenProvider = () => Task.FromResult(this.connectionService.Settings.Token);
                  options.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Binary;

                  options.HttpMessageHandlerFactory = factory => new HttpClientHandler
                  {
                      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                  };
              })
              .Build();
        }

    }
}
