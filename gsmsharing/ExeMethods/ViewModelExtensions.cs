using gsmsharing.ViewModels;

namespace gsmsharing.ExeMethods
{
    public static class ViewModelExtensions
    {
        public static Models.Post ToModel(this ViewModels.PostViewModel viewModel, string userId = null)
        {
            // Create a new Post model
            var post = new Models.Post
            {
                Title = viewModel.Title,
                Tags = viewModel.Tags,
                Content = viewModel.Content,
                CommunityID = viewModel.CommunityID,
                AllowComments = viewModel.AllowComments,

                // SEO properties
                MetaTitle = viewModel.MetaTitle ?? viewModel.Title,
                MetaDescription = viewModel.MetaDescription ?? viewModel.MetaDescription,
                OgTitle = viewModel.OgTitle ?? viewModel.Title,
                OgDescription = viewModel.OgDescription ?? viewModel.MetaDescription,

                // URL slug - generate from title if not provided
                Slug = !string.IsNullOrEmpty(viewModel.Slug)
                    ? viewModel.Slug
                    : GenerateSlug(viewModel.Title),

                // Status and timestamps
                PostStatus = viewModel.PostStatus ?? "Draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                // If post status is Published, set PublishedAt
                PublishedAt = viewModel.PostStatus == "Published" ? DateTime.UtcNow : null,

                // Set the UserId if provided
                UserId = userId,

                // Default values
                ViewCount = 0,
                IsPromoted = false,
                IsFeatured = false
            };

            // Handle featured image - use URL for web access
            post.FeaturedImage = viewModel.FeaturedImageUrl;

            return post;
        }

        public static PostEditViewModel ToEditViewModel(this Models.Post post)
        {
            return new PostEditViewModel
            {
                PostID = post.PostID,
                UserId = post.UserId,
                Title = post.Title,
                Tags = post.Tags,
                Content = post.Content,
                CommunityID = post.CommunityID ?? 0,
                CurrentFeaturedImageUrl = post.FeaturedImage,
                AllowComments = post.AllowComments ?? true,
                IsPromoted = post.IsPromoted ?? false,
                IsFeatured = post.IsFeatured ?? false,
                MetaTitle = post.MetaTitle,
                MetaDescription = post.MetaDescription,
                OgTitle = post.OgTitle,
                OgDescription = post.OgDescription,
                Slug = post.Slug,
                PostStatus = post.PostStatus,
                ImageAction = "keep" // Default to keeping the current image
            };
        }

        public static Models.Post ToModelFromEdit(this PostEditViewModel viewModel, string existingImageUrl = null)
        {
            var post = new Models.Post
            {
                PostID = viewModel.PostID,
                UserId = viewModel.UserId,
                Title = viewModel.Title,
                Tags = viewModel.Tags,
                Content = viewModel.Content,
                CommunityID = viewModel.CommunityID,
                AllowComments = viewModel.AllowComments,
                IsPromoted = viewModel.IsPromoted,
                IsFeatured = viewModel.IsFeatured,

                // SEO properties
                MetaTitle = viewModel.MetaTitle ?? viewModel.Title,
                MetaDescription = viewModel.MetaDescription,
                OgTitle = viewModel.OgTitle ?? viewModel.Title,
                OgDescription = viewModel.OgDescription ?? viewModel.MetaDescription,

                // Keep the same slug
                Slug = viewModel.Slug,

                // Status and timestamps
                PostStatus = viewModel.PostStatus ?? "Draft",
                UpdatedAt = DateTime.UtcNow
            };

            // Handle image based on action
            if (viewModel.ImageAction == "remove")
            {
                post.FeaturedImage = null;
            }
            else if (viewModel.ImageAction == "replace" && !string.IsNullOrEmpty(viewModel.FeaturedImageUrl))
            {
                post.FeaturedImage = viewModel.FeaturedImageUrl;
            }
            else if (viewModel.ImageAction == "keep")
            {
                post.FeaturedImage = existingImageUrl;
            }

            return post;
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Convert to lowercase
            string slug = title.ToLower();

            // Remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Convert spaces to hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

            // Remove multiple hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }

        public static async Task<Models.Post> ToModelWithFileUploadAsync(
            this ViewModels.PostViewModel viewModel,
            string userId,
            string webRootPath,
            string imagesFolder = "uploads/posts")
        {
            var post = viewModel.ToModel(userId);

            // Handle file upload if a file was provided
            if (viewModel.FeaturedImage != null && viewModel.FeaturedImage.Length > 0)
            {
                // Create directory if it doesn't exist
                string uploadsFolder = Path.Combine(webRootPath, imagesFolder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.FeaturedImage.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.FeaturedImage.CopyToAsync(fileStream);
                }

                // Set the FeaturedImage property to the relative path
                post.FeaturedImage = Path.Combine(imagesFolder, uniqueFileName).Replace("\\", "/");
            }

            return post;
        }
    }
}
