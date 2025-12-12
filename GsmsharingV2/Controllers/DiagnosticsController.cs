using GsmsharingV2.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(ApplicationDbContext context, ILogger<DiagnosticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var diagnostics = new Dictionary<string, object>();

            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                diagnostics["DatabaseConnection"] = canConnect ? "✅ Connected" : "❌ Failed";

                if (canConnect)
                {
                    // Check table counts
                    diagnostics["MobilePosts_Total"] = await _context.MobilePosts.CountAsync();
                    diagnostics["MobilePosts_Published"] = await _context.MobilePosts.CountAsync(mp => mp.publish == 1);
                    
                    diagnostics["GsmBlog_Total"] = await _context.GsmBlogs.CountAsync();
                    diagnostics["GsmBlog_Published"] = await _context.GsmBlogs.CountAsync(gb => gb.Publish == true);
                    
                    diagnostics["Forums_Total"] = await _context.UsersFourm.CountAsync();
                    diagnostics["Forums_Published"] = await _context.UsersFourm.CountAsync(f => f.Publish == 1);
                    
                    diagnostics["Posts_Total"] = await _context.Posts.CountAsync();
                    diagnostics["AffiliateProducts_Total"] = await _context.AffiliationProducts.CountAsync();
                    diagnostics["Users_Total"] = await _context.Users.CountAsync();
                    diagnostics["Communities_Total"] = await _context.Communities.CountAsync();
                    
                    // New tables with existing data
                    diagnostics["BlogSEO_Total"] = await _context.BlogSEO.CountAsync();
                    diagnostics["BlogComments_Total"] = await _context.BlogComments.CountAsync();
                    diagnostics["ProductReview_Total"] = await _context.ProductReview.CountAsync();
                    diagnostics["BlogSpecContainer_Total"] = await _context.BlogSpecContainer.CountAsync();

                    // Check if tables exist
                    diagnostics["MobilePosts_TableExists"] = await TableExists("MobilePosts");
                    diagnostics["GsmBlog_TableExists"] = await TableExists("GsmBlog");
                    diagnostics["UsersFourm_TableExists"] = await TableExists("UsersFourm");
                    diagnostics["AffiliationProgram_TableExists"] = await TableExists("AffiliationProgram");
                    diagnostics["BlogSEO_TableExists"] = await TableExists("BlogSEO");
                    diagnostics["BlogComments_TableExists"] = await TableExists("BlogComments");
                    diagnostics["ProductReview_TableExists"] = await TableExists("ProductReview");
                    diagnostics["BlogSpecContainer_TableExists"] = await TableExists("BlogSpecContainer");
                }
            }
            catch (Exception ex)
            {
                diagnostics["Error"] = ex.Message;
                diagnostics["StackTrace"] = ex.StackTrace;
                _logger.LogError(ex, "Diagnostics error");
            }

            ViewBag.Diagnostics = diagnostics;
            return View();
        }

        private async Task<bool> TableExists(string tableName)
        {
            try
            {
                var sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                var result = await command.ExecuteScalarAsync();
                await connection.CloseAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

