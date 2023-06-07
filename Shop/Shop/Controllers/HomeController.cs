using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _dbContext.Product.Include(u => u.Category),
                Categories = _dbContext.Category
            };
            return View(homeVM);
        }

        public IActionResult Details(int id)
        {
            List<Cart> cartList = new List<Cart>();
            if (HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                cartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            DetailsVM  detailsVM = new()
            {
                Product = _dbContext.Product.Include(u => u.Category)
                    .Where(u => u.Id == id).FirstOrDefault(),
                ExistsInCart = false
            };

            foreach(var item in cartList)
            {
                if(item.ProductId == id)
                {
                    detailsVM.ExistsInCart = true;
                }
            }

            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<Cart> cartList = new List<Cart>();

            if(HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                cartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            cartList.Add(new Cart { ProductId = id });
            HttpContext.Session.Set(WebConstants.SessionCart, cartList);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<Cart> cartList = new List<Cart>();

            if(HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                cartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            var itemToRemove = cartList.SingleOrDefault(i => i.ProductId == id);
            if(itemToRemove != null)
            {
                cartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WebConstants.SessionCart, cartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
