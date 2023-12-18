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

    private void SwitchDatabase(string databaseName)
    {
        DatabaseName = databaseName;
        _database = _client.GetDatabase(databaseName);
    }

    private readonly string _connectionString;
    
    private string _databaseName;

    private readonly IMongoClient _client;

    private IMongoDatabase _database;

    public MongoDBHelper(string connectionString, string databaseName)
    {
        //if (!connectionString.StartsWith("mongodb://")) {
        //    connectionString = "mongodb://" + connectionString;
        //}

        _connectionString = connectionString;
        _databaseName = databaseName;


        MongoUrl url = new MongoUrl("mongodb://" + connectionString);
        MongoClientSettings settings = MongoClientSettings.FromUrl(url);
        settings.ServerSelectionTimeout = new TimeSpan(0, 0, 3);
        _client = new MongoClient(settings);
        _client.ListDatabaseNames();
        _database = _client.GetDatabase(databaseName);
    }

    private IMongoClient Client { get => _client; }

    private IMongoDatabase Database { get => _database; }
    
    public string DatabaseName { get => _databaseName; private set => _databaseName = value; }

    public string ConnectionString => _connectionString;

    public TodoGroupItem[] GetGroupsItems()
    {
        var collection = Database.GetCollection<BsonDocument>("_user_todo_group");
        var result = collection.Find(Builders<BsonDocument>.Filter.Empty)
                               .ToEnumerable()
                               .Select(item => new TodoGroupItem(item["name"].AsString))
                               .ToArray();
        return result;
    }

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
