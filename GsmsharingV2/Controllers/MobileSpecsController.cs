using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.Controllers
{
    public class MobileSpecsController : Controller
    {
        private readonly IMobileSpecsService _mobileSpecsService;
        private readonly ILogger<MobileSpecsController> _logger;

        public MobileSpecsController(
            IMobileSpecsService mobileSpecsService,
            ILogger<MobileSpecsController> logger)
        {
            _mobileSpecsService = mobileSpecsService;
            _logger = logger;
        }

        // GET: /mobiles - List all mobile models
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            var models = await _mobileSpecsService.GetAllModelsAsync(page, pageSize);
            var totalCount = await _mobileSpecsService.GetTotalModelsCountAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            return View(models);
        }

        // GET: /mobiles/brand/{slug} - List models by brand
        [HttpGet]
        public async Task<IActionResult> Brand(string slug)
        {
            var brand = await _mobileSpecsService.GetBrandBySlugAsync(slug);
            if (brand == null)
            {
                return NotFound($"Brand '{slug}' not found");
            }

            var models = await _mobileSpecsService.GetModelsByBrandAsync(brand.BrandID);
            ViewBag.Brand = brand;
            return View("BrandModels", models);
        }

        // GET: /mobiles/search - Search models
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            var models = await _mobileSpecsService.SearchModelsAsync(q);
            ViewBag.SearchQuery = q;
            return View("SearchResults", models);
        }

        // GET: /mobiles/compare - Compare models
        [HttpGet]
        public IActionResult Compare()
        {
            return View();
        }

        // POST: /mobiles/compare - Get models for comparison
        [HttpPost]
        public async Task<IActionResult> Compare(List<int> modelIds)
        {
            if (modelIds == null || modelIds.Count < 2)
            {
                ModelState.AddModelError("", "Please select at least 2 models to compare");
                return View();
            }

            var models = await _mobileSpecsService.GetModelsForComparisonAsync(modelIds);
            return View(models);
        }

        // GET: /mobiles/details/{brandSlug}/{modelSlug} - Model details with SEO
        [HttpGet]
        public async Task<IActionResult> Details(string brandSlug, string modelSlug)
        {
            var model = await _mobileSpecsService.GetModelBySlugAndBrandAsync(brandSlug, modelSlug);
            if (model == null)
            {
                return NotFound($"Model '{modelSlug}' not found");
            }

            return View(model);
        }

        // GET: /mobiles/brands - List all brands
        [HttpGet]
        public async Task<IActionResult> Brands()
        {
            var brands = await _mobileSpecsService.GetAllBrandsAsync();
            return View(brands);
        }

        // GET: /mobiles/price-range - Filter by price range
        [HttpGet]
        public async Task<IActionResult> PriceRange(decimal min, decimal max)
        {
            var models = await _mobileSpecsService.GetModelsByPriceRangeAsync(min, max);
            ViewBag.MinPrice = min;
            ViewBag.MaxPrice = max;
            return View("SearchResults", models);
        }

        // GET: /mobiles/create - Create new model (Admin/Editor only)
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var brands = await _mobileSpecsService.GetAllBrandsAsync();
            ViewBag.Brands = brands;
            return View();
        }

        // POST: /mobiles/create - Create new model
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MobileModel model)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _mobileSpecsService.GetAllBrandsAsync();
                ViewBag.Brands = brands;
                return View(model);
            }

            try
            {
                var createdModel = await _mobileSpecsService.CreateModelAsync(model);
                return RedirectToAction("Details", new { 
                    brandSlug = createdModel.Brand?.Slug, 
                    modelSlug = createdModel.Slug 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mobile model");
                ModelState.AddModelError("", "Error creating mobile model");
                var brands = await _mobileSpecsService.GetAllBrandsAsync();
                ViewBag.Brands = brands;
                return View(model);
            }
        }

        // GET: /mobiles/edit/{id} - Edit model (Admin/Editor only)
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _mobileSpecsService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var brands = await _mobileSpecsService.GetAllBrandsAsync();
            ViewBag.Brands = brands;
            return View(model);
        }

        // POST: /mobiles/edit/{id} - Edit model
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MobileModel model)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _mobileSpecsService.GetAllBrandsAsync();
                ViewBag.Brands = brands;
                return View(model);
            }

            try
            {
                var updatedModel = await _mobileSpecsService.UpdateModelAsync(model);
                return RedirectToAction("Details", new { 
                    brandSlug = updatedModel.Brand?.Slug, 
                    modelSlug = updatedModel.Slug 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mobile model");
                ModelState.AddModelError("", "Error updating mobile model");
                var brands = await _mobileSpecsService.GetAllBrandsAsync();
                ViewBag.Brands = brands;
                return View(model);
            }
        }

        // POST: /mobiles/delete/{id} - Delete model (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mobileSpecsService.DeleteModelAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mobile model");
                return RedirectToAction("Index");
            }
        }
    }
}
