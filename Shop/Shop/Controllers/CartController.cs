using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            List<Cart> shoppingCartList = new();
            if (HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

            IEnumerable<Product> prodList = _context.Product.Where(p => prodInCart.Contains(p.Id));

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<Cart> shoppingCartList = new();
            if (HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _context.Product.Where(p => prodInCart.Contains(p.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _context.AppUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = prodList.ToList()
            };

            return View(ProductUserVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

            var subject = "New Inquiry";
            string htmlBody = "";

            using(StreamReader sr = System.IO.File.OpenText(pathToTemplate))
            {
                htmlBody = sr.ReadToEnd();
            }

            StringBuilder productListSb = new();

            foreach (var prod in ProductUserVM.ProductList)
            {
                productListSb.Append($" - Name: {prod.Name} <span style='font-size:14px;'>(ID: {prod.Id}</span><br />");
            }

            string msg = string.Format(htmlBody, 
                productUserVM.ApplicationUser.FullName, 
                productUserVM.ApplicationUser.Email, 
                productUserVM.ApplicationUser.PhoneNumber, 
                productListSb.ToString());

            await _emailSender.SendEmailAsync(WebConstants.EmailAdmin, subject, msg);
            
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(ProductUserVM productUserVM)
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
        {
            List<Cart> shoppingCartList = new();
            if(HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<IEnumerable<Cart>>(WebConstants.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<Cart>>(WebConstants.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(i => i.ProductId == id));

            HttpContext.Session.Set(WebConstants.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }
    }
}
