using ATodoList.Models;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Services
{
    public static class DatabaseService
    {
        private static MongoDBHelper DB { get; } = new MongoDBHelper("mongodb://127.0.0.1:27017", "ATodoList");

        public static TodoGroupItem[] LoadTodoGroupItems()
            => DB.GetGroupsItems();

        public static TodoItem[] LoadTodoItemFromGroupName(string groupName, bool isFinish)
            => DB.GetTodoItemFromGroup(groupName).Where(item => item.IsFinish == isFinish).ToArray();

        public static TodoItem[] LoadTodoItemsFromGroup(string groupName)
            => DB.GetTodoItemFromGroup(groupName);

        public static bool SwitchTodoItemFinishStatue(string groupName, TodoItem item)
            => DB.UpdateTodoItemInfo(groupName, item.ObjectId, item.Title, item.DeadLine, item.Description, !item.IsFinish);

        public static bool RemoveGroup(string groupName)
            => DB.RemoveGroup(groupName);

        public static bool AddGroup(TodoGroupItem groupItem, IEnumerable<TodoItem> todoItems)
            => AddGroup(groupItem.Name, todoItems);

        public static bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
            => DB.AddGroup(groupName);

        public static bool RenameGroup(string oldName, string newName)
            => DB.RenameGroup(oldName, newName);

        public static bool AlterTodoItemInfo(string groupName, ObjectId objectId, string title, DateTime? deadLine, string description, bool isFinish)
            => DB.UpdateTodoItemInfo(groupName, objectId, title, deadLine, description, isFinish);

        public static bool RemoveTodoItem(string groupName, ObjectId objectId)
            => DB.RemoveTodoItemFromGroup(groupName, objectId);

        public static bool AddNewTodoItem(string currentGroup, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
            => DB.AddTodoItemIntoTargetGroup(currentGroup, title, deadLine, description, isFinish);
    }
}
