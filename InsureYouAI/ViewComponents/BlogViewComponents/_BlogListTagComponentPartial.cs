using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.BlogViewComponents
{
    public class _BlogListTagComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
