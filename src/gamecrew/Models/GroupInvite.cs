using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamecrew.Models
{
    public class GroupInvite : BaseModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string InvitedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerID { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupID { get; set; }
    }
}