using gsmsharing.Database;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace gsmsharing.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {

        private readonly DatabaseConnection _dbConnection;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(DatabaseConnection dbConnection, ILogger<CategoryRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public Task<Category> CreateAsync(Category category)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                string sql = @"SELECT CategoryID, Name, Slug, ParentCategoryID, Description, 
                               MetaTitle, MetaDescription, OgTitle, OgDescription, OgImage, 
                               IconClass, DisplayOrder, IsActive, CreatedAt, UpdatedAt, 
                               CreatedBy, UpdatedBy 
                               FROM Categories";

                DataTable result = await _dbConnection.ExecuteQueryAsync(sql);
                return ConvertDataTableToCategories(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all categories");
                throw;
            }
        }

        public async Task<IEnumerable<SelectListItem>> CreateCategorySelectListAsync()
        {
            IEnumerable<Category> categories = await GetAllAsync();
            var selectList = new List<SelectListItem>();
            var parentCategories = categories.Where(c => c.ParentCategoryID == null).OrderBy(c => c.DisplayOrder);

            foreach (var parentCategory in parentCategories)
            {
                // Add parent category as disabled option
                selectList.Add(new SelectListItem
                {
                    Value = parentCategory.CategoryID.ToString(),
                    Text = parentCategory.Name,                    
                    Disabled = true
                });

                // Add subcategories
                var subcategories = categories.Where(c => c.ParentCategoryID == parentCategory.CategoryID)
                                              .OrderBy(c => c.DisplayOrder);
                foreach (var subcategory in subcategories)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = subcategory.CategoryID.ToString(),
                        Text = $"- {subcategory.Name}"
                    });
                }
            }

            return selectList;
        }

        public async Task<IEnumerable<CategoryViewModel>> GetHierarchicalCategoriesAsync()
        {
            //try
            //{
            //    // Get all categories from database
            //    var allCategories = await GetAllAsync();
            //    var hierarchicalCategories = new List<CategoryViewModel>();

            //    // Get parent categories
            //    var parentCategories = allCategories.Where(c => c.ParentCategoryID == null)
            //                                      .OrderBy(c => c.DisplayOrder);

            //    // Build hierarchical structure
            //    foreach (var parent in parentCategories)
            //    {
            //        var categoryViewModel = string.Empty;
            //        //var categoryViewModel = new CategoryViewModel
            //        //{
            //        //    CategoryID = parent.CategoryID,
            //        //    Name = parent.Name,
            //        //    ParentCategoryID = parent.ParentCategoryID,
            //        //    IconClass = parent.IconClass,
            //        //    Slug = parent.Slug,
            //        //    Description = parent.Description,
            //        //    DisplayOrder = parent.DisplayOrder,
            //        //    SubCategories = allCategories.Where(c => c.ParentCategoryID == parent.CategoryID)
            //        //                               .OrderBy(c => c.DisplayOrder)
            //        //                               .ToList()
            //        //};

            //        hierarchicalCategories.Add(categoryViewModel);
            //    }

            //    return hierarchicalCategories;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error occurred while building hierarchical categories");
            //    throw;
            //}
            return null;
        }

        public async Task<IEnumerable<CategoryViewModel>> GetHierarchicalCategoriesTestAsync()
        {
            try
            {
                // Get all categories from database
                var allCategories = await GetAllAsync();
                var hierarchicalCategories = new List<CategoryViewModel>();

                // Get parent categories
                var parentCategories = allCategories.Where(c => c.ParentCategoryID == null)
                                                  .OrderBy(c => c.DisplayOrder);

                foreach (var parent in parentCategories)
                {
                    var subcategories = allCategories
                        .Where(c => c.ParentCategoryID == parent.CategoryID)
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CategoryViewModel
                        {
                            CategoryID = c.CategoryID,
                            Name = c.Name,
                            ParentCategoryID = c.ParentCategoryID,
                            IconClass = c.IconClass,
                            Slug = c.Slug,
                            Description = c.Description,
                            DisplayOrder = c.DisplayOrder,
                            ItemCount = GetCategoryItemCount(c.CategoryID) // You'll need to implement this method
                        })
                        .ToList();

                    var categoryViewModel = new CategoryViewModel
                    {
                        CategoryID = parent.CategoryID,
                        Name = parent.Name,
                        ParentCategoryID = parent.ParentCategoryID,
                        IconClass = parent.IconClass,
                        Slug = parent.Slug,
                        Description = parent.Description,
                        DisplayOrder = parent.DisplayOrder,
                        ItemCount = GetCategoryItemCount(parent.CategoryID) + subcategories.Sum(s => s.ItemCount),
                        SubCategories = subcategories
                    };

                    hierarchicalCategories.Add(categoryViewModel);
                }

                return hierarchicalCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while building hierarchical categories");
                throw;
            }
        }

        private int GetCategoryItemCount(int categoryId)
        {
            // Implement this method to get the count of items in each category
            // This could be a database query to count related items
            return 0; // Placeholder return
        }

        public Task<Category> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetByParentIdAsync(int parentId)
        {
            throw new NotImplementedException();
        }

        public Task<Category> GetBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Category> UpdateAsync(Category category)
        {
            throw new NotImplementedException();
        }

        //converting Category to Proper Model
        private IEnumerable<Category> ConvertDataTableToCategories(DataTable dataTable)
        {
            List<Category> categories = new List<Category>();

            foreach (DataRow row in dataTable.Rows)
            {
                categories.Add(new Category
                {
                    CategoryID = Convert.ToInt32(row["CategoryID"]),
                    Name = row["Name"].ToString(),
                    Slug = row["Slug"].ToString(),
                    ParentCategoryID = row["ParentCategoryID"] != DBNull.Value ? Convert.ToInt32(row["ParentCategoryID"]) : null,
                    Description = row["Description"].ToString(),
                    MetaTitle = row["MetaTitle"].ToString(),
                    MetaDescription = row["MetaDescription"].ToString(),
                    OgTitle = row["OgTitle"].ToString(),
                    OgDescription = row["OgDescription"].ToString(),
                    OgImage = row["OgImage"].ToString(),
                    IconClass = row["IconClass"].ToString(),
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                    CreatedBy = row["CreatedBy"].ToString(),
                    UpdatedBy = row["UpdatedBy"].ToString()
                });
            }

            return categories;
        }
    }
}
