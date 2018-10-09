﻿using System.Diagnostics;
using HQ.Cadence;
using HQ.Cadence.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Site.AspNetCore21.Views.Models;

namespace Site.AspNetCore21.Views.Controllers
{
    public class HomeController : Controller
    {
	    private readonly IMetricsHost<HomeController> _metrics;

	    public HomeController(IMetricsHost<HomeController> metrics)
	    {
		    _metrics = metrics;
	    }
		
		[Counter, Timer(TimeUnit.Seconds, TimeUnit.Seconds), Meter("requests", TimeUnit.Seconds), Histogram(SampleType.Biased, 1L)]
        public IActionResult Index()
        {
	        return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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
