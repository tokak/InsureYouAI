using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutNavbarComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
