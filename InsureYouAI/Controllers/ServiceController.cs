using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAI.Controllers
{
    public class ServiceController : Controller
    {
        private readonly InsureContext _context;
        public ServiceController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult ServiceList()
        {
            ViewBag.ControllerName = "Hizmelet";
            ViewBag.PageName = "Mevcut Sigorta Hizmetleri Listesi";
            var values = _context.Services.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            ViewBag.ControllerName = "Hizmetler";
            ViewBag.PageName = "Yeni Hizmet Yazısı Girişi";
            return View();
        }

        [HttpPost]
        public IActionResult CreateService(Service service)
        {
            _context.Services.Add(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        [HttpGet]
        public IActionResult UpdateService(int id)
        {
            ViewBag.ControllerName = "Hizmetler";
            ViewBag.PageName = "Hizmet Yazısı Güncelleme Sayfası";
            var value = _context.Services.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateService(Service service)
        {
            _context.Services.Update(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        public IActionResult DeleteService(int id)
        {
            var value = _context.Services.Find(id);
            _context.Services.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

    }
}
