

using System;
using System.Diagnostics;
using gamecrew.Helpers;
using gamecrew.Models;
using gamecrew.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gamecrew.Controllers;

public class HomeController : Controller
{
    private readonly IHttpContextAccessor _httpCtxtAcc;
    private ISession _session => _httpCtxtAcc.HttpContext.Session;
    private PlayerContext PlayerCtxt { get; set; }
    private AppSettings ASettings {get;set;}
    IPlayerService Service { get; }

    public HomeController(IHttpContextAccessor httpContextAccessor, IPlayerService _service, AppSettings _appS)
    {
        Service = _service;
        _httpCtxtAcc = httpContextAccessor;
        PlayerCtxt = _session.GetObjectFromJson<PlayerContext>("playerContext");
        ASettings = _appS;
    }

    public IActionResult Index()
    {
        TempData["CKey"] = ASettings.CaptchaKey;
        if (PlayerCtxt != null)
        {
            return RedirectToAction("Index","Dashboard");
        }
        else 
        {
            return View();
        }            
    }

    [HttpPost]
    public IActionResult Login(PlayerLogin _login) 
    {
        TempData["CKey"] = ASettings.CaptchaKey;
        try
        {
            if (ModelState.IsValid)
            {
                Player ExistingPlayer = Service.GetPlayerViaEmail(_login.Email);
                if (ExistingPlayer != null && Service.CheckPassword(ExistingPlayer, _login.Password))
                {
                    if (PlayerCtxt != null)
                    {
                        _session.Remove("playerContext");
                    }
                    PlayerCtxt = new PlayerContext
                    {
                        Profile = ExistingPlayer
                    };
                    HttpContext.Session.SetObjectAsJson("playerContext", PlayerCtxt);

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["Message"] = "Invalid Credentials";
                    return View("Index");
                }
            }
            else
            {
                TempData["Message"] = "Invalid Credentials";
                return View("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Registration!\n" + ex.Message;
            return View();
        }
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(Player _player)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Player GetPlayer = Service.GetPlayerViaEmail(_player.Email);
                if (GetPlayer == null)
                {
                    Player NewPlayer = Service.Create(_player);
                    TempData["Message"] = "Registration Success! Please login!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Email", "Email already used!");
                    return View(_player);
                }
            }
            else 
            {
                TempData["Message"] = "Error Registration!";
                return View();
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Registration!\n" + ex.Message;
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult Logout() 
    {
        try
        {
            _session.Remove("playerContext");
            if (PlayerCtxt != null)
            {
                string logoutMessage = "Hate to see you leave " + PlayerCtxt.Profile.Name + "! Please come back any time!";
                _session.Remove("playerContext");
                TempData["Message"] = logoutMessage;
                return RedirectToAction("Index","Home");
            }
            else 
            {
                TempData["Message"] = "Please Login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error logging out!\n" + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}

