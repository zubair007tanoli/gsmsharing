namespace discussionspot.ViewModels
{
    public class AwardViewModel
    {
        public int AwardId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int CoinCost { get; set; }
        public int GiveKarma { get; set; }
        public int ReceiveKarma { get; set; }
        public bool IsActive { get; set; }
    }
}
