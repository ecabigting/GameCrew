using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamecrew.Models
{
    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string LastEditedBy { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime LastEditedDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [BsonRepresentation(BsonType.Boolean)]
        public bool IsEnabled { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string IsEnabledBy { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime DateEnabled { get; set; }
    }
}