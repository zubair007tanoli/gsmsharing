namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CategoryDropdownItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; } // FontAwesome icon class (e.g., "fa-microchip")
        public string? Description { get; set; } // Optional description
    }
}
