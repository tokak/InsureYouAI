using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InsureYouAI.Controllers
{
    public class ArticleController : Controller
    {
        private readonly InsureContext _context;
        public ArticleController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult ArticleList()
        {
            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Makale Listesi";
            var values = _context.Articles.Include(x => x.AppUser).Include(y => y.Category).ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateArticle()
        {
            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Yeni Makale Oluştur";

            var categories = _context.Categories
                           .Select(x => new SelectListItem
                           {
                               Text = x.CategoryName,
                               Value = x.CategoryId.ToString()
                           })
                           .ToList();

            ViewBag.Categories = categories;

            var authors = _context.Users
                         .Select(x => new SelectListItem
                         {
                             Text = x.Name + " " + x.Surname,
                             Value = x.Id
                         })
                         .ToList();

            ViewBag.Authors = authors;

            return View();
        }

        [HttpPost]
        public IActionResult CreateArticle(Article article)
        {
            article.CreatedDate = DateTime.Now;
            _context.Articles.Add(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        [HttpGet]
        public IActionResult UpdateArticle(int id)
        {

            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Makale Güncelleme Sayfası";

            var categories = _context.Categories
                           .Select(x => new SelectListItem
                           {
                               Text = x.CategoryName,
                               Value = x.CategoryId.ToString()
                           })
                           .ToList();

            ViewBag.Categories = categories;

            var authors = _context.Users
                         .Select(x => new SelectListItem
                         {
                             Text = x.Name + " " + x.Surname,
                             Value = x.Id
                         })
                         .ToList();

            ViewBag.Authors = authors;

            var value = _context.Articles.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateArticle(Article article)
        {
            _context.Articles.Update(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        public IActionResult DeleteArticle(int id)
        {
            var value = _context.Articles.Find(id);
            _context.Articles.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }
    }
}
