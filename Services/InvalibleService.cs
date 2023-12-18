using ATodoList.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Services
{
    internal sealed class InvalibleService : IDBHelper
    {
        public bool AddGroup(string groupName) => false;

        public bool AddTodoItemIntoTargetGroup(string targetGroupName, string title, DateTime? deadLine, string description, bool isFinish) => false;

        public TodoGroupItem[] GetGroupsItems() => [];

        public TodoItem[] GetTodoItemFromGroup(string groupName) => [];

        public bool RemoveGroup(string groupName) => false;

        public bool RemoveTodoItemFromGroup(string groupName, ObjectId targetObjectId) => false;

        public bool RenameGroup(string oldName, string newName) => false;

        public bool TryGetGroupBelongCollectionName(string groupName, out string collectionName)
        {
            collectionName = string.Empty;
            return false;
        }

        public bool UpdateTodoItemInfo(string groupName, ObjectId _id, string title, DateTime? deadLine, string description, bool isFinish) => false;

        internal static InvalibleService Disable { get; } = new();
    }
}
