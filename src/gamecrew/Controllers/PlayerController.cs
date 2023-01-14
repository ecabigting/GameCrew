using System;
using gamecrew.Helpers;
using gamecrew.Models;
using gamecrew.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gamecrew.Controllers;

public class PlayerController : Controller
{
    private readonly IHttpContextAccessor _httpCtxtAcc;
    private ISession _session => _httpCtxtAcc.HttpContext.Session;
    private PlayerContext PlayerCtxt { get; set; }
    IGroupServices GroupServices { get; }
    IPlayerService PlayerServices { get; }

    public PlayerController(IPlayerService _pService, IGroupServices _gService, IHttpContextAccessor httpContextAccessor) 
    {
        GroupServices = _gService;
        PlayerServices = _pService;
        _httpCtxtAcc = httpContextAccessor;
        PlayerCtxt = _session.GetObjectFromJson<PlayerContext>("playerContext");
    }

    [HttpGet]
    public IActionResult Index(string playerId)
    {
        if (PlayerCtxt != null)
        {
            return View(PlayerServices.Get(playerId));
        }
        else
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }
}