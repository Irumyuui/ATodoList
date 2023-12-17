using ATodoList.Models;
using MongoDB.Bson;
using System;

namespace ATodoList.Services;

interface IDBHelper
{
    TodoGroupItem[] GetGroupsItems();

    bool TryGetGroupBelongCollectionName(string groupName, out string collectionName);

    TodoItem[] GetTodoItemFromGroup(string groupName);

    bool UpdateTodoItemInfo(string groupName, ObjectId _id, string title, DateTime? deadLine, string description, bool isFinish);

    bool RemoveGroup(string groupName);

    bool AddGroup(string groupName);

    bool RenameGroup(string oldName, string newName);

    bool RemoveTodoItemFromGroup(string groupName, ObjectId targetObjectId);

    bool AddTodoItemIntoTargetGroup(string targetGroupName, string title, DateTime? deadLine, string description, bool isFinish);
}
