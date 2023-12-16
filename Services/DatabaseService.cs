using ATodoList.Models;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Services
{
    public static class DatabaseService
    {
        private static Dictionary<string, List<TodoItem>> _todoListData = new() {
            {"全部任务",[
                new TodoItem(1, "123", new DateTime(114, 5, 14)),
                new TodoItem(2, "1233", new DateTime(114, 5, 14), "No", true),
                new TodoItem(3, "1243", new DateTime(114, 5, 14), "NO"),
                new TodoItem(4, "1223"),
                ]},
            {"作业", []},
            {"任务", []}
        };

        public static TodoGroupItem[] LoadTodoGroupItems()
        {
            return _todoListData.Keys.Select(item => new TodoGroupItem(item))
                                     .ToArray();
        }

        public static TodoItem[] LoadTodoItemFromGroupName(string groupName, bool isFinish)
        {
            if (_todoListData.TryGetValue(groupName, out var result)) {
                return result.Where(item => item.IsFinish == isFinish)
                             .ToArray();
            }
            return [];
        }

        public static TodoItem[] LoadTodoItemsFromGroup(string groupName)
        {
            if (_todoListData.TryGetValue(groupName, out var result)) {
                return [.. result];
            }
            return [];
        }

        public static bool SwitchTodoItemFinishStatue(string groupName, TodoItem item)
        {
            // check contains
            if (!_todoListData.TryGetValue(groupName, out var list))
                return false;
            // check contains
            if (list.Contains(item) == false)
                return false;

            item.IsFinish = !item.IsFinish;
            return true;
        }

        public static bool RemoveGroup(string groupName)
        {
            return _todoListData.Remove(groupName);
        }

        public static bool AddGroup(TodoGroupItem groupItem, IEnumerable<TodoItem> todoItems)
        {
            return AddGroup(groupItem.Name, todoItems);
        }

        public static bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
        {
            if (_todoListData.TryGetValue(groupName, out var result)) {
                result.AddRange(todoItems);
            }
            _todoListData.Add(groupName, todoItems.ToList());

            return true;
        }

        public static bool RenameGroup(string groupName, string newName)
        {
            if (!_todoListData.TryGetValue(groupName, out var tempList) || _todoListData.ContainsKey(newName)) {
                return false;
            }
            _todoListData.Remove(groupName);
            _todoListData.Add(newName, tempList);
            return true;
        }

        public static bool AlterTodoItemInfo(string groupName, int objectId, string title, DateTime? deadLine, string description, bool isFinish)
        {
            if (!_todoListData.TryGetValue(groupName, out var list)) {
                return false;
            }

            foreach (var item in list) {
                if (item.ObjectId == objectId) {
                    item.Title = title;
                    item.DeadLine = deadLine;
                    item.Description = description;
                    item.IsFinish = isFinish;
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveTodoItem(string groupName, int objectId)
        {
            if (!_todoListData.TryGetValue(groupName, out var list) || !list.Any(item => item.ObjectId == objectId)) {
                return false;
            }

            _todoListData[groupName] = list.Where(item => item.ObjectId != objectId).ToList();
            return true;
        }
    }
}
