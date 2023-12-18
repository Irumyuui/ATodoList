using ATodoList.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ATodoList.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _inputGroupName = string.Empty;

        private TodoGroupItem[] _groupItems;

        private TodoItem[] _currentTodoGroupFinishItems;

        private TodoItem[] _currentTodoGroupYieldFinishItems;

        private int _groupListSelectIndex;

        private string _selectedGroupName;

        private string _mainTitleName;
        
        public MainWindowViewModel()
        {
            _groupItems = Services.DatabaseService.LoadTodoGroupItems();
            _currentTodoGroupFinishItems = [];
            _currentTodoGroupYieldFinishItems = [];
            _groupListSelectIndex = -1;
            _selectedGroupName = string.Empty;
            _mainTitleName = (Services.DatabaseService.DB as Services.MongoDBHelper)?.DatabaseName ?? "A Todo List";
        }

        public string MainTitleName
        {
            get => _mainTitleName;
            set => this.RaiseAndSetIfChanged(ref _mainTitleName, value);
        }

        public void ResetStatus()
        {
            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            CurrentTodoGroupFinishItems = [];
            CurrentTodoGroupYieldFinishItems = [];
            GroupListSelectIndex = -1;
            SelectedGroupName = string.Empty;
            MainTitleName = (Services.DatabaseService.DB as Services.MongoDBHelper)?.DatabaseName ?? "A Todo List";
        }

        public string InputGroupName
        {
            get => _inputGroupName;
            set => this.RaiseAndSetIfChanged(ref _inputGroupName, value);
        }

        public TodoGroupItem[] GroupItems
        {
            get => _groupItems;
            set => this.RaiseAndSetIfChanged(ref _groupItems, value);
        }

        public TodoItem[] CurrentTodoGroupFinishItems
        {
            get => _currentTodoGroupFinishItems;
            set => this.RaiseAndSetIfChanged(ref _currentTodoGroupFinishItems, value);
        }

        public TodoItem[] CurrentTodoGroupYieldFinishItems
        {
            get => _currentTodoGroupYieldFinishItems;
            set => this.RaiseAndSetIfChanged(ref _currentTodoGroupYieldFinishItems, value);
        }

        public int GroupListSelectIndex
        {
            get => _groupListSelectIndex;
            set => this.RaiseAndSetIfChanged(ref _groupListSelectIndex, value);
        }

        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set => this.RaiseAndSetIfChanged(ref _selectedGroupName, value);
        }

        public bool AddGroup(string groupName) => AddGroup(groupName, []);

        public bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
        {
            if (!Services.DatabaseService.AddGroup(groupName, todoItems)) {
                return false;
            }

            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            InputGroupName = string.Empty;

            return true;
        }

        public bool RemoveSelectedGroup(string groupName)
        {
            if (!Services.DatabaseService.RemoveGroup(groupName)) {
                return false;
            }

            GroupListSelectIndex = -1;  // -1 means not selected
            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            CurrentTodoGroupFinishItems = [];
            CurrentTodoGroupYieldFinishItems = [];
            SelectedGroupName = string.Empty;

            return true;
        }

        public void LoadTodoItems(string groupName)
        {
            var current = Services.DatabaseService.LoadTodoItemsFromGroup(groupName);

            CurrentTodoGroupYieldFinishItems = current.Where(item => !item.IsFinish)
                                                      .OrderBy(item => item.DeadLine is null)
                                                      .ThenBy(item => item.DeadLine)
                                                      .ToArray();
            CurrentTodoGroupFinishItems = current.Where(item => item.IsFinish)
                                                 .OrderBy(item => item.DeadLine is null)
                                                 .ThenBy(item => item.DeadLine)
                                                 .ToArray();
        }
    
        public void ReloadTodoItemsFromCurrentSelectedGroup() {
            //CurrentTodoItems = Services.DatabaseService.LoadTodoItemsFromGroup(SelectedGroupName);
            LoadTodoItems(SelectedGroupName);
        }

        public bool RenameSelectedGroupName(string newName)
        {
            if (!Services.DatabaseService.RenameGroup(SelectedGroupName, newName)) {
                return false;
            }

            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            GroupListSelectIndex = -1;

            return true;
        }

        public bool SwitchTodoItemFinishStatue(TodoItem todoItem)
        {
            var result = Services.DatabaseService.SwitchTodoItemFinishStatue(SelectedGroupName, todoItem);
        
            if (result) {
                ReloadTodoItemsFromCurrentSelectedGroup();
                return true;
            }

            return false;
        }

        public bool AlterTodoItemInfo(MongoDB.Bson.ObjectId objectId, string title, DateTime? deadLine, string description, bool isFinish)
        {
            var result = Services.DatabaseService.AlterTodoItemInfo(SelectedGroupName, objectId, title, deadLine, description, isFinish);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        public bool RemoveTodoItem(MongoDB.Bson.ObjectId objectId)
        {
            var result = Services.DatabaseService.RemoveTodoItem(SelectedGroupName, objectId);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        public bool AddNewTodoItemToCurrentGroup(string? newItemTitle)
        {
            if (string.IsNullOrWhiteSpace(newItemTitle)) {
                return false;
            }

            var result = Services.DatabaseService.AddNewTodoItem(SelectedGroupName, newItemTitle.Trim());
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        public bool AddNewTodoItemToGroup(string groupName, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
        {
            if (string.IsNullOrWhiteSpace(title)) {
                return false;
            }

            var result = Services.DatabaseService.AddNewTodoItem(groupName, title.Trim(), deadLine, description, isFinish);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }
    }
}
