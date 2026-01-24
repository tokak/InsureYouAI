using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAI.Controllers
{
    public class SliderController : Controller
    {
        private readonly InsureContext _context;
        public SliderController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult SliderList()
        {
            ViewBag.ControllerName = "Slider";
            ViewBag.PageName = "Ana Sayfa Slider Listesi";
            var values = _context.Sliders.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateSlider()
        {
            ViewBag.ControllerName = "Slider";
            ViewBag.PageName = "Ana Sayfa Yeni Slider Girişi";
            return View();
        }

        [HttpPost]
        public IActionResult CreateSlider(Slider slider)
        {
            _context.Sliders.Add(slider);
            _context.SaveChanges();
            return RedirectToAction("SliderList");
        }

        [HttpGet]
        public IActionResult UpdateSlider(int id)
        {
            ViewBag.ControllerName = "Slider";
            ViewBag.PageName = "Ana Sayfa Slider Güncelleme İşlemi";
            var value = _context.Sliders.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateSlider(Slider slider)
        {
            _context.Sliders.Update(slider);
            _context.SaveChanges();
            return RedirectToAction("SliderList");
        }

        public IActionResult DeleteSlider(int id)
        {
            var value = _context.Sliders.Find(id);
            _context.Sliders.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("SliderList");
        }
    }
}
