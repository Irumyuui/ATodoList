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

/// <summary>
/// 提供Database服务
/// </summary>
public static class DatabaseService
{
    /// <summary>
    /// 外部Database服务对象
    /// 当原Database对象不可用时，将转换为InvalibleService对象
    /// </summary>
    private static IDBHelper _dB = InvalibleService.Disable;

    /// <summary>
    /// 外部Database服务对象
    /// 当原Database对象不可用时，将转换为InvalibleService对象
    /// </summary>
    public static IDBHelper DB { get => _dB; private set => _dB = value; }

    /// <summary>
    /// 加载TodoGroupItems
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 从指定的组名中加载指定完成状态的TodoItems
    /// </summary>
    /// <param name="groupName">组名</param>
    /// <param name="isFinish">完成状态</param>
    /// <returns></returns>
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

    /// <summary>
    /// 从指定的组名中加载所有的TodoItems
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 切换指定组中指定事项的完成情况
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="item"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 移除组
    /// 注意将会把其中所有事项清空
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 添加组
    /// </summary>
    /// <param name="groupItem"></param>
    /// <param name="todoItems"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 添加组
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="todoItems"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 更改组名
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 修改指定_id的事项信息
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="objectId"></param>
    /// <param name="title"></param>
    /// <param name="deadLine"></param>
    /// <param name="description"></param>
    /// <param name="isFinish"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 根据_id移除指定组中的事项
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="objectId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 向指定组中添加新的事项，_id将自动生成
    /// </summary>
    /// <param name="currentGroup"></param>
    /// <param name="title"></param>
    /// <param name="deadLine"></param>
    /// <param name="description"></param>
    /// <param name="isFinish"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 设置database
    /// </summary>
    /// <param name="db"></param>
    public static void SetDatabaseService(IDBHelper db) => DB = db;

    /// <summary>
    /// 尝试设置MongoDB服务
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="databaseName"></param>
    /// <returns>设置状态</returns>
    public static bool TrySetMongoDBService(string connectionString, string databaseName)
    {
        return MongoDBHelper.TrySwitchMongoDBServise(connectionString, databaseName, ref _dB);
    }
}
