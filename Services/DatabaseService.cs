using ATodoList.Models;
using DynamicData;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Services;

public static class DatabaseService
{
    private static IDBHelper _dB = InvalibleService.Disable;

    public static IDBHelper DB { get => _dB; private set => _dB = value; }

    public static TodoGroupItem[] LoadTodoGroupItems()
    {
        try {
            return DB.GetGroupsItems();
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return [];
        }
    }

    public static TodoItem[] LoadTodoItemFromGroupName(string groupName, bool isFinish)
    {
        try {
            return DB.GetTodoItemFromGroup(groupName).Where(item => item.IsFinish == isFinish).ToArray();
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return [];
        }
    }

    public static TodoItem[] LoadTodoItemsFromGroup(string groupName)
    {
        try {
            return DB.GetTodoItemFromGroup(groupName);

        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return [];
        }
    }

    public static bool SwitchTodoItemFinishStatue(string groupName, TodoItem item)
    {
        try {
            return DB.UpdateTodoItemInfo(groupName, item.ObjectId, item.Title, item.DeadLine, item.Description, !item.IsFinish);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool RemoveGroup(string groupName)
    {
        try {
            return DB.RemoveGroup(groupName);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool AddGroup(TodoGroupItem groupItem, IEnumerable<TodoItem> todoItems)
    {
        try {
            return AddGroup(groupItem.Name, todoItems);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
    {
        try {
            return DB.AddGroup(groupName);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool RenameGroup(string oldName, string newName)
    {
        try {
            return DB.RenameGroup(oldName, newName);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool AlterTodoItemInfo(string groupName, ObjectId objectId, string title, DateTime? deadLine, string description, bool isFinish)
    {
        try {
            return DB.UpdateTodoItemInfo(groupName, objectId, title, deadLine, description, isFinish);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool RemoveTodoItem(string groupName, ObjectId objectId)
    {
        try {
            return DB.RemoveTodoItemFromGroup(groupName, objectId);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static bool AddNewTodoItem(string currentGroup, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
    {
        try {
            return DB.AddTodoItemIntoTargetGroup(currentGroup, title, deadLine, description, isFinish);
        } catch (Exception e) {
            Debug.WriteLine(e);
            _dB = InvalibleService.Disable;
            return false;
        }
    }

    public static void SetDatabaseService(IDBHelper db) => DB = db;

    public static bool TrySetDatabaseService(string connectionString, string databaseName)
    {
        return MongoDBHelper.TrySwitchMongoDBServise(connectionString, databaseName, ref _dB);
    }
}
