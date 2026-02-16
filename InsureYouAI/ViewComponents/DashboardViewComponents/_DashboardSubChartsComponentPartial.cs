using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.DashboardViewComponents
{
    public class _DashboardSubChartsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
