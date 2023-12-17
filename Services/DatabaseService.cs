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
        {
            return DB.GetGroupsItems();
        }

        public static TodoItem[] LoadTodoItemFromGroupName(string groupName, bool isFinish)
        {
            //if (_todoListData.TryGetValue(groupName, out var result)) {
            //    return result.Where(item => item.IsFinish == isFinish)
            //                 .ToArray();
            //}
            //return [];
            return DB.GetTodoItemFromGroup(groupName).Where(item => item.IsFinish == isFinish).ToArray();
        }

        public static TodoItem[] LoadTodoItemsFromGroup(string groupName)
        {
            //if (_todoListData.TryGetValue(groupName, out var result)) {
            //    return [.. result];
            //}
            //return [];

            return DB.GetTodoItemFromGroup(groupName);
        }

        public static bool SwitchTodoItemFinishStatue(string groupName, TodoItem item)
        {
            //// check contains
            //if (!_todoListData.TryGetValue(groupName, out var list))
            //    return false;
            //// check contains
            //if (list.Contains(item) == false)
            //    return false;

            //item.IsFinish = !item.IsFinish;


            //return true;

            return DB.UpdateTodoItemInfo(groupName, item.ObjectId, item.Title, item.DeadLine, item.Description, !item.IsFinish);
        }

        public static bool RemoveGroup(string groupName)
        {
            //return _todoListData.Remove(groupName);
            return DB.RemoveGroup(groupName);
        }

        public static bool AddGroup(TodoGroupItem groupItem, IEnumerable<TodoItem> todoItems)
        {
            return AddGroup(groupItem.Name, todoItems);
        }

        public static bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
        {
            //if (_todoListData.TryGetValue(groupName, out var result)) {
            //    result.AddRange(todoItems);
            //}
            //_todoListData.Add(groupName, todoItems.ToList());

            //return true;

            return DB.AddGroup(groupName);
        }

        public static bool RenameGroup(string oldName, string newName)
        {
            //if (!_todoListData.TryGetValue(oldName, out var tempList) || _todoListData.ContainsKey(newName)) {
            //    return false;
            //}
            //_todoListData.Remove(oldName);
            //_todoListData.Add(newName, tempList);
            //return true;
            return DB.RenameGroup(oldName, newName);
        }

        public static bool AlterTodoItemInfo(string groupName, ObjectId objectId, string title, DateTime? deadLine, string description, bool isFinish)
        {
            //if (!_todoListData.TryGetValue(groupName, out var list)) {
            //    return false;
            //}

            //foreach (var item in list) {
            //    if (item.ObjectId == objectId) {
            //        item.Title = title;
            //        item.DeadLine = deadLine;
            //        item.Description = description;
            //        item.IsFinish = isFinish;
            //        return true;
            //    }
            //}

            //return false;

            return DB.UpdateTodoItemInfo(groupName, objectId, title, deadLine, description, isFinish);
        }

        public static bool RemoveTodoItem(string groupName, ObjectId objectId)
        {
            //if (!_todoListData.TryGetValue(groupName, out var list) || !list.Any(item => item.ObjectId == objectId)) {
            //    return false;
            //}

            //_todoListData[groupName] = list.Where(item => item.ObjectId != objectId).ToList();
            //return true;

            return DB.RemoveTodoItemFromGroup(groupName, objectId);
        }

        public static bool AddNewTodoItem(string currentGroup, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
        {
            //if (!_todoListData.TryGetValue(currentGroup, out var list)) {
            //    return false;
            //}
            //list.Add(new TodoItem(_totItemCount ++, title, deadLine, description, isFinish));
            //return true;

            return DB.AddTodoItemIntoTargetGroup(currentGroup, title, deadLine, description, isFinish);
        }
    }
}
