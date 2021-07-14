using FileUploader.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FileUploader.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Runtime.CompilerServices;

namespace FileUploader.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString =
                 @"Data Source=.\sqlexpress;Initial Catalog=FileUploader;Integrated Security=true;";

        private readonly IWebHostEnvironment _environment;

        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        public IActionResult Upload(Image image, IFormFile imageFile)
        {
            
            string actualFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string finalFileName = Path.Combine(_environment.WebRootPath, "uploads", actualFileName);
            using (FileStream fs = new FileStream(finalFileName, FileMode.CreateNew)) { 
                imageFile.CopyTo(fs);
            }

            image.FileName = actualFileName;
            UploadDb db = new UploadDb(_connectionString);
            db.AddImage(image);
            return View(image);
        }

        public IActionResult ViewImage(int id)
        {
            var vm = new ViewImageModel();
            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }

            if (!HasPermission(id))
            {
                vm.HasPermission = false;
                vm.Image = new Image { Id = id };
            }
            else
            {
                vm.HasPermission = true;
                var db = new UploadDb(_connectionString);
                db.IncrementViewCount(id);
                var image = db.GetById(id);
                if(image == null)
                {
                    return Redirect("/");
                }

                vm.Image = image;

            }

            return View(vm);
        }

        private bool HasPermission(int id)
        {
            var allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
            if (allowedIds == null)
            {
                return false;
            }
            return allowedIds.Contains(id);
        }
        [HttpPost]
        public IActionResult ViewImage(int id, string password)
        {
            UploadDb db = new UploadDb(_connectionString);
            var image = db.GetById(id);
            if (image == null)
            {
                return Redirect("/");
            }

            if (password != image.Password)
            {
                TempData["message"] = "Invalid Password";
            }
            else
            {
                var allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
                if (allowedIds == null)
                {
                    allowedIds = new List<int>();
                }
                allowedIds.Add(id);
                HttpContext.Session.Set("allowedids", allowedIds);
            }

            return Redirect($"/Home/Viewimage?id={id}");
        }
       
    }
}
