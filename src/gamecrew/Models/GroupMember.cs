using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamecrew.Models
{
    public class GroupMember : BaseModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerID { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string InviterID { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool AcceptedInvitation { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
    }

    public class PlayerMemberships 
    {
        public string GroupName { get; set; }
        public long CurrentGroupMemberCount { get; set; }
        public string GroupImage { get; set; }
        public string GroupID { get; set; }
    }
}