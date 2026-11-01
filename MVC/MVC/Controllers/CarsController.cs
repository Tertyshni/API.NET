using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace CarCatalog.Controllers
{
    public class CarsController : Controller
    {
        public ActionResult Index()
        {
            List<Car> cars = new List<Car>
            {
                new Car { Marka = "BMW", Model = "X5", Price = 45000 },
                new Car { Marka = "Audi", Model = "A6", Price = 38000 },
                new Car { Marka = "Toyota", Model = "Camry", Price = 30000 },
                new Car { Marka = "Mercedes", Model = "E-Class", Price = 50000 }
            };

            return View(cars);
        }
    }
}