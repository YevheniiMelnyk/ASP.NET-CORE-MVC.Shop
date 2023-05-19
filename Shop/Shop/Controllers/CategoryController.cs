using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;
using System.Collections.Generic;

namespace Shop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _dbContext.Category;
            return View(categoryList);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Category category)
        {
            _dbContext.Category.Add(category);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
