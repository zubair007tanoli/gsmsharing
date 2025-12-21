using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using GsmsharingV2.Database;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly GsmsharingV2.Interfaces.INotificationService _notificationService;

        public CommentService(
            ICommentRepository commentRepository,
            IMapper mapper,
            ILogger<CommentService> logger,
            ApplicationDbContext context,
            GsmsharingV2.Interfaces.INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return null;

            var dto = _mapper.Map<CommentDto>(comment);
            dto.ReplyCount = (await _commentRepository.GetRepliesAsync(id)).Count();
            return dto;
        }

        public async Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments);

            // Load replies for each comment
            foreach (var commentDto in commentsDto)
            {
                var replies = await _commentRepository.GetRepliesAsync(commentDto.CommentID);
                commentDto.Replies = _mapper.Map<List<CommentDto>>(replies);
                commentDto.ReplyCount = replies.Count();
            }

            return commentsDto;
        }

        public async Task<IEnumerable<CommentDto>> GetByUserIdAsync(string userId)
        {
            var comments = await _commentRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<IEnumerable<CommentDto>> GetRepliesAsync(int parentCommentId)
        {
            var replies = await _commentRepository.GetRepliesAsync(parentCommentId);
            return _mapper.Map<IEnumerable<CommentDto>>(replies);
        }

        public async Task<CommentDto> CreateAsync(CreateCommentDto createCommentDto, string userId)
        {
            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.UserId = userId;

            var created = await _commentRepository.CreateAsync(comment);
            
            // Create notification for post author if comment is on a post
            if (createCommentDto.PostID > 0)
            {
                var post = await _context.Posts.FindAsync(createCommentDto.PostID);
                if (post != null && post.UserId != userId)
                {
                    var userName = _context.Users.Find(userId)?.UserName ?? "Someone";
                    await _notificationService.CreateNotificationAsync(
                        post.UserId,
                        "New Comment",
                        $"{userName} commented on your post: {post.Title}",
                        "comment",
                        created.CommentID,
                        "Comment"
                    );
                }
            }
            
            return _mapper.Map<CommentDto>(created);
        }

        public async Task<CommentDto> UpdateAsync(UpdateCommentDto updateCommentDto, string userId)
        {
            var existing = await _commentRepository.GetByIdAsync(updateCommentDto.CommentID);
            if (existing == null || existing.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            existing.Content = updateCommentDto.Content;
            var updated = await _commentRepository.UpdateAsync(existing);
            return _mapper.Map<CommentDto>(updated);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            await _commentRepository.DeleteAsync(id);
        }

        public async Task<int> GetCommentCountAsync(int postId)
        {
            return await _commentRepository.GetCommentCountAsync(postId);
        }
    }
}

