using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutNavbarMenuComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
