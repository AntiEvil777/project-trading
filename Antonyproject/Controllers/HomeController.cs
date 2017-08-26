using Antonyproject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Antonyproject.Controllers
{
    public class HomeController : Controller
    {
        private MyDatabaseEntities db = new MyDatabaseEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail([Bind(Include = "emailID,name,email,subject,category,message")] Email emailModel)
        {
            if (ModelState.IsValid)
            {                
                db.Emails.Add(emailModel);
                db.SaveChanges();
            }
            EmailUtility.sendMail(emailModel.email, emailModel.message, emailModel.subject);
            return View("Index");
        }
    }
}