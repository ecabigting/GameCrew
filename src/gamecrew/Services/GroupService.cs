using System;
using System.Collections.Generic;
using System.Linq;
using gamecrew.Helpers;
using gamecrew.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace gamecrew.Services
{
    public class GroupService : IGroupServices
    {
        private readonly IMongoCollection<PlayerGroups> _groups;
        private readonly IMongoCollection<GroupMember> _members;
        private readonly IMongoCollection<PlayerJoinRequest> _request;
        private readonly IMongoCollection<GroupInvite> _invites;
        private readonly IMongoCollection<PlayerGroupAdmins> _admins;

        public GroupService(AppSettings _appSett)
        {
            var mDBClient = new MongoClient(_appSett.ConnectionString);
            var database = mDBClient.GetDatabase(_appSett.DatabaseName);
            _groups = database.GetCollection<PlayerGroups>("Groups");
            if (_groups == null)
            {
                database.CreateCollection("Groups", null);
                _groups = database.GetCollection<PlayerGroups>("Groups");
            }
            _members = database.GetCollection<GroupMember>("GroupMembers");
            if (_members == null) 
            {
                database.CreateCollection("GroupMembers", null);
                _members = database.GetCollection<GroupMember>("GroupMembers");
            }
            _request = database.GetCollection<PlayerJoinRequest>("PlayerJoinRequest");
            if (_members == null)
            {
                database.CreateCollection("PlayerJoinRequest", null);
                _request = database.GetCollection<PlayerJoinRequest>("PlayerJoinRequest");
            }
            _invites = database.GetCollection<GroupInvite>("GroupInvites");
            if (_invites == null)
            {
                database.CreateCollection("GroupInvites", null);
                _invites = database.GetCollection<GroupInvite>("GroupInvites");
            }
            _admins = database.GetCollection<PlayerGroupAdmins>("GroupAdmins");
            if (_admins == null)
            {
                database.CreateCollection("GroupInvites", null);
                _admins = database.GetCollection<PlayerGroupAdmins>("GroupAdmins");
            }

        }

        public List<PlayerGroups> Get() =>
            _groups.Find(group => true).ToList();

        public List<PlayerGroupViewModel> GetPlayerGroupsToSearch(string playerId) 
        {
            List<PlayerGroupViewModel> PGViewModel = new List<PlayerGroupViewModel>();
            List<PlayerGroups> PGList = _groups.Find<PlayerGroups>(groups => true).ToList();
            foreach (PlayerGroups pg in PGList) 
            {
                PGViewModel.Add(new PlayerGroupViewModel
                {
                    AdminID = pg.AdminID,
                    Description = pg.Description,
                    GroupId = pg.Id,
                    Image = pg.Image,
                    IsPendingRequest = (_request.Find<PlayerJoinRequest>(pjr => pjr.PlayerId == playerId && pjr.GroupId == pg.Id).FirstOrDefault() == null) ? false : true,
                    Name = pg.Name,
                    IsMember = (_members.Find<GroupMember>(gm => gm.GroupId == pg.Id && gm.PlayerID == playerId).FirstOrDefault() == null) ? false : true,
                    AdminName = pg.Admin
                }); ;
            }
            return PGViewModel;
        }

        public PlayerGroups Get(string id) =>
           _groups.Find<PlayerGroups>(group => group.Id == id).FirstOrDefault();

        public void Leave(string playerID, string groupId)
        => _members.DeleteOne(m => m.PlayerID == playerID && m.GroupId == groupId);

        public bool CheckIfGroupNameExist(string groupName)
        => (_groups.Find<PlayerGroups>(g => g.Name == groupName).FirstOrDefault() != null) ? true : false;

        public List<PlayerGroups> GetByName(string groupName)
        {
            var filter = Builders<PlayerGroups>.Filter.Regex("Name", new MongoDB.Bson.BsonRegularExpression(groupName, "i"));
            return _groups.Find(filter).ToList();
        }

        public void AddMember(string groupID, Player newMember, string inviterID)
        {
            GroupMember member = new GroupMember
            {
                CreatedBy = newMember.Id,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                InviterID = inviterID,
                IsEnabled = true,
                IsEnabledBy = inviterID,
                JoinedDate = DateTime.Now,
                LastEditedBy = inviterID,
                LastEditedDate = DateTime.Now,
                GroupId = groupID,
                PlayerID = newMember.Id,
                AcceptedInvitation = true
            };
            _members.InsertOne(member);
        }

        public void RemoveMember(string groupID, string memberId)
        {
            _members.DeleteOne(gm => gm.GroupId == groupID && gm.PlayerID == memberId);
        }

        public PlayerGroups Create(PlayerGroups groupIn, Player admin)
        {
            //get admin info
            groupIn.DateEnabled = DateTime.Now;
            groupIn.CreatedDate = DateTime.Now;
            groupIn.LastEditedDate = DateTime.Now;
            groupIn.IsEnabled = true;
            groupIn.IsEnabledBy = admin.Id;
            groupIn.CreatedBy = admin.Id;
            groupIn.LastEditedBy = admin.Id;
            groupIn.Admin = admin.Name;
            groupIn.AdminID = admin.Id;
            _groups.InsertOne(groupIn);
            PlayerGroups NewGroup = _groups.Find<PlayerGroups>(g => g.Name == groupIn.Name).FirstOrDefault();
            return NewGroup;
        }

        public void AddNewAdmin(string playerID, string GroupId, string Setby) 
        {
            PlayerGroupAdmins newAdmin = new PlayerGroupAdmins { 
                CreatedBy = Setby,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                GroupId = GroupId,
                IsEnabled = true,
                IsEnabledBy = Setby,
                LastEditedBy = Setby,
                LastEditedDate = DateTime.Now,
                PlayerId = playerID
            };
            _admins.InsertOne(newAdmin);
        }

        public GroupMember Join(string groupId, string playerId)
        {
            GroupMember newMember = new GroupMember
            {
                CreatedBy = playerId,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                InviterID = playerId,
                IsEnabled = false,
                IsEnabledBy = null,
                JoinedDate = DateTime.Now,
                LastEditedBy = playerId,
                LastEditedDate = DateTime.Now,
                GroupId = groupId,
                PlayerID = playerId,
                AcceptedInvitation = false
            };
            _members.InsertOne(newMember);
            return newMember;
        }

        public void Update(string id, PlayerGroups groupIn) =>
            _groups.ReplaceOne(group => group.Id == id, groupIn);

        public void Remove(Player groupIn) =>
            _groups.DeleteOne(group => group.Id == groupIn.Id);

        public void Remove(string id) =>
            _groups.DeleteOne(group => group.Id == id);

        public List<PlayerMemberships> GetPlayerGroups(string playerID) 
        {
            List<PlayerMemberships> pMemberships = new List<PlayerMemberships>();
            List<GroupMember> playerGroups = _members.Find<GroupMember>(gm =>gm.PlayerID == playerID).ToList();
            foreach (GroupMember membership in playerGroups) 
            {
                PlayerGroups currentGroup = _groups.Find<PlayerGroups>(pg => pg.Id == membership.GroupId).FirstOrDefault();
                pMemberships.Add(
                        new PlayerMemberships { 
                            CurrentGroupMemberCount = _members.Find<GroupMember>(g => g.GroupId == membership.GroupId).ToList().Count(),
                            GroupImage = currentGroup.Image,
                            GroupName = currentGroup.Name,
                            GroupID = membership.GroupId
                        }
                    );
            }
            return pMemberships;
        }

        public List<PlayerGroups> GetPlayerGroupsTheyAreAnAdmin(string playerID)
        {
            List<PlayerGroups> pGroups = _groups.Find<PlayerGroups>(g => true).ToList();
            List<PlayerGroupAdmins> admins = _admins.Find<PlayerGroupAdmins>(pga => pga.PlayerId == playerID).ToList();
            pGroups = pGroups.Where(
                groups => admins.Any(a => a.GroupId == groups.Id)
                || groups.AdminID == playerID).ToList();
            return pGroups;
        }

        public string RequestToJoinAGroup(string groupId,Player player) 
        {
            PlayerGroups groupToJoin = _groups.Find<PlayerGroups>(g => g.Id == groupId).FirstOrDefault();
            if (groupToJoin != null)
            {
                PlayerJoinRequest requestToJoin = new PlayerJoinRequest { 
                    CreatedBy = player.Id,
                    CreatedDate = DateTime.Now,
                    DateEnabled = DateTime.Now,
                    GroupId = groupId,
                    IsEnabled = false,
                    IsEnabledBy = null,
                    LastEditedBy = player.Id,
                    LastEditedDate = DateTime.Now,
                    PlayerId = player.Id,
                    PlayerDetails = player
                };
                _request.InsertOne(requestToJoin);
                return "You Requested to join " + groupToJoin.Name + ". Please allow some time for admins to accept your request!";
            }
            else 
            {
                return "Invalid Group! Please try again!";
            }
        }

        public GroupsAdminViewModel GetGroupsAdminView(string groupId,string adminId) 
        {
            List<PlayerGroupAdmins> admins = _admins.Find<PlayerGroupAdmins>(a => a.GroupId == groupId && a.PlayerId == adminId).ToList();
            return new GroupsAdminViewModel
            {
                GroupDetails = _groups.Find<PlayerGroups>(g => g.Id == groupId && 
                (g.AdminID == adminId || admins != null)).FirstOrDefault(),
                Request = _request.Find<PlayerJoinRequest>(pjr => pjr.GroupId == groupId).ToList()
            };
        }

        public GroupsAdminViewModel GetGroupDetails(string groupId)
        {
            return new GroupsAdminViewModel
            {
                GroupDetails = _groups.Find<PlayerGroups>(g => g.Id == groupId).FirstOrDefault(),
                Request = _request.Find<PlayerJoinRequest>(pjr => pjr.GroupId == groupId).ToList()
            };
        }

        public string ApproveRequest(string adminId, string groupId, string requestId) 
        {
            // check if group exist
            PlayerGroups group = _groups.Find<PlayerGroups>(pg => pg.Id == groupId && pg.AdminID == adminId).FirstOrDefault();
            if (group != null)
            {
                // get request
                PlayerJoinRequest PlayerRequest = _request.Find<PlayerJoinRequest>(pjr => pjr.Id == requestId && pjr.GroupId == group.Id).FirstOrDefault();
                GroupMember member = _members.Find<GroupMember>(m => m.PlayerID == PlayerRequest.PlayerId && m.GroupId == PlayerRequest.GroupId).FirstOrDefault();
                if (PlayerRequest != null && member == null)
                {
                    _request.DeleteOne(r => r.Id == PlayerRequest.Id);
                    GroupMember newMembership = new GroupMember { 
                        CreatedBy = adminId,
                        AcceptedInvitation = true,
                        CreatedDate = DateTime.Now,
                        DateEnabled = DateTime.Now,
                        GroupId = group.Id,
                        InviterID = adminId,
                        IsEnabled = true,
                        IsEnabledBy = adminId,
                        JoinedDate = DateTime.Now,
                        LastEditedBy = adminId,
                        LastEditedDate = DateTime.Now,
                        PlayerID = PlayerRequest.PlayerId
                    };
                    _members.InsertOne(newMembership);
                    return "You accepted " + PlayerRequest.PlayerDetails.Name + " to join your group!";
                }
                else 
                {
                    return "Invalid Group Approval request!";
                }
            }
            else 
            {
                return "Invalid group request!";
            }
        }

        public List<GroupMember> GetMembers(string groupId) => _members.Find<GroupMember>(gm => gm.GroupId == groupId).ToList();

        public void InvitePlayerToGroup(string groupID, string playerId, string inviterID) 
        {
            _invites.InsertOne(new GroupInvite { 
                CreatedBy = inviterID,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                GroupID = groupID,
                InvitedBy = inviterID,
                IsEnabled = true,
                IsEnabledBy = inviterID,
                LastEditedBy = inviterID,
                LastEditedDate = DateTime.Now,
                PlayerID = playerId
            });
        }

        public List<GroupInvite> GetInvites(string groupId)
            => _invites.Find<GroupInvite>(gi => gi.GroupID == groupId).ToList();

        public List<GroupInvite> GetPlayerInvites(string playerID)
            => _invites.Find<GroupInvite>(gi => gi.PlayerID == playerID).ToList();

        public GroupInvite GetInvite(string inviteId)
            => _invites.Find<GroupInvite>(gi => gi.Id == inviteId).FirstOrDefault();

        public void AcceptInvite(GroupInvite invite,Player invitedPlayer) 
        {
            AddMember(invite.GroupID,invitedPlayer,invite.InvitedBy);
            _invites.DeleteMany(i => i.GroupID == invite.GroupID && i.PlayerID == invitedPlayer.Id);
        }

        public void DeclineInvite(string groupId, string playerId)
        {
            _invites.DeleteMany(i => i.GroupID == groupId && i.PlayerID == playerId);
        }

        public List<PlayerGroupAdmins> GetAllGroupAdmins(string groupId)
            => _admins.Find<PlayerGroupAdmins>(pga => pga.GroupId == groupId).ToList();

    }


    public interface IGroupServices
    {
        List<PlayerGroups> Get();
        PlayerGroups Get(string id);
        List<PlayerGroups> GetByName(string groupName);
        void AddMember(string groupID, Player newMember, string inviterID);
        void RemoveMember(string groupID, string memberId);
        PlayerGroups Create(PlayerGroups group, Player admin);
        void Update(string id, PlayerGroups groupIn);
        void Remove(Player groupIn);
        void Remove(string id);
        bool CheckIfGroupNameExist(string groupName);
        GroupMember Join(string groupId, string playerId);
        List<PlayerMemberships> GetPlayerGroups(string playerID);
        List<PlayerGroups> GetPlayerGroupsTheyAreAnAdmin(string playerID);
        List<PlayerGroupViewModel> GetPlayerGroupsToSearch(string playerId);
        string RequestToJoinAGroup(string groupId, Player player);
        GroupsAdminViewModel GetGroupsAdminView(string groupId, string adminId);
        string ApproveRequest(string adminId, string groupId, string requestId);
        List<GroupMember> GetMembers(string groupId);
        GroupsAdminViewModel GetGroupDetails(string groupId);
        void Leave(string playerID, string groupId);
        void InvitePlayerToGroup(string groupID, string playerId, string inviterID);
        List<GroupInvite> GetInvites(string groupId);
        List<GroupInvite> GetPlayerInvites(string playerID);
        GroupInvite GetInvite(string inviteId);
        void AcceptInvite(GroupInvite invite, Player invitedPlayer);
        void DeclineInvite(string groupId, string playerId);
        void AddNewAdmin(string playerID, string GroupId, string Setby);
        List<PlayerGroupAdmins> GetAllGroupAdmins(string groupId);
    }
}