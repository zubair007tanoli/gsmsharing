namespace GsmsharingV2.Models
{
    public class MobileSpecs
    {
        public int Specid { get; set; }
        public string UserId { get; set; }
        public string ModelName { get; set; }
        public string NetworkInfo { get; set; }
        public DateTime? Launched { get; set; }
        public string Body { get; set; }
        public string Display { get; set; }
        public string OS { get; set; }
        public string Processor { get; set; }
        public string Memory { get; set; }
        public string MainCamera { get; set; }
        public string SelfiCam { get; set; }
        public string Sounds { get; set; }
        public string Common { get; set; }
        public string Sensors { get; set; }
        public string Battery { get; set; }
        public string Price { get; set; }
        public string MetaInfo { get; set; }
        public string Tags { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
    }
}

