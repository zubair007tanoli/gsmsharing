using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Models;

namespace GsmsharingV2.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Post mappings
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.CommunityName, opt => opt.MapFrom(src => src.Community != null ? src.Community.Name : string.Empty))
                .ForMember(dest => dest.CommunitySlug, opt => opt.MapFrom(src => src.Community != null ? src.Community.Slug : string.Empty))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))
                .ForMember(dest => dest.ReactionCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count : 0));

            CreateMap<CreatePostDto, Post>()
                .ForMember(dest => dest.PostID, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Score, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
                .ForMember(dest => dest.UpvoteCount, opt => opt.Ignore())
                .ForMember(dest => dest.DownvoteCount, opt => opt.Ignore())
                .ForMember(dest => dest.SchemaMarkup, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Community, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore())
                .ForMember(dest => dest.Votes, opt => opt.Ignore())
                .ForMember(dest => dest.SavedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Reports, opt => opt.Ignore())
                .ForMember(dest => dest.History, opt => opt.Ignore());

            CreateMap<UpdatePostDto, Post>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OgTitle, opt => opt.Ignore())
                .ForMember(dest => dest.OgDescription, opt => opt.Ignore())
                .ForMember(dest => dest.OgImage, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Community, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            // Community mappings
            CreateMap<Community, CommunityDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.UserName : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.PostCount, opt => opt.Ignore()); // Will be set in service if needed

            CreateMap<CreateCommunityDto, Community>()
                .ForMember(dest => dest.CommunityID, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore());

            CreateMap<UpdateCommunityDto, Community>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore());

            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : string.Empty))
                .ForMember(dest => dest.CommunityCount, opt => opt.Ignore()) // Will be set in service if needed
                .ForMember(dest => dest.PostCount, opt => opt.Ignore()); // Will be set in service if needed

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.CategoryID, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.OgTitle, opt => opt.Ignore())
                .ForMember(dest => dest.OgDescription, opt => opt.Ignore())
                .ForMember(dest => dest.OgImage, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDisabledParent, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Communities, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.OgTitle, opt => opt.Ignore())
                .ForMember(dest => dest.OgDescription, opt => opt.Ignore())
                .ForMember(dest => dest.OgImage, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDisabledParent, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Communities, opt => opt.Ignore());

            // Reverse mappings for views (DTO to Model)
            CreateMap<PostDto, Post>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Community, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            CreateMap<CommunityDto, Community>()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.ChatRooms, opt => opt.Ignore());

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Communities, opt => opt.Ignore());

            // Model to DTO for Create/Update operations
            CreateMap<Post, CreatePostDto>();
            CreateMap<Post, UpdatePostDto>();
            CreateMap<Community, CreateCommunityDto>();
            CreateMap<Community, UpdateCommunityDto>();
            CreateMap<Category, CreateCategoryDto>();
            CreateMap<Category, UpdateCategoryDto>();

            // Comment mappings
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.UserAvatar, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyCount, opt => opt.Ignore())
                .ForMember(dest => dest.ReactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.CommentID, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore());

            CreateMap<UpdateCommentDto, Comment>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PostID, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCommentID, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore());

            // Reaction mappings
            CreateMap<Reaction, ReactionDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));

            CreateMap<CreateReactionDto, Reaction>()
                .ForMember(dest => dest.ReactionID, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.Comment, opt => opt.Ignore());
        }
    }
}

