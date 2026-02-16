using InsureYouAI.Context;
using InsureYouAINew.Models;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.DashboardViewComponents
{
    public class _DashboardPolicyTypesComponentPartial : ViewComponent
    {
        private readonly InsureContext _context;
        public _DashboardPolicyTypesComponentPartial(InsureContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var result = _context.Policies.GroupBy(x => x.PolicyType)
                        .Select(g => new PolicyGroupViewModel
                        {
                            PolicyType = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

            ViewBag.TotalPolicyCount = result.Sum(x => x.Count);


            return View(result);
        }
    }
}
