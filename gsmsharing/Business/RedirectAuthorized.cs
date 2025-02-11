namespace gsmsharing.Business
{
    public class RedirectAuthorized
    {
        private readonly RequestDelegate _delegate;

        public RedirectAuthorized(RequestDelegate @delegate)
        {
            _delegate = @delegate;
        }

        public async Task Invoke(HttpContext context)
        {

            if (context.User.Identity.IsAuthenticated &&
                context.Request.Path.StartsWithSegments("/UserAccounts/Login") ||
                context.Request.Path.StartsWithSegments("/UserAccounts/Register"))
            {

                context.Response.Redirect("/");
                return;
            }
            await _delegate(context);
        }
    }
}
