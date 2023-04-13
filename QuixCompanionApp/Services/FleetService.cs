using Newtonsoft.Json;
using QuixCompanionApp.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuixCompanionApp.Services
{
    public sealed class FleetService : IDisposable
    {
        private readonly HttpClient http;
        private readonly ConnectionService connectionService;
        private readonly LoggingService logger;
        private readonly Logger localLogger;

        public FleetService()
        {
            this.http = new HttpClient();
            this.connectionService = ConnectionService.Instance;
            this.logger = LoggingService.Instance;
            this.localLogger = Logger.Instance;
            this.http.DefaultRequestHeaders.Add("Authorization", $"bearer {this.connectionService.Settings.Token}");
        }

        public async Task CheckFirmwareUpdates()
        {
            try
            {
                if (this.connectionService.Settings.Token != null)
                {
                    var settings = this.connectionService.Settings;
                    var url = $"https://fleet-management-api-{settings.WorkspaceId}.deployments.quix.ai" +
                        $"/webhooks/firmware-version/{settings.DeviceId}";
                    var res = await this.http.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                    {
                        var dto = JsonConvert.DeserializeObject<FirmwareVersionCheckDTO>(await res.Content.ReadAsStringAsync());
                        if (dto.Current != dto.Target)
                        {
                            this.connectionService.OnFirmwareUpdateReceived(new FirmwareUpdate
                            {
                                Version = dto.Target,
                                CampaignId = dto.CampaignId,
                                Notification = dto.Notification
                            });
                        }
                        else
                        {
                            this.localLogger.Log("Firmware version is up to date");
                        }
                    }
                    else
                    {
                        this.localLogger.Log($"Firmware updated check failed with http status: {res.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Firmware update check failed", ex);
            }
        }

        public void Dispose()
        {
            this.http?.Dispose();
        }
    }
}
