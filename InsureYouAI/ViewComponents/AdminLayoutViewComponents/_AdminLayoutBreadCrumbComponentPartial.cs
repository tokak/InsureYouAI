using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutBreadCrumbComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
