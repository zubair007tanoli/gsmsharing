namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommentTreeViewModel
    {
        public CommentViewModel Comment { get; set; } = new();
        public List<CommentTreeViewModel> Children { get; set; } = new();
        public bool IsCollapsed { get; set; }
        public int Depth { get; set; }

        // For rendering
        public bool ShowContinueThread => Depth >= 10 && Children.Any();
        public string ContinueThreadUrl => $"?comment={Comment.CommentId}";
        public int ChildCount => GetTotalChildCount();

        private int GetTotalChildCount()
        {
            var count = Children.Count;
            foreach (var child in Children)
            {
                count += child.GetTotalChildCount();
            }
            return count;
        }
    }
}
