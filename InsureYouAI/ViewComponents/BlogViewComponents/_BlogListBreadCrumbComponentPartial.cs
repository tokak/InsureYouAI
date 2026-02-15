using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.BlogViewComponents
{
    public class _BlogListBreadCrumbComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
