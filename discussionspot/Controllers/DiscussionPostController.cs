using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace discussionspot.Controllers
{
    public class DiscussionPostController : Controller
    {
       

     

        /// <summary>
        /// Gets all posts with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPosts(string sortBy = "hot", string timeFilter = "week", int? communityId = null)
        {
            return View();
        }

        /// <summary>
        /// Displays a single post with comments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Post(int id)
        {
            return View();
        }

        /// <summary>
        /// Displays the create post form
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreatePost()
        {
            return View();
        }
    }
  }
