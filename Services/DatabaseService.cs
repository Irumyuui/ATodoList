using ATodoList.Models;
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
                new TodoItem(1, "1233", new DateTime(114, 5, 14), "No", true),
                new TodoItem(1, "1243", new DateTime(114, 5, 14), "NO"),
                new TodoItem(1, "1223", new DateTime(114, 5, 14)),
                ]},
            {"作业", []},
            {"任务", []}
        };

        public static TodoGroupItem[] LoadTodoGroupItems()
        {
            return _todoListData.Keys.Select(item => new TodoGroupItem(item))
                                     .ToArray();
        }

        public static TodoItem[] LoadTodoItemsFromGroup(string groupName)
        {
            if (_todoListData.TryGetValue(groupName, out var result)) {
                return [.. result];
            }
            return [];
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
    }
}
