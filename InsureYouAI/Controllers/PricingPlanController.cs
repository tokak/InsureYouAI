using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAI.Controllers
{
    public class PricingPlanController : Controller
    {
        private readonly InsureContext _context;
        public PricingPlanController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult PricingPlanList()
        {
            ViewBag.ControllerName = "AI Destekli Sigorta Planı";
            ViewBag.PageName = "Mevcut Sigorta Plan Listeleri";
            var values = _context.PricingPlans.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreatePricingPlan()
        {
            ViewBag.ControllerName = "AI Destekli Sigorta Planı";
            ViewBag.PageName = "Yeni Sigorta Planı Oluşturma";
            return View();
        }

        [HttpPost]
        public IActionResult CreatePricingPlan(PricingPlan pricingPlan)
        {
            _context.PricingPlans.Add(pricingPlan);
            _context.SaveChanges();
            return RedirectToAction("PricingPlanList");
        }

        [HttpGet]
        public IActionResult UpdatePricingPlan(int id)
        {
            ViewBag.ControllerName = "AI Destekli Sigorta Planı";
            ViewBag.PageName = "Sigorta Plan Revizyonu";
            var value = _context.PricingPlans.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdatePricingPlan(PricingPlan pricingPlan)
        {
            _context.PricingPlans.Update(pricingPlan);
            _context.SaveChanges();
            return RedirectToAction("PricingPlanList");
        }

        public IActionResult DeletePricingPlan(int id)
        {
            var value = _context.PricingPlans.Find(id);
            _context.PricingPlans.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("PricingPlanList");
        }

        public IActionResult ChangeStatus(int id)
        {
            var value = _context.PricingPlans.Find(id);
            if (value.IsFeature == true)
            {
                value.IsFeature = false;
            }
            else
            {
                value.IsFeature = true;
            }
            _context.SaveChanges();
            return RedirectToAction("PricingPlanList");
        }
    }
}
