namespace QuixTracker.Models
{
    public sealed class FirmwareVersionCheckDTO
    {
        public string Current { get; set; }

        public string Target { get; set; }

        public string CampaignId { get; set; }
    }
}
