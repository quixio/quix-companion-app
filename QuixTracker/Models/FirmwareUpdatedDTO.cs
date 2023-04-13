namespace QuixTracker.Models
{
    internal sealed class FirmwareUpdatedDTO
    {
        /// <summary>
        /// Serial number of the bike (same as the device id set in the app settings).
        /// </summary>
        public string BikeId { get; set; }

        /// <summary>
        /// Campaign id that triggered the firmware update.
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Current firware version of the bike.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Set updates status. Accepted values are "Success" and "Error".
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// If the status is "Error", set the error description.
        /// </summary>
        public string Error { get; set; }
    }
}
