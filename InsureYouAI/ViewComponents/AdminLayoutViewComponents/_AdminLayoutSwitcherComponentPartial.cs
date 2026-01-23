using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutSwitcherComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
