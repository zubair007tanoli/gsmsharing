using System.Threading.Tasks;

namespace discussionspot.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface to coordinate multiple repositories
    /// </summary>
    public interface IUnitOfWork
    {
        // Repository properties
        IUserRepository Users { get; }
        IUserProfileRepository UserProfiles { get; }
        ICategoryRepository Categories { get; }
        ICommunityRepository Communities { get; }
        ICommunityMemberRepository CommunityMembers { get; }
        IPostRepository Posts { get; }
        ICommentRepository Comments { get; }
        IMediaRepository Media { get; }
        ITagRepository Tags { get; }
        IPostTagRepository PostTags { get; }
        IPostVoteRepository PostVotes { get; }
        //ICommentVoteRepository CommentVotes { get; }
        IPollOptionRepository PollOptions { get; }
        IPollVoteRepository PollVotes { get; }
        IPollConfigurationRepository PollConfigurations { get; }
        IAwardRepository Awards { get; }
        IPostAwardRepository PostAwards { get; }
        ICommentAwardRepository CommentAwards { get; }
        ISeoMetadataRepository SeoMetadata { get; }

        // Transaction management
        Task<int> SaveChangesAsync();
        int SaveChanges();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}