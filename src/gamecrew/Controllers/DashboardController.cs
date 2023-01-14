using System;
using System.Collections.Generic;
using gamecrew.Helpers;
using gamecrew.Models;
using gamecrew.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gamecrew.Controllers;

public class DashboardController : Controller
{
    private readonly IHttpContextAccessor _httpCtxtAcc;
    private ISession _session => _httpCtxtAcc.HttpContext.Session;
    private PlayerContext PlayerCtxt { get; set; }
    IGroupServices GroupServices { get; }
    IPlayerService PlayerServices { get; }

    public DashboardController(IHttpContextAccessor httpContextAccessor,IGroupServices _gService,IPlayerService _pService) 
    {
        GroupServices = _gService;
        PlayerServices = _pService;
        _httpCtxtAcc = httpContextAccessor;
        PlayerCtxt = _session.GetObjectFromJson<PlayerContext>("playerContext");
    }

    public IActionResult Index()
    {
        if (PlayerCtxt != null)
        {
            try 
            {
                if (PlayerCtxt != null)
                {
                    List<GroupInvite> GroupInvites = GroupServices.GetPlayerInvites(PlayerCtxt.Profile.Id);
                    List<GroupInvitesOfPlayer> Invites = new List<GroupInvitesOfPlayer>();
                    foreach (GroupInvite invite in GroupInvites) 
                    {
                        PlayerGroups group = GroupServices.Get(invite.GroupID);
                        Invites.Add(new GroupInvitesOfPlayer {
                            GroupId = invite.GroupID,
                            GroupImage = group.Image,
                            GroupName = group.Name,
                            InvitedDate = invite.CreatedDate,
                            InviteId = invite.Id,
                            InviterName = PlayerServices.Get(invite.InvitedBy).Name
                        });
                    }
                    // get player groups
                    return View(new DashboardViewModel
                    {
                        Memberships = GroupServices.GetPlayerGroups(PlayerCtxt.Profile.Id),
                        PlayerGroupsAsAdmin = GroupServices.GetPlayerGroupsTheyAreAnAdmin(PlayerCtxt.Profile.Id),
                        Invites = Invites
                    }); ;
                }
                else 
                {
                    TempData["Message"] = "Please login again!";
                    return RedirectToAction("Index","Home");
                }

            } catch (Exception ex) 
            {
                TempData["Message"] = ex.Message;
                return View();
            }
        }
        else
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

