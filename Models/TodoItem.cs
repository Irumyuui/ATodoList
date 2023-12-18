using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models;

/// <summary>
/// 待办事项
/// </summary>
/// <param name="objectId">在MongoDB中的_id</param>
/// <param name="title">标题</param>
/// <param name="deadLine">截止时间</param>
/// <param name="description">备注</param>
/// <param name="isFinish">完成状态</param>
public class TodoItem(MongoDB.Bson.ObjectId objectId, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
{
    /// <summary>
    /// 在MongoDB中的_id
    /// </summary>
    public MongoDB.Bson.ObjectId ObjectId { get; } = objectId;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = title;

    /// <summary>
    /// 截止时间
    /// </summary>
    public DateTime? DeadLine { get; set; } = deadLine;

    /// <summary>
    /// 备注
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// 完成状态
    /// </summary>
    public bool IsFinish { get; set; } = isFinish;

    /// <summary>
    /// 转换为BsonDocument，带有_id
    /// </summary>
    /// <returns>BsonDocument对象</returns>
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

    /// <summary>
    /// 从给定的条例中构造符合TodoItem的BsonDocument对象
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="deadLine">截止期限</param>
    /// <param name="description">备注</param>
    /// <param name="isFinish">完成状态</param>
    /// <returns>BsonDocument对象</returns>
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

    /// <summary>
    /// 从给定的BsonDocument对象中提取TodoItem对象
    /// </summary>
    /// <param name="bson"></param>
    /// <returns></returns>
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
