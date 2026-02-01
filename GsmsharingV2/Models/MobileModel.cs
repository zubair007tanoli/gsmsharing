namespace GsmsharingV2.Models
{
    /// <summary>
    /// Represents a mobile device model with comprehensive specifications
    /// </summary>
    public class MobileModel
    {
        public int ModelID { get; set; }
        public int BrandID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        
        // Network Specifications
        public string? NetworkTechnology { get; set; } // 2G, 3G, 4G, 5G
        public string? NetworkSpeed { get; set; }
        public string? GPRS { get; set; }
        public string? EDGE { get; set; }
        
        // Launch
        public DateTime? LaunchDate { get; set; }
        public string? LaunchStatus { get; set; } // Available, Discontinued, Coming Soon
        
        // Body
        public string? Dimensions { get; set; }
        public decimal? Weight { get; set; }
        public string? Build { get; set; } // Glass front, aluminum back, etc.
        public string? SIM { get; set; } // Dual SIM, eSIM, etc.
        
        // Display
        public string? DisplayType { get; set; } // AMOLED, LCD, etc.
        public string? DisplaySize { get; set; }
        public string? DisplayResolution { get; set; }
        public string? DisplayProtection { get; set; } // Gorilla Glass, etc.
        public string? DisplayFeatures { get; set; }
        
        // Platform
        public string? OS { get; set; } // Android, iOS
        public string? OSVersion { get; set; }
        public string? Chipset { get; set; }
        public string? CPU { get; set; }
        public string? GPU { get; set; }
        
        // Memory
        public string? CardSlot { get; set; }
        public string? InternalStorage { get; set; }
        public string? RAM { get; set; }
        
        // Main Camera
        public string? MainCameraSingle { get; set; }
        public string? MainCameraDual { get; set; }
        public string? MainCameraTriple { get; set; }
        public string? MainCameraQuad { get; set; }
        public string? MainCameraFeatures { get; set; } // LED flash, HDR, panorama
        public string? MainCameraVideo { get; set; }
        
        // Selfie Camera
        public string? SelfieCameraSingle { get; set; }
        public string? SelfieCameraDual { get; set; }
        public string? SelfieCameraFeatures { get; set; }
        public string? SelfieCameraVideo { get; set; }
        
        // Sound
        public string? Loudspeaker { get; set; }
        public string? HeadphoneJack { get; set; }
        public string? AudioFeatures { get; set; }
        
        // Communications
        public string? WLAN { get; set; } // WiFi
        public string? Bluetooth { get; set; }
        public string? GPS { get; set; }
        public string? NFC { get; set; }
        public string? InfraredPort { get; set; }
        public string? Radio { get; set; }
        public string? USB { get; set; }
        
        // Features
        public string? Sensors { get; set; }
        public string? Messaging { get; set; }
        public string? Browser { get; set; }
        
        // Battery
        public string? BatteryType { get; set; }
        public int? BatteryCapacity { get; set; } // mAh
        public string? Charging { get; set; } // Fast charging, wireless charging
        public string? BatteryFeatures { get; set; }
        
        // Misc
        public decimal? Price { get; set; }
        public string? PriceCurrency { get; set; }
        public string? Colors { get; set; }
        public string? ImageUrl { get; set; }
        public string? OfficialUrl { get; set; }
        
        // SEO Fields
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        
        // Tracking
        public string? UserId { get; set; }
        public int? ViewCount { get; set; } = 0;
        public bool IsVerified { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public MobileBrand Brand { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
