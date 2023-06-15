using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BabyPedia.Controllers
{
    public class WelcomeController : Controller
    {
        public IActionResult ParentWelcome()
        {
            return View();
        }
        /*
         * 
access_token$sandbox$j3cyrbgqf4shn93p$639d620307359ca74bbc7e9f56dc8179
         */

        public IActionResult PediaWelcome()
        {
            return View();
        }
    }
}