using System.Diagnostics;                     
using Microsoft.AspNetCore.Mvc;              
using WebApp.Models;                      

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;   // Logger for debugging and diagnostics

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;                                // Injected logger assigned
    }

    public IActionResult Index()
    {
        return View();                                   // Return main homepage view
    }

    public IActionResult Privacy()
    {
        return View();                                   // Return Privacy page view
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Disable response caching for the error page so errors aren't cached
    public IActionResult Error()
    {
        // Create ErrorViewModel with current request ID (for debugging/tracing)
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
