using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamecrew.Models
{
    public class PlayerGroups : BaseModel
    {
        public string Admin { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string AdminID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string Image { get; set; }
    }

    public class PlayerGroupViewModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string AdminID { get; set; }
        public string AdminName { get; set; }
        public bool IsPendingRequest { get; set; }
        public bool IsMember { get; set; }
    }

    public class PlayerJoinRequest : BaseModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerId { get; set; }
        public string GroupId { get; set; }
        public Player PlayerDetails { get; set; }
    }

    public class GroupsAdminViewModel
    {
        public PlayerGroups GroupDetails { get; set; }
        public List<PlayerJoinRequest> Request { get; set; }
        public List<GroupMembersListViewModel> Members { get; set; }
        public bool? CanLeave { get; set; }
    }

    public class GroupMembersListViewModel 
    {
        public string PlayerId { get; set; }
        public string PlayerImage { get; set; }
        public string PlayerName { get; set; }
        public DateTime JoiningDate { get; set; }
        public bool CanBeAdmin { get; set; }
    }

    public class PlayerGroupAdmins : BaseModel 
    {
        public string GroupId { get; set; }
        public string PlayerId { get; set; }
        public string SetBy { get; set; }
    }
}