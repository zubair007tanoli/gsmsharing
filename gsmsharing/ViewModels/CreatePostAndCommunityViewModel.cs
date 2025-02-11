namespace gsmsharing.ViewModels
{
    public class CreatePostAndCommunityViewModel
    {
        public PostViewModel Post { get; set; }
        public CommunityViewModel Community { get; set; }

        public CreatePostAndCommunityViewModel()
        {
            Post = new PostViewModel();
            Community = new CommunityViewModel();
        }
    }
}
