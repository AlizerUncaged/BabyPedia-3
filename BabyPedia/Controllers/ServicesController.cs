using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BabyPedia.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult OnlineConsultation()
        {
            return View();
        }

        public IActionResult Vaccination()
        {
            return View();
        }
        //
        // public IActionResult Appointment()
        // {
        //     return View();
        // }

        public IActionResult RecordTracking()
        {
            return View();
        }
    }
}