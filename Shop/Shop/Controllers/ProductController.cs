using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _dbContext.Product;

            foreach (var product in productList)
            {
                product.Category = _dbContext.Category.FirstOrDefault(x => x.CategoryId == product.CategoryId);
            }

            return View(productList);
        }

        // update / insert
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _dbContext.Category.Select(i => new SelectListItem
                {
                    Text = i.CategoryName,
                    Value = i.CategoryId.ToString()
                })
            };


            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _dbContext.Product.Find(id);
                if(productVM.Product == null )
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        // update / insert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if(productVM.Product.Id == 0)
                {
                    string upload = webRootPath + WebConstants.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;
                    _dbContext.Product.Add(productVM.Product);
                }
                else
                {
                    var prod = _dbContext.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);

                    if(files.Count > 0)
                    {
                        string upload = webRootPath + WebConstants.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, prod.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create);
                        files[0].CopyTo(fileStream);

                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = prod.Image;
                    }
                    _dbContext.Product.Update(productVM.Product);
                }


                //_dbContext.Category.Add(category);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _dbContext.Category.Select(i => new SelectListItem
            {
                Text = i.CategoryName,
                Value = i.CategoryId.ToString()
            });

            return View(productVM);
        }

        public IActionResult Delete(int? id)
        {
            Product product = _dbContext.Product.Include(q => q.Category).FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var product = _dbContext.Product.Find(id);
            if(product == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WebConstants.ImagePath;
            var oldFile = Path.Combine(upload, product.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _dbContext.Product.Remove(product);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
