using ATodoList.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Services;

public sealed class MongoDBHelper : IDBHelper
{
    /// <summary>
    /// 尝试转换输入helper
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <param name="mongoDBHelper"></param>
    /// <returns>若抛出异常，则构造失败</returns>
    public static bool TrySwitchMongoDBServise(string connectionString, string databaseName, ref MongoDBHelper mongoDBHelper)
    {
        if (databaseName == string.Empty)
            return false;

        if (mongoDBHelper.ConnectionString != connectionString) {
            try {
                mongoDBHelper = new MongoDBHelper(connectionString, databaseName);
            } catch (Exception e) {
                Debug.WriteLine(e);
                return false;
            }
        }

        if (mongoDBHelper.DatabaseName != databaseName) {
            try {
                mongoDBHelper.SwitchDatabase(databaseName);
            } catch (Exception e) {
                Debug.WriteLine(e);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 尝试转换输入helper
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <param name="db"></param>
    /// <returns>若抛出异常，则构造失败</returns>
    public static bool TrySwitchMongoDBServise(string connectionString, string databaseName, ref IDBHelper db)
    {
        if (db is MongoDBHelper mongoDBHelper) {
            var result = TrySwitchMongoDBServise(connectionString, databaseName, ref mongoDBHelper);
            db = mongoDBHelper;
            return result;
        } else {
            return TryGetMongoDBHelper(connectionString, databaseName, out db);
        }
    }

    /// <summary>
    /// 尝试构造一个mongodb服务对象
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <param name="db"></param>
    /// <returns>若抛出异常，则构造失败</returns>
    public static bool TryGetMongoDBHelper(string connectionString, string databaseName, out IDBHelper db)
    {
        try {
            db = new MongoDBHelper(connectionString, databaseName);
        } catch (Exception e) {
            Debug.WriteLine(e);
            db = InvalibleService.Disable;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 切换database
    /// </summary>
    /// <param name="databaseName"></param>
    private void SwitchDatabase(string databaseName)
    {
        DatabaseName = databaseName;
        _database = _client.GetDatabase(databaseName);
    }

    /// <summary>
    /// mongodb host
    /// </summary>
    private readonly string _connectionString;
    
    /// <summary>
    /// mongodb database name
    /// </summary>
    private string _databaseName;

    /// <summary>
    /// mongodb client
    /// </summary>
    private readonly IMongoClient _client;

    /// <summary>
    /// mongodb current database
    /// </summary>
    private IMongoDatabase _database;

    /// <summary>
    /// 从指定的host和db name中构造mongodb服务对象
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    public MongoDBHelper(string connectionString, string databaseName)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;

        MongoUrl url = new MongoUrl("mongodb://" + connectionString);
        MongoClientSettings settings = MongoClientSettings.FromUrl(url);
        settings.ServerSelectionTimeout = new TimeSpan(0, 0, 3);
        _client = new MongoClient(settings);
        _client.ListDatabaseNames();
        _database = _client.GetDatabase(databaseName);
    }

    /// <summary>
    /// mongodb client
    /// </summary>
    private IMongoClient Client { get => _client; }

    /// <summary>
    /// mongodb current database
    /// </summary>
    private IMongoDatabase Database { get => _database; }

    /// <summary>
    /// mongodb database name
    /// </summary>
    public string DatabaseName { get => _databaseName; private set => _databaseName = value; }

    /// <summary>
    /// mongodb host
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// 获取所有组别
    /// </summary>
    /// <returns></returns>
    public TodoGroupItem[] GetGroupsItems()
    {
        var collection = Database.GetCollection<BsonDocument>("_user_todo_group");
        var result = collection.Find(Builders<BsonDocument>.Filter.Empty)
                               .ToEnumerable()
                               .Select(item => new TodoGroupItem(item["name"].AsString))
                               .ToArray();
        return result;
    }

    /// <summary>
    /// 获取输入组的真正集合名
    /// </summary>
    /// <param name="groupName">指定的用户输入组名</param>
    /// <param name="collectionName">用户输入组名对应的集合</param>
    /// <returns>结果</returns>
    public bool TryGetGroupBelongCollectionName(string groupName, out string collectionName)
    {
        collectionName = string.Empty;

        var userTodoGroupCollection = Database.GetCollection<BsonDocument>("_user_todo_group");
        var mayBeHaveName = userTodoGroupCollection.Find(Builders<BsonDocument>.Filter.Eq("name", groupName)).FirstOrDefault();
        
        if (mayBeHaveName is not null && mayBeHaveName.TryGetValue("collectionName", out var collectionNameValue)) {
            try {
                collectionName = collectionNameValue.AsString;
            } catch (Exception e) {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 从指定的用户输入组名中获得所有TodoItem
    /// </summary>
    /// <param name="groupName">指定的用户输入组名</param>
    /// <returns></returns>
    public TodoItem[] GetTodoItemFromGroup(string groupName)
    {
        if (!TryGetGroupBelongCollectionName(groupName, out var collectionName)) {
            return [];
        }

        var targetCollection = Database.GetCollection<BsonDocument>(collectionName);
        
        var result = targetCollection.Find(Builders<BsonDocument>.Filter.Empty)
                                     .ToEnumerable()
                                     .Select(TodoItem.Parse)
                                     .ToArray();
        return result;
    }

    /// <summary>
    /// 更新用户输入组名中Todoitem的信息
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="_id"></param>
    /// <param name="title"></param>
    /// <param name="deadLine"></param>
    /// <param name="description"></param>
    /// <param name="isFinish"></param>
    /// <returns></returns>
    public bool UpdateTodoItemInfo(string groupName, ObjectId _id, string title, DateTime? deadLine, string description, bool isFinish)
    {
        if (!TryGetGroupBelongCollectionName(groupName, out var collectionName)) {
            return false;
        }

        var collection = Database.GetCollection<BsonDocument>(collectionName);

        var deadline = deadLine?.ToString();

        var result = collection.UpdateOne(
                Builders<BsonDocument>.Filter.Eq("_id", _id),
                Builders<BsonDocument>.Update.Set("title", title)
                                             .Set("deadline", deadline)
                                             .Set("description", description)
                                             .Set("isFinish", isFinish)
            );

        return result.ModifiedCount is not 0;
    }

    /// <summary>
    /// 移除指定的用户输入组名
    /// </summary>
    /// <param name="groupName">用户输入组名</param>
    /// <returns>移除结果</returns>
    public bool RemoveGroup(string groupName)
    {
        if (groupName is "_user_todo_group" || !TryGetGroupBelongCollectionName(groupName, out var targetCollectionName))
            return false;

        try {
            Database.DropCollection(targetCollectionName);
        } catch (Exception e) {
            Debug.WriteLine(e);
        }

        var userGroup = Database.GetCollection<BsonDocument>("_user_todo_group");
        var result = userGroup.DeleteOne(Builders<BsonDocument>.Filter.Eq("name", groupName));

        return result.DeletedCount is not 0;
    }

    /// <summary>
    /// 添加用户输入组名
    /// </summary>
    /// <param name="groupName">用户输入组名</param>
    /// <returns>添加结果</returns>
    public bool AddGroup(string groupName)
    {
        if (TryGetGroupBelongCollectionName(groupName, out _)) {
            return false;
        }

        var collectionName = string.Join('_',
                                groupName.Trim()
                                      .Split()
                                      .Where(item => !string.IsNullOrWhiteSpace(item))
            );

        var userGroup = Database.GetCollection<BsonDocument>("_user_todo_group");
        userGroup.InsertOne(new BsonDocument { { "name", groupName }, { "collectionName", collectionName } });

        return true;
    }

    /// <summary>
    /// 重命名组名，不影响实际集合名
    /// </summary>
    /// <param name="oldName">旧组名</param>
    /// <param name="newName">新组名</param>
    /// <returns>修改结果</returns>
    public bool RenameGroup(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;   

        var userGroup = Database.GetCollection<BsonDocument>("_user_todo_group");

        var oldNameFindResult = userGroup.Find(Builders<BsonDocument>.Filter.Eq("name", oldName)).FirstOrDefault();

        if (oldNameFindResult is null || !oldNameFindResult.TryGetValue("_id", out var targetItemValue)) {
            return false;
        }

        var newNameCount = userGroup.CountDocuments(Builders<BsonDocument>.Filter.Eq("name", newName));
        if (newNameCount is not 0) {
            return false;
        }

        try {
            ObjectId result = targetItemValue.AsObjectId;
            var res = userGroup.UpdateOne(
                    Builders<BsonDocument>.Filter.Eq("_id", result),
                    Builders<BsonDocument>.Update.Set("name", newName)
            );
            return true;
        } catch (Exception ex) {
            Debug.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    /// 从指定的组中移除指定的文档，根据_id
    /// </summary>
    /// <param name="groupName">指定的组名</param>
    /// <param name="targetObjectId">_id</param>
    /// <returns></returns>
    public bool RemoveTodoItemFromGroup(string groupName, ObjectId targetObjectId)
    {
        if (!TryGetGroupBelongCollectionName(groupName, out var collectionName)) {
            return false;
        }

        var collection = Database.GetCollection<BsonDocument>(collectionName);

        try {
            var result = collection.DeleteOne(
                    Builders<BsonDocument>.Filter.Eq("_id", targetObjectId)
                );

            return result.DeletedCount is 1;
        } catch (Exception e) {
            Debug.WriteLine(e);

            return false;
        }
    }

    /// <summary>
    /// 将一个全新的事项加入指定的组名
    /// </summary>
    /// <param name="targetGroupName"></param>
    /// <param name="title"></param>
    /// <param name="deadLine"></param>
    /// <param name="description"></param>
    /// <param name="isFinish"></param>
    /// <returns></returns>
    public bool AddTodoItemIntoTargetGroup(string targetGroupName,
                                                  string title,
                                                  DateTime? deadLine,
                                                  string description,
                                                  bool isFinish)
    {
        if (!TryGetGroupBelongCollectionName(targetGroupName, out var collectionName)) {
            return false;
        }

        var collection = Database.GetCollection<BsonDocument>(collectionName);

        try {
            collection.InsertOne(TodoItem.ToBsonDocumentWithoutObjectId(title, deadLine, description, isFinish));

            return true;

        } catch (Exception ex) {
            Debug.WriteLine(ex);

            return false;
        }
    }
}
