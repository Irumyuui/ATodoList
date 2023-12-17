using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models
{
    public class TodoItem(MongoDB.Bson.ObjectId objectId, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
    {
        public MongoDB.Bson.ObjectId ObjectId { get; } = objectId;

        public string Title { get; set; } = title;

        public DateTime? DeadLine { get; set; } = deadLine;

        public string Description { get; set; } = description;

        public bool IsFinish { get; set; } = isFinish;

        public MongoDB.Bson.BsonDocument ToBsonDocument()
        {
            return new BsonDocument {
                {"_id", ObjectId },
                {"title", Title},
                {"deadline", DeadLine?.ToString() },
                {"description", Description},
                {"isFinish", IsFinish},
            };
        }

        public static TodoItem Parse(BsonDocument bson)
        {
            MongoDB.Bson.ObjectId _id = bson["_id"].AsObjectId;
            string title = bson["title"].AsString;

            DateTime? deadLine = null;
            if (bson.TryGetValue("deadline", out var deadlineValue) && DateTime.TryParse(deadlineValue.AsString, out var deadLineResult)) {
                deadLine = deadLineResult;
            }

            string description = bson["description"].AsString;
            bool isFinish = bson["isFinish"].AsBoolean;

            return new TodoItem(_id, title, deadLine, description, isFinish);
        }
    }
}
