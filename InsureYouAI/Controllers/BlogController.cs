using InsureYouAI.Context;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InsureYouAI.Controllers
{
    public class BlogController : Controller
    {
        private readonly InsureContext _context;
        public BlogController(InsureContext context)
        {
            _context = context;
        }
        public IActionResult BlogList()
        {
            return View();
        }

        public IActionResult GetBlogByCategory(int id)
        {
            ViewBag.c = id;
            return View();
        }

        public IActionResult BlogDetail(int id)
        {
            ViewBag.i = id;
            return View();
        }

        public PartialViewResult GetBlog()
        {
            return PartialView();
        }

        [HttpPost]
        public IActionResult GetBlog(string keyword)
        {
            return View();
        }

        [HttpGet]
        public PartialViewResult AddComment()
        {

            return PartialView();
        }


    }

}
