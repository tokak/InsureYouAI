using InsureYouAI.Context;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.ViewComponents.DefaultViewComponents
{
    public class _DefaultSliderComponentPartial : ViewComponent
    {
        private readonly InsureContext _context;
        public _DefaultSliderComponentPartial(InsureContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var values = _context.Sliders.ToList();
            return View(values);
        }
    }
}
