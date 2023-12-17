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

namespace ATodoList.Services
{
    public class MongoDBHelper
    {
        private string _connectionString;
        
        private string _databaseName;

        private IMongoClient _client;

        private IMongoDatabase _database;

        public MongoDBHelper(string connectionString, string databaseName)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
        }

        private IMongoClient Client { get => _client; }

        private IMongoDatabase Database { get => _database; }

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
            
            if (mayBeHaveName is not null && !mayBeHaveName.TryGetValue("collectionName", out var collectionNameValue)) {
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
            //var userTodoGroupCollection = Database.GetCollection<BsonDocument>("_user_todo_group");

            //var mayBeHaveName = userTodoGroupCollection.Find(Builders<BsonDocument>.Filter.Eq("name", groupName))
            //            .FirstOrDefault();

            //if (mayBeHaveName is null || !mayBeHaveName.TryGetValue("collectionName", out var collectionNameValue)) {
            //    return [];
            //}

            //string collectionName;
            //try {
            //    collectionName = collectionNameValue.AsString;
            //} catch (Exception e) {
            //    Debug.WriteLine(e);
            //    return [];
            //}

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
    }
}
