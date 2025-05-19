using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace discussionspot.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Repository instances
        private IPostRepository _posts;
        private IUserRepository _users;
        private IUserProfileRepository _userProfiles;
        private ICategoryRepository _categories;
        private ICommunityRepository _communities;
        private ICommunityMemberRepository _communityMembers;
        private ICommentRepository _comments;
        private IMediaRepository _media;
        private ITagRepository _tags;
        private IPostTagRepository _postTags;
        private IPostVoteRepository _postVotes;
        private IPollOptionRepository _pollOptions;
        private IPollVoteRepository _pollVotes;
        private IPollConfigurationRepository _pollConfigurations;
        private IAwardRepository _awards;
        private IPostAwardRepository _postAwards;
        private ICommentAwardRepository _commentAwards;
        private ISeoMetadataRepository _seoMetadata;

        public UnitOfWork(
            ApplicationDbContext context,
            UserManager<ApplicationUsers> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Implement repository properties
        public IPostRepository Posts => _posts ??= new PostRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context, _userManager, _roleManager);
        public IUserProfileRepository UserProfiles => _userProfiles ??= new UserProfileRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public ICommunityRepository Communities => _communities ??= new CommunityRepository(_context);
        public ICommunityMemberRepository CommunityMembers => _communityMembers ??= new CommunityMemberRepository(_context);
        public ICommentRepository Comments => _comments ??= new CommentRepository(_context);
        public IMediaRepository Media => _media ??= new MediaRepository(_context);
        public ITagRepository Tags => _tags ??= new TagRepository(_context);
        public IPostTagRepository PostTags => _postTags ??= new PostTagRepository(_context);
        public IPostVoteRepository PostVotes => _postVotes ??= new PostVoteRepository(_context);
        public IPollOptionRepository PollOptions => _pollOptions ??= new PollOptionRepository(_context);
        public IPollVoteRepository PollVotes => _pollVotes ??= new PollVoteRepository(_context);
        public IPollConfigurationRepository PollConfigurations => _pollConfigurations ??= new PollConfigurationRepository(_context);
        public IAwardRepository Awards => _awards ??= new AwardRepository(_context);
        public IPostAwardRepository PostAwards => _postAwards ??= new PostAwardRepository(_context);
        public ICommentAwardRepository CommentAwards => _commentAwards ??= new CommentAwardRepository(_context);
        public ISeoMetadataRepository SeoMetadata => _seoMetadata ??= new SeoMetadataRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }
    }
}