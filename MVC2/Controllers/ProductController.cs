using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YourProject.Data;
using YourProject.Models;

namespace YourProject.Controllers
{
    public class ProductsController : Controller
    {
        private AppDbContext db;

        public ProductsController(AppDbContext context)
        {
            db = context;
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(
                db.Categories.ToList(), "Id", "Name");

            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index", "Category");
            }

            ViewBag.Categories = new SelectList(
                db.Categories.ToList(), "Id", "Name");

            return View(product);
        }
    }
}