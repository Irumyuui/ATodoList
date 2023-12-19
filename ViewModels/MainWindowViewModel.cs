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

        /// <summary>
        /// 当前所使用的数据库名字
        /// </summary>
        public string MainTitleName
        {
            get => _mainTitleName;
            set => this.RaiseAndSetIfChanged(ref _mainTitleName, value);
        }

        /// <summary>
        /// 重新加载所有项目
        /// </summary>
        public void ResetStatus()
        {
            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            CurrentTodoGroupFinishItems = [];
            CurrentTodoGroupYieldFinishItems = [];
            GroupListSelectIndex = -1;
            SelectedGroupName = string.Empty;
            MainTitleName = (Services.DatabaseService.DB as Services.MongoDBHelper)?.DatabaseName ?? "A Todo List";
        }

        /// <summary>
        /// 添加组名输入框内容
        /// </summary>
        public string InputGroupName
        {
            get => _inputGroupName;
            set => this.RaiseAndSetIfChanged(ref _inputGroupName, value);
        }

        /// <summary>
        /// 待办事项组
        /// </summary>
        public TodoGroupItem[] GroupItems
        {
            get => _groupItems;
            set => this.RaiseAndSetIfChanged(ref _groupItems, value);
        }

        /// <summary>
        /// 当前组中，已完成的事项列表
        /// </summary>
        public TodoItem[] CurrentTodoGroupFinishItems
        {
            get => _currentTodoGroupFinishItems;
            set => this.RaiseAndSetIfChanged(ref _currentTodoGroupFinishItems, value);
        }

        /// <summary>
        /// 当前组中，未完成的事项组
        /// </summary>
        public TodoItem[] CurrentTodoGroupYieldFinishItems
        {
            get => _currentTodoGroupYieldFinishItems;
            set => this.RaiseAndSetIfChanged(ref _currentTodoGroupYieldFinishItems, value);
        }

        /// <summary>
        /// 所选的当前组索引
        /// </summary>
        public int GroupListSelectIndex
        {
            get => _groupListSelectIndex;
            set => this.RaiseAndSetIfChanged(ref _groupListSelectIndex, value);
        }

        /// <summary>
        /// 被选择的组名
        /// </summary>
        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set => this.RaiseAndSetIfChanged(ref _selectedGroupName, value);
        }

        /// <summary>
        /// 添加组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public bool AddGroup(string groupName) => AddGroup(groupName, []);


        /// <summary>
        /// 添加组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="todoItems"></param>
        /// <returns></returns>
        public bool AddGroup(string groupName, IEnumerable<TodoItem> todoItems)
        {
            if (!Services.DatabaseService.AddGroup(groupName, todoItems)) {
                return false;
            }

            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            InputGroupName = string.Empty;

            return true;
        }

        /// <summary>
        /// 移除选择的组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 从指定的组中加载事项
        /// </summary>
        /// <param name="groupName">指定的组名</param>
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
        
        /// <summary>
        /// 从当前所选择的组中重新加载事项
        /// </summary>
        public void ReloadTodoItemsFromCurrentSelectedGroup() {
            LoadTodoItems(SelectedGroupName);
        }

        /// <summary>
        /// 重命名当前选中的组
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameSelectedGroupName(string newName)
        {
            if (!Services.DatabaseService.RenameGroup(SelectedGroupName, newName)) {
                return false;
            }

            GroupItems = Services.DatabaseService.LoadTodoGroupItems();
            GroupListSelectIndex = -1;

            return true;
        }

        /// <summary>
        /// 切换事项完成状态
        /// </summary>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        public bool SwitchTodoItemFinishStatue(TodoItem todoItem)
        {
            var result = Services.DatabaseService.SwitchTodoItemFinishStatue(SelectedGroupName, todoItem);
        
            if (result) {
                ReloadTodoItemsFromCurrentSelectedGroup();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 修改事项信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="title"></param>
        /// <param name="deadLine"></param>
        /// <param name="description"></param>
        /// <param name="isFinish"></param>
        /// <returns></returns>
        public bool AlterTodoItemInfo(MongoDB.Bson.ObjectId objectId, string title, DateTime? deadLine, string description, bool isFinish)
        {
            var result = Services.DatabaseService.AlterTodoItemInfo(SelectedGroupName, objectId, title, deadLine, description, isFinish);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        /// <summary>
        /// 根据_id从目前选择的组中移除指定的事项
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public bool RemoveTodoItem(MongoDB.Bson.ObjectId objectId)
        {
            var result = Services.DatabaseService.RemoveTodoItem(SelectedGroupName, objectId);
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        /// <summary>
        /// 向当前选择的组中添加新的事项
        /// </summary>
        /// <param name="newItemTitle">新事项的标题</param>
        /// <returns></returns>
        public bool AddNewTodoItemToCurrentGroup(string? newItemTitle)
        {
            if (string.IsNullOrWhiteSpace(newItemTitle)) {
                return false;
            }

            var result = Services.DatabaseService.AddNewTodoItem(SelectedGroupName, newItemTitle.Trim());
            ReloadTodoItemsFromCurrentSelectedGroup();
            return result;
        }

        /// <summary>
        /// 添加一个新的待办事项到输入的组中
        /// </summary>
        /// <param name="groupName">输入的组名</param>
        /// <param name="title">待办事项标题</param>
        /// <param name="deadLine">待办事项截止时间</param>
        /// <param name="description">待办事项描述</param>
        /// <param name="isFinish">待办事项截止时间</param>
        /// <returns>添加结果</returns>
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
