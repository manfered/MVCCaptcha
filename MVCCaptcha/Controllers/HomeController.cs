using MVCCaptcha.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCCaptcha.Controllers
{
    public class HomeController : Controller
    {


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string CaptchaUserInput)
        {
            var resultStr = string.Empty;
            var captchaStr = ControllerContext.HttpContext.Session["Captcha"];
            CaptchaUserInput = CaptchaUserInput.ToLower();
            if (String.Equals(captchaStr, CaptchaUserInput)){
                resultStr = "Captcha is entered correctly!!";
            }
            else
            {
                resultStr = "Captcha re-enter captcha!";
            }
            
            return Json(new { Data = resultStr, captchaStr = captchaStr , CaptchaUserInput = CaptchaUserInput }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Farzad(Fred) Seifi";

            return View();
        }


        public ActionResult GenerateCaptcha()
        {
            Captcha captcha = new Captcha(6, 200, 100, ControllerContext);
            return File(captcha.create_captcha(), "image/jpeg");
        }
        
    }
}