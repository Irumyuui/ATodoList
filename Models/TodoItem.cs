using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                {"deadline", DeadLine?.ToString() ?? string.Empty},
                {"description", Description},
                {"isFinish", IsFinish},
            };
        }

        public static MongoDB.Bson.BsonDocument ToBsonDocumentWithoutObjectId(
                string title, DateTime? deadLine = null, string description = "", bool isFinish = false
            )
        {
            return new BsonDocument {
                {"title", title},
                {"deadline", deadLine?.ToString() ?? string.Empty},
                {"description", description},
                {"isFinish", isFinish},
            };
        }

        public static TodoItem Parse(BsonDocument bson)
        {
            MongoDB.Bson.ObjectId _id = bson["_id"].AsObjectId;
            string title = bson["title"].AsString;

            DateTime? deadLine = null;
            if (bson.TryGetValue("deadline", out var deadlineValue)) {
                try {
                    if (DateTime.TryParse(deadlineValue.AsString, out var deadLineResult))
                        deadLine = deadLineResult;
                } catch (Exception e) {
                    deadLine = null;
                    Debug.WriteLine(e);
                }
            }

            string description = bson["description"].AsString;
            bool isFinish = bson["isFinish"].AsBoolean;

            return new TodoItem(_id, title, deadLine, description, isFinish);
        }
    }
}
