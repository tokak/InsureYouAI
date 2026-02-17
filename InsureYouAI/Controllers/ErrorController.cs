using Microsoft.AspNetCore.Mvc;

namespace InsureYouAI.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult Error404()
        {
            return View("Page404");
        }

       

        [Route("Error/{code}")]
        public IActionResult HandleErrorCode(int code)
        {
            return code switch
            {
                404 => View("Page404"),
                _ => View("PageGeneric")
            };
        }
    }

}
