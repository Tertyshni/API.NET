using Microsoft.AspNetCore.Mvc;
using YourProject.Data;
using YourProject.Models;

namespace YourProject.Controllers
{
    public class CategoryController : Controller
    {
        private AppDbContext db;

        public CategoryController(AppDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(category);
        }
    }
}