using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.DashboardViewComponents
{
    public class _DashboardScriptsComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
