using System;
using System.Collections.Generic;
using System.Linq;
using gamecrew.Helpers;
using gamecrew.Models;
using gamecrew.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gamecrew.Controllers;

public class GroupsController : Controller
{
    private readonly IHttpContextAccessor _httpCtxtAcc;
    private ISession _session => _httpCtxtAcc.HttpContext.Session;
    private PlayerContext PlayerCtxt { get; set; }
    IGroupServices GroupServices { get; }
    IPlayerService PlayerServices { get; }

    public GroupsController(IHttpContextAccessor httpContextAccessor, IGroupServices _gService, IPlayerService _pService)
    {
        GroupServices = _gService;
        PlayerServices = _pService;
        _httpCtxtAcc = httpContextAccessor;
        PlayerCtxt = _session.GetObjectFromJson<PlayerContext>("playerContext");
    }

    [HttpGet]
    public IActionResult Index(string groupId) 
    {
        if (PlayerCtxt != null)
        {
            GroupsAdminViewModel returningModel = GroupServices.GetGroupsAdminView(groupId, PlayerCtxt.Profile.Id);
            List<GroupMembersListViewModel> groupMemberDetails = new List<GroupMembersListViewModel>();
            List<GroupMember> currentMembers = GroupServices.GetMembers(groupId);
            foreach (GroupMember player in currentMembers) 
            {
                Player playerDetails = PlayerServices.Get(player.PlayerID);
                groupMemberDetails.Add(new GroupMembersListViewModel
                {
                    JoiningDate = player.JoinedDate,
                    PlayerId = player.PlayerID,
                    PlayerImage = playerDetails.Image,
                    PlayerName = playerDetails.Name
                });
            }
            returningModel.Members = groupMemberDetails;
            return View(returningModel);
        }
        else
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult GroupDetails(string groupId)
    {
        if (PlayerCtxt != null)
        {
            GroupsAdminViewModel returningModel = GroupServices.GetGroupsAdminView(groupId, PlayerCtxt.Profile.Id);
            List<GroupMembersListViewModel> groupMemberDetails = new List<GroupMembersListViewModel>();
            List<GroupMember> currentMembers = GroupServices.GetMembers(groupId);
            foreach (GroupMember player in currentMembers)
            {
                Player playerDetails = PlayerServices.Get(player.PlayerID);
                groupMemberDetails.Add(new GroupMembersListViewModel
                {
                    JoiningDate = player.JoinedDate,
                    PlayerId = player.PlayerID,
                    PlayerImage = playerDetails.Image,
                    PlayerName = playerDetails.Name,
                    CanBeAdmin = (GroupServices.GetAllGroupAdmins(groupId).FirstOrDefault() == null) ? true : false
                });
            }
            returningModel.Members = groupMemberDetails;
            return View(returningModel);
        }
        else
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult GroupView(string groupId) 
    {
        if (PlayerCtxt != null)
        {
            GroupsAdminViewModel returningModel = GroupServices.GetGroupDetails(groupId);
            List<GroupMembersListViewModel> groupMemberDetails = new List<GroupMembersListViewModel>();
            List<GroupMember> currentMembers = GroupServices.GetMembers(groupId);
            foreach (GroupMember player in currentMembers)
            {
                Player playerDetails = PlayerServices.Get(player.PlayerID);
                groupMemberDetails.Add(new GroupMembersListViewModel
                {
                    JoiningDate = player.JoinedDate,
                    PlayerId = player.PlayerID,
                    PlayerImage = playerDetails.Image,
                    PlayerName = playerDetails.Name
                });
            }
            returningModel.Members = groupMemberDetails;
            if (returningModel.GroupDetails.AdminID == PlayerCtxt.Profile.Id)
            {
                returningModel.CanLeave = null;
            }
            else 
            {
                if (returningModel.Members.FirstOrDefault(m => m.PlayerId == PlayerCtxt.Profile.Id) != null)
                {
                    returningModel.CanLeave = true;
                }
                else
                {
                    returningModel.CanLeave = false;
                }
            }
            return View(returningModel);
        }
        else
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult ApproveRequest(string requestId,string grouId) 
    {
        if (PlayerCtxt != null)
        {
            TempData["RequestMessage"] = GroupServices.ApproveRequest(PlayerCtxt.Profile.Id,grouId,requestId);
            return RedirectToAction("GroupDetails", "Groups", new { groupId = grouId });
        }
        else
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult Create() 
    {
        if (PlayerCtxt != null)
        {
            return View();
        }
        else 
        {
            TempData["Message"] = "Please login!";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public IActionResult Create(PlayerGroups groups) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                if (ModelState.IsValid)
                {
                    // check if group name exist
                    if (!GroupServices.CheckIfGroupNameExist(groups.Name))
                    {
                        PlayerGroups NewGroup = GroupServices.Create(groups, PlayerCtxt.Profile);
                        GroupServices.AddNewAdmin(PlayerCtxt.Profile.Id, NewGroup.Id, PlayerCtxt.Profile.Id);
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        ModelState.AddModelError("Name", "Group name already used!");
                        return View(groups);
                    }
                }
                else
                {
                    return View();
                }
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Creating Group! Try again later.\n" + ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult Find() 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                if (ModelState.IsValid)
                {
                    return View(GroupServices.GetPlayerGroupsToSearch(PlayerCtxt.Profile.Id));
                }
                else
                {
                    return View();
                }
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Loading Groups! Try again later.\n" + ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult RequestJoinGroup(string groupId) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                TempData["Message"] = GroupServices.RequestToJoinAGroup(groupId,PlayerCtxt.Profile);
                return RedirectToAction("Find", "Groups");
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error join a Group! Try again later.\n" + ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult Leave(string groupID) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                PlayerGroups GroupToLeave = GroupServices.Get(groupID);
                if (GroupToLeave != null)
                {
                    GroupServices.Leave(PlayerCtxt.Profile.Id, groupID);
                    TempData["GroupAsAMember"] = "You left " + GroupToLeave.Name + "!";
                }
                else 
                {
                    TempData["GroupAsAMember"] = "Error leaving " + GroupToLeave.Name + "! Try again later."; 
                }
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Leaving Group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index","Dashboard");
        }
    }

    [HttpGet]
    public IActionResult InvitePlayersList(string groupId)
    {
        try
        {
            if (PlayerCtxt != null)
            {
                List<Player> Players = PlayerServices.Get();
                List<GroupMember> Members = GroupServices.GetMembers(groupId);
                List<GroupInvite> Invites = GroupServices.GetInvites(groupId);
                Players = Players.Where(players =>
                Members.All(m => m.PlayerID != players.Id) && 
                Invites.All(i => i.PlayerID != players.Id) &&
                players.Id != PlayerCtxt.Profile.Id).ToList();
                ViewBag.GroupId = groupId;
                return View(Players);
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Inviting a member to a Group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet]
    public IActionResult InvitePlayerToGroup(string groupId, string playerID) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                PlayerGroups groupToInviteInto = GroupServices.Get(groupId);
                Player playerToInvite = PlayerServices.Get(playerID);
                GroupInvite groupInvites = GroupServices.GetInvites(groupId).FirstOrDefault(gi => gi.PlayerID == playerID);
                if (playerToInvite != null &&
                    groupToInviteInto != null &&
                    groupInvites == null &&
                    groupToInviteInto.AdminID != playerID)
                {
                    GroupServices.InvitePlayerToGroup(groupId, playerID, PlayerCtxt.Profile.Id);
                    TempData["Message"] = "You invited " + playerToInvite.Name + " into your group " + groupToInviteInto.Name + "!";
                    return RedirectToAction("InvitePlayersList", "Groups",new { groupId = groupId });
                }
                else 
                {
                    TempData["Message"] = "Error Trying to invite! Try again later!";
                    return RedirectToAction("InvitePlayersList", "Groups", new { groupId = groupId });
                }
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Inviting a member to a Group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }

    }

    [HttpGet]
    public IActionResult AcceptInvite(string inviteId) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                GroupInvite invite = GroupServices.GetInvite(inviteId);
                PlayerGroups invitedGroup = GroupServices.Get(invite.GroupID);
                Player inviter = PlayerServices.Get(invite.InvitedBy);
                Player invitedPlayer = PlayerServices.Get(invite.PlayerID);
                if (invitedGroup != null && invite != null)
                {
                    GroupServices.AcceptInvite(invite,invitedPlayer);
                    TempData["Message"] = "You accepted the invitation of " + inviter.Name +
                        " to group " + invitedGroup.Name;
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["Message"] = "Error accepting invite! Try again later!";
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Inviting a member to a Group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet]
    public IActionResult DeclineInvite(string inviteId)
    {
        try
        {
            if (PlayerCtxt != null)
            {
                GroupInvite invite = GroupServices.GetInvite(inviteId);
                PlayerGroups invitedGroup = GroupServices.Get(invite.GroupID);
                Player inviter = PlayerServices.Get(invite.InvitedBy);
                Player invitedPlayer = PlayerServices.Get(invite.PlayerID);
                if (invitedGroup != null && invite != null)
                {
                    GroupServices.DeclineInvite(invite.GroupID, invite.PlayerID);
                    TempData["Message"] = "You Declined the invitation of " + inviter.Name +
                        " to group " + invitedGroup.Name;
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["Message"] = "Error accepting invite! Try again later!";
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Inviting a member to a Group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet]
    public IActionResult SetPlayerAsAdmin(string setBy, string playerId, string groupId) 
    {
        try
        {
            if (PlayerCtxt != null)
            {
                GroupServices.AddNewAdmin(playerId, groupId, PlayerCtxt.Profile.Id);
                Player newAdmin = PlayerServices.Get(playerId);
                TempData["Message"] = "You set " + newAdmin.Name + " as admin!";
                return RedirectToAction("GroupDetails", "Groups", new { groupId = groupId });
            }
            else
            {
                TempData["Message"] = "Please login!";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error Setting ad new admin to your group! Try again later.\n" + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    public IActionResult Index()
    {
        return View();
    }
}


