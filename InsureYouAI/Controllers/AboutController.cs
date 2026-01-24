using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace InsureYouAI.Controllers
{
    public class AboutController : Controller
    {
        private readonly InsureContext _context;
        public AboutController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult AboutList()
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Mevcut Hakkımızda Yazısı";
            var values = _context.Abouts.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateAbout()
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Yeni Hakkımızda Yazı Girişi (Tema Bütünlüğünü Korumak İçin 1 Adet Hakkımızda Yazısı Giriniz)";
            return View();
        }

        [HttpPost]
        public IActionResult CreateAbout(About about)
        {
            _context.Abouts.Add(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

        public IActionResult DeleteAbout(int id)
        {
            var value = _context.Abouts.Find(id);
            _context.Abouts.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

        [HttpGet]
        public IActionResult UpdateAbout(int id)
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Hakkımızda Yazı Güncelleme Sayfası";
            var value = _context.Abouts.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateAbout(About about)
        {
            _context.Abouts.Update(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

       
    }
}
