using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAI.Controllers
{
    public class AboutItemController : Controller
    {
        private readonly InsureContext _context;
        public AboutItemController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult AboutItemList()
        {
            ViewBag.ControllerName = "Hakkımızda Ögeleri";
            ViewBag.PageName = "Mevcut Hakkımızda Ögeleri";
            var values = _context.AboutItems.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateAboutItem()
        {
            ViewBag.ControllerName = "Hakkımızda Ögeleri";
            ViewBag.PageName = "Yeni Hakkımızda Öge Girişi";
            return View();
        }

        [HttpPost]
        public IActionResult CreateAboutItem(AboutItem aboutItem)
        {
            _context.AboutItems.Add(aboutItem);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }

        [HttpGet]
        public IActionResult UpdateAboutItem(int id)
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Hakkımızda Ögeleri Güncelleme Sayfası";
            var value = _context.AboutItems.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateAboutItem(AboutItem aboutItem)
        {
            _context.AboutItems.Update(aboutItem);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }

        public IActionResult DeleteAboutItem(int id)
        {
            var value = _context.AboutItems.Find(id);
            _context.AboutItems.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("AboutItemList");
        }
    }
}
