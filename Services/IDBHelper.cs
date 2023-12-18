using ATodoList.Models;
using MongoDB.Bson;
using System;

namespace ATodoList.Services;

public interface IDBHelper
{
    /// <summary>
    /// 获取所有组别
    /// </summary>
    /// <returns></returns>
    TodoGroupItem[] GetGroupsItems();

    /// <summary>
    /// 获取输入组的真正集合名
    /// </summary>
    /// <param name="groupName">指定的用户输入组名</param>
    /// <param name="collectionName">用户输入组名对应的集合</param>
    /// <returns>结果</returns>
    bool TryGetGroupBelongCollectionName(string groupName, out string collectionName);

    /// <summary>
    /// 从指定的用户输入组名中获得所有TodoItem
    /// </summary>
    /// <param name="groupName">指定的用户输入组名</param>
    /// <returns></returns>
    TodoItem[] GetTodoItemFromGroup(string groupName);

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
    bool UpdateTodoItemInfo(string groupName, ObjectId _id, string title, DateTime? deadLine, string description, bool isFinish);

    /// <summary>
    /// 移除指定的用户输入组名
    /// </summary>
    /// <param name="groupName">用户输入组名</param>
    /// <returns>移除结果</returns>
    bool RemoveGroup(string groupName);

    /// <summary>
    /// 添加用户输入组名
    /// </summary>
    /// <param name="groupName">用户输入组名</param>
    /// <returns>添加结果</returns>
    bool AddGroup(string groupName);

    /// <summary>
    /// 重命名组名，不影响实际集合名
    /// </summary>
    /// <param name="oldName">旧组名</param>
    /// <param name="newName">新组名</param>
    /// <returns>修改结果</returns>
    bool RenameGroup(string oldName, string newName);

    /// <summary>
    /// 从指定的组中移除指定的文档，根据_id
    /// </summary>
    /// <param name="groupName">指定的组名</param>
    /// <param name="targetObjectId">_id</param>
    /// <returns></returns>
    bool RemoveTodoItemFromGroup(string groupName, ObjectId targetObjectId);

    /// <summary>
    /// 将一个全新的事项加入指定的组名
    /// </summary>
    /// <param name="targetGroupName"></param>
    /// <param name="title"></param>
    /// <param name="deadLine"></param>
    /// <param name="description"></param>
    /// <param name="isFinish"></param>
    /// <returns></returns>
    bool AddTodoItemIntoTargetGroup(string targetGroupName, string title, DateTime? deadLine, string description, bool isFinish);
}
