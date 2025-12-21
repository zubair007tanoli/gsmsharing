namespace GsmsharingV2.Models.NewSchema
{
    public class SystemSetting
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

