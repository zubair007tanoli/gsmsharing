using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;

namespace GsmsharingV2.Services
{
    public class ReactionService : IReactionService
    {
        private readonly IReactionRepository _reactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReactionService> _logger;

        public ReactionService(
            IReactionRepository reactionRepository,
            IMapper mapper,
            ILogger<ReactionService> logger)
        {
            _reactionRepository = reactionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReactionDto?> GetByIdAsync(int id)
        {
            var reaction = await _reactionRepository.GetByIdAsync(id);
            return reaction == null ? null : _mapper.Map<ReactionDto>(reaction);
        }

        public async Task<IEnumerable<ReactionDto>> GetByPostIdAsync(int postId)
        {
            var reactions = await _reactionRepository.GetByPostIdAsync(postId);
            return _mapper.Map<IEnumerable<ReactionDto>>(reactions);
        }

        public async Task<IEnumerable<ReactionDto>> GetByCommentIdAsync(int commentId)
        {
            var reactions = await _reactionRepository.GetByCommentIdAsync(commentId);
            return _mapper.Map<IEnumerable<ReactionDto>>(reactions);
        }

        public async Task<IEnumerable<ReactionSummaryDto>> GetReactionSummaryAsync(int? postId, int? commentId, string userId)
        {
            var reactionTypes = new[] { "like", "love", "laugh", "wow", "sad", "angry" };
            var summaries = new List<ReactionSummaryDto>();

            foreach (var type in reactionTypes)
            {
                var count = await _reactionRepository.GetReactionCountAsync(postId, commentId, type);
                var userHasReacted = await _reactionRepository.UserHasReactedAsync(userId, postId, commentId, type);

                summaries.Add(new ReactionSummaryDto
                {
                    ReactionType = type,
                    Count = count,
                    UserHasReacted = userHasReacted
                });
            }

            return summaries.Where(s => s.Count > 0 || s.UserHasReacted);
        }

        public async Task<ReactionDto> ToggleReactionAsync(CreateReactionDto createReactionDto, string userId)
        {
            // Check if user already has this reaction
            Reaction? existing = null;
            if (createReactionDto.PostID.HasValue)
            {
                existing = await _reactionRepository.GetByUserAndPostAsync(userId, createReactionDto.PostID.Value);
            }
            else if (createReactionDto.CommentID.HasValue)
            {
                existing = await _reactionRepository.GetByUserAndCommentAsync(userId, createReactionDto.CommentID.Value);
            }

            // If exists and same type, remove it (toggle off)
            if (existing != null && existing.ReactionType == createReactionDto.ReactionType)
            {
                await _reactionRepository.DeleteAsync(existing.ReactionID);
                return _mapper.Map<ReactionDto>(existing);
            }

            // If exists but different type, update it
            if (existing != null)
            {
                existing.ReactionType = createReactionDto.ReactionType;
                existing.CreatedAt = DateTime.UtcNow;
                // Note: Update would need to be implemented in repository
                await _reactionRepository.DeleteAsync(existing.ReactionID);
            }

            // Create new reaction
            var reaction = _mapper.Map<Reaction>(createReactionDto);
            reaction.UserId = userId;
            var created = await _reactionRepository.CreateAsync(reaction);
            return _mapper.Map<ReactionDto>(created);
        }

        public async Task<bool> DeleteReactionAsync(int id, string userId)
        {
            var reaction = await _reactionRepository.GetByIdAsync(id);
            if (reaction == null || reaction.UserId != userId)
                return false;

            await _reactionRepository.DeleteAsync(id);
            return true;
        }
    }
}

