using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace discussionspot9.Services
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
        // In Services/ViewRenderService.cs

        public async Task<string> RenderToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using var sw = new StringWriter();

            // Try multiple approaches to find the view
            ViewEngineResult viewResult = null;

            // Approach 1: FindView
            viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            // Approach 2: GetView if FindView failed
            if (viewResult.View == null)
            {
                viewResult = _razorViewEngine.GetView("", viewName, false);
            }

            // Approach 3: Try with explicit path
            if (viewResult.View == null && !viewName.StartsWith("~/"))
            {
                var explicitPath = $"~/Views/Shared/{viewName}.cshtml";
                viewResult = _razorViewEngine.GetView("", explicitPath, false);
            }

            if (viewResult.View == null)
            {
                var searchedLocations = viewResult.SearchedLocations != null
                    ? string.Join(Environment.NewLine, viewResult.SearchedLocations)
                    : "No searched locations available";

                throw new InvalidOperationException($"View '{viewName}' not found. Searched locations:{Environment.NewLine}{searchedLocations}");
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
