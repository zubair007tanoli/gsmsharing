using gsmsharing.Database;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace gsmsharing.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly DatabaseConnection _db;
      

        public CommunityRepository(DatabaseConnection db)
        {
            _db = db;
            
        }

        public async Task<IEnumerable<Community>> GetAllAsync()
        {
            try
            {
                const string sql = @"
                SELECT 
                    CommunityID,
                    Name,
                    IsVerified
                FROM Communities
                WHERE IsPrivate = 0 OR IsPrivate IS NULL
                ORDER BY Name";

                var dataTable = await await Task.FromResult(_db.ExecuteQueryAsync(sql));
                var communities = new List<Community>();

                foreach (DataRow row in dataTable.Rows)
                {
                    communities.Add(new Community
                    {
                        CommunityID = await Task.FromResult(Convert.ToInt32(row["CommunityID"])),
                        Name = row["Name"].ToString(),
                        IsVerified = row["IsVerified"] != DBNull.Value ?
                            await Task.FromResult(Convert.ToBoolean(row["IsVerified"])) : null
                    });
                }

                return communities;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetCommunitiesForDropdownAsync()
        {
            try
            {
                var communities = await GetAllAsync();

                return await Task.FromResult(communities.Select(c => new SelectListItem { Value = c.CommunityID.ToString(), Text = c.IsVerified.GetValueOrDefault() ? $"{c.Name} ✓" : c.Name }));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Community>> GetByCategoryIdAsync(int categoryId)
        {
            const string sql = @"
            SELECT * FROM Communities 
            WHERE CategoryID = @categoryId 
            ORDER BY Name";

            var parameters = new Dictionary<string, object>
        {
            { "@categoryId", categoryId }
        };

            var result = await _db.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(result.Rows.Cast<DataRow>().Select(MapToCommunity));
        }

        public async Task<bool> AddMemberAsync(int communityId, string userId)
        {
            const string sql = @"
            UPDATE Communities 
            SET MemberCount = MemberCount + 1 
            WHERE CommunityID = @communityId;
            
            INSERT INTO CommunityMembers (CommunityID, UserId, JoinedAt)
            VALUES (@communityId, @userId, @joinedAt)";

            var parameters = new Dictionary<string, object>
        {
            { "@communityId", communityId },
            { "@userId", userId },
            { "@joinedAt", DateTime.UtcNow }
        };

            try
            {
                await await Task.FromResult(_db.ExecuteNonQueryAsync(sql, parameters));
                return true;
            }
            catch (Exception ex)
            {
             
                return false;
            }
        }

        public async Task<Community> CreateAsync(CommunityViewModel viewModel, String Id)
        {

            
            Community community = new Community()
            {
                CategoryID = viewModel.CategoryID,
                Name = viewModel.Name,
                IsPrivate = viewModel.IsPrivate,
                IsVerified = viewModel.IsVerified,
                Slug = viewModel.Slug,
                Rules = viewModel.Rules,
                Description = viewModel.Description,
                CreatorId = Id

            };
            const string sql = @"
            INSERT INTO Communities (
                Name, Slug, Description, Rules, CoverImage, 
                IconImage, CreatorId, IsPrivate, IsVerified, 
                MemberCount, CreatedAt, UpdatedAt, CategoryID
            ) VALUES (
                @name, @slug, @description, @rules, @coverImage,
                @iconImage, @creatorId, @isPrivate, @isVerified,
                @memberCount, @createdAt, @updatedAt, @categoryId
            );
            SELECT SCOPE_IDENTITY();";


            var parameters = await Task.FromResult(CreateParameterDictionary(community));

            int id = await _db.ExecuteScalarAsync<int>(sql, parameters);
            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Communities WHERE CommunityID = @id";

            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

             await Task.FromResult(_db.ExecuteNonQueryAsync(sql, parameters));
        }


        public async Task<Community?> GetByIdAsync(int id)
        {
            const string sql = @"
            SELECT * FROM Communities 
            WHERE CommunityID = @id";

            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

            var result = await await Task.FromResult(_db.ExecuteQueryAsync(sql, parameters));

            return result.Rows.Count > 0 ? await Task.FromResult(MapToCommunity(result.Rows[0])) : null;
        }

        public async Task<Community> GetBySlugAsync(string slug)
        {
            const string sql = @"
            SELECT * FROM Communities 
            WHERE Slug = @slug";

            var parameters = new Dictionary<string, object>
        {
            { "@slug", slug }
        };

            var result = await _db.ExecuteQueryAsync(sql, parameters);
            return result.Rows.Count > 0 ? await Task.FromResult(MapToCommunity(result.Rows[0])) : null;
        }


        public async Task<int> GetMemberCountAsync(int communityId)
        {
            const string sql = @"
            SELECT MemberCount 
            FROM Communities 
            WHERE CommunityID = @communityId";

            var parameters = new Dictionary<string, object>
                {
                    { "@communityId", communityId }
                };

            return await _db.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<bool> RemoveMemberAsync(int communityId, string userId)
        {
            const string sql = @"
            UPDATE Communities 
            SET MemberCount = MemberCount - 1 
            WHERE CommunityID = @communityId;
            
            DELETE FROM CommunityMembers 
            WHERE CommunityID = @communityId AND UserId = @userId";

            var parameters = new Dictionary<string, object>
                {
                    { "@communityId", communityId },
                    { "@userId", userId }
                };

            try
            {
                await Task.FromResult(_db.ExecuteNonQueryAsync(sql, parameters));
                return true;
            }
            catch (Exception ex)
            {             

                return false;
            }
        }

        public async Task<Community> UpdateAsync(Community community)
        {
            const string sql = @"
            UPDATE Communities SET 
                Name = @name,
                Slug = @slug,
                Description = @description,
                Rules = @rules,
                CoverImage = @coverImage,
                IconImage = @iconImage,
                IsPrivate = @isPrivate,
                IsVerified = @isVerified,
                MemberCount = @memberCount,
                UpdatedAt = @updatedAt,
                CategoryID = @categoryId
            WHERE CommunityID = @id";

            var parameters = await Task.FromResult(CreateParameterDictionary(community));
            parameters.Add("@id", community.CommunityID);

            await Task.FromResult(_db.ExecuteNonQueryAsync(sql, parameters));
            return await GetByIdAsync(community.CommunityID);
        }

        private static Community MapToCommunity(DataRow row)
        {
            return new Community
            {
                CommunityID = Convert.ToInt32(row["CommunityID"]),
                Name = row["Name"].ToString(),
                Slug = row["Slug"].ToString(),
                Description = row["Description"]?.ToString(),
                Rules = row["Rules"]?.ToString(),
                CoverImage = row["CoverImage"]?.ToString(),
                IconImage = row["IconImage"]?.ToString(),
                CreatorId = row["CreatorId"]?.ToString(),
                IsPrivate = row["IsPrivate"] != DBNull.Value && Convert.ToBoolean(row["IsPrivate"]),
                IsVerified = row["IsVerified"] != DBNull.Value && Convert.ToBoolean(row["IsVerified"]),
                MemberCount = row["MemberCount"] != DBNull.Value ? Convert.ToInt32(row["MemberCount"]) : 0,
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.UtcNow,
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : DateTime.UtcNow,
                CategoryID = row["CategoryID"] != DBNull.Value ? Convert.ToInt32(row["CategoryID"]) : null
            };
        }

        private static Dictionary<string, object> CreateParameterDictionary(Community community)
        {
            return new Dictionary<string, object>
            {
                { "@name", community.Name },
                { "@slug", community.Slug },
                { "@description", (object)community.Description ?? DBNull.Value },
                { "@rules", (object)community.Rules ?? DBNull.Value },
                { "@coverImage", (object)community.CoverImage ?? DBNull.Value },
                { "@iconImage", (object)community.IconImage ?? DBNull.Value },
                { "@creatorId", (object)community.CreatorId ?? DBNull.Value },
                { "@isPrivate", (object)community.IsPrivate ?? DBNull.Value },
                { "@isVerified", (object)community.IsVerified ?? DBNull.Value },
                { "@memberCount", (object)community.MemberCount ?? DBNull.Value },
                { "@createdAt", (object)community.CreatedAt ?? DateTime.UtcNow },
                { "@updatedAt", (object)community.UpdatedAt ?? DateTime.UtcNow },
                { "@categoryId", (object)community.CategoryID ?? DBNull.Value }
            };
            //return new Dictionary<string, object>
            //{
            //    {"@name", community.Name},
            //    {"@slug",community.Slug },
            //    {"@description", (object)community.Description ?? DBNull.Value},
            //    {"@rules",(object)community.Rules ?? DBNull.Value},
            //    {"@CreatorId",(object),community.Creator }
            //};
        }


    }
}
