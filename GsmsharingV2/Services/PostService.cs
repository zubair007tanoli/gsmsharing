using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GsmsharingV2.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostService> _logger;

        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PostDto?> GetByIdAsync(int id)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(id);
                if (post == null) return null;

                return _mapper.Map<PostDto>(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post by ID: {PostId}", id);
                throw;
            }
        }

        public async Task<PostDto?> GetBySlugAsync(string slug)
        {
            try
            {
                var post = await _postRepository.GetBySlugAsync(slug);
                if (post == null) return null;

                return _mapper.Map<PostDto>(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post by slug: {Slug}", slug);
                throw;
            }
        }

        public async Task<PostDto?> GetBySlugAndCommunityAsync(string slug, string communitySlug)
        {
            try
            {
                var post = await _postRepository.GetBySlugAndCommunityAsync(slug, communitySlug);
                if (post == null) return null;

                return _mapper.Map<PostDto>(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post by slug and community: {Slug}, {CommunitySlug}", slug, communitySlug);
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetAllAsync()
        {
            try
            {
                var posts = await _postRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all posts");
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetByCommunityIdAsync(int communityId)
        {
            try
            {
                var posts = await _postRepository.GetByCommunityIdAsync(communityId);
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by community ID: {CommunityId}", communityId);
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var posts = await _postRepository.GetByUserIdAsync(userId);
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<PostDto> CreateAsync(CreatePostDto createPostDto, string userId)
        {
            try
            {
                var post = _mapper.Map<Post>(createPostDto);
                post.UserId = userId;
                post.PostStatus = "published";
                post.CreatedAt = DateTime.UtcNow;
                post.PublishedAt = DateTime.UtcNow;
                post.AllowComments = createPostDto.AllowComments ?? true;

                var createdPost = await _postRepository.CreateAsync(post);
                return _mapper.Map<PostDto>(createdPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto, string userId)
        {
            try
            {
                var existingPost = await _postRepository.GetByIdAsync(updatePostDto.PostID);
                if (existingPost == null)
                {
                    throw new KeyNotFoundException($"Post with ID {updatePostDto.PostID} not found");
                }

                if (existingPost.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User does not have permission to update this post");
                }

                _mapper.Map(updatePostDto, existingPost);
                existingPost.UpdatedAt = DateTime.UtcNow;

                var updatedPost = await _postRepository.UpdateAsync(existingPost);
                return _mapper.Map<PostDto>(updatedPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post: {PostId} for user: {UserId}", updatePostDto.PostID, userId);
                throw;
            }
        }

        public async Task DeleteAsync(int id, string userId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(id);
                if (post == null)
                {
                    throw new KeyNotFoundException($"Post with ID {id} not found");
                }

                if (post.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User does not have permission to delete this post");
                }

                await _postRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post: {PostId} for user: {UserId}", id, userId);
                throw;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _postRepository.GetTotalCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total post count");
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetPaginatedAsync(int page, int pageSize)
        {
            try
            {
                var posts = await _postRepository.GetPaginatedAsync(page, pageSize);
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated posts: Page {Page}, PageSize {PageSize}", page, pageSize);
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetFeaturedPostsAsync()
        {
            try
            {
                var posts = await _postRepository.GetFeaturedPostsAsync();
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured posts");
                throw;
            }
        }

        public async Task<IEnumerable<PostDto>> GetRecentPostsAsync(int count)
        {
            try
            {
                var posts = await _postRepository.GetRecentPostsAsync(count);
                return _mapper.Map<IEnumerable<PostDto>>(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent posts: Count {Count}", count);
                throw;
            }
        }

        public async Task IncrementViewCountAsync(int id)
        {
            try
            {
                await _postRepository.IncrementViewCountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for post: {PostId}", id);
                throw;
            }
        }
    }
}

