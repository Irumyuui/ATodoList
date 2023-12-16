using ATodoList.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
            var collections = Database.ListCollectionNames()
                                      .ToEnumerable()
                                      .Select(item => new TodoGroupItem(item))
                                      .ToArray();
            return collections;
        }

        public async Task<TodoGroupItem[]> GetGroupItemsAsync()
        {
            List<TodoGroupItem> list = [];

            foreach (var item in (await Database.ListCollectionNamesAsync()).ToEnumerable()) {
                list.Add(new TodoGroupItem(item));
            }

            return [.. list];
        }
    }
}
