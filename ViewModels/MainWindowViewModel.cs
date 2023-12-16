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

        public MainWindowViewModel()
        {
            _groupItems = Services.DatabaseService.LoadTodoGroupItems();
            _currentTodoGroupFinishItems = [];
            _currentTodoGroupYieldFinishItems = [];
            _groupListSelectIndex = -1;
            _selectedGroupName = string.Empty;
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

        public void AddGroup(string groupName) => AddGroup(groupName, []);

        public void AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
        {
            Services.DatabaseService.AddGroup(groupName, todoItems);

            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            InputGroupName = string.Empty;
        }

        public void RemoveSelectedGroup(string groupName)
        {
            if (!Services.DatabaseService.RemoveGroup(groupName)) {
                return;
            }

            GroupListSelectIndex = -1;  // -1 means not selected
            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            CurrentTodoGroupFinishItems = [];
            CurrentTodoGroupYieldFinishItems = [];
            SelectedGroupName = string.Empty;
        }

        public void LoadTodoItems(string groupName)
        {
            var current = Services.DatabaseService.LoadTodoItemsFromGroup(groupName);

            CurrentTodoGroupYieldFinishItems = current.Where(item => !item.IsFinish).ToArray();
            CurrentTodoGroupFinishItems = current.Where(item => item.IsFinish).ToArray();
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

        public bool AlterTodoItemInfo(int objectId, string title, DateTime? deadLine, string description, bool isFinish)
        {
            var result = Services.DatabaseService.AlterTodoItemInfo(SelectedGroupName, objectId, title, deadLine, description, isFinish);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        public bool RemoveTodoItem(int objectId)
        {
            var result = Services.DatabaseService.RemoveTodoItem(SelectedGroupName, objectId);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        public bool AddNewTodoItem(string? newItemTitle)
        {
            if (string.IsNullOrWhiteSpace(newItemTitle)) {
                return false;
            }

            var result = Services.DatabaseService.AddNewTodoItem(SelectedGroupName, newItemTitle.Trim());
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }
    }
}
