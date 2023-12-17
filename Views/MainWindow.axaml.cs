using ATodoList.Models;
using ATodoList.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using DynamicData.Aggregation;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using static System.Net.Mime.MediaTypeNames;

namespace ATodoList.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        /// <summary>
        /// 消息提示管理
        /// </summary>
        private WindowNotificationManager? _manager;

        public MainWindow()
        {
            InitializeComponent();
            //_manager = new WindowNotificationManager(this) { MaxItems = 3 };
        }

        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="e"></param>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _manager = new WindowNotificationManager(this) {
                MaxItems = 3
            };
            _manager.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
            _manager.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        }

        /// <summary>
        /// 提示消息
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="type">消息类型</param>
        private void ShowNotification(string? title, string? message, NotificationType type)
        {
            _manager?.Show(new Notification(title, message, type));
        }

        private void ShowNotification(string? message, NotificationType type) => ShowNotification(type.ToString(), message, type);

        private void GroupItemList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectGroupName = ((sender as ListBox)?.SelectedItem as TodoGroupItem)?.Name ?? string.Empty;

            ViewModel!.SelectedGroupName = selectGroupName;
            ViewModel!.ReloadTodoItemsFromCurrentSelectedGroup();
        }

        public async void Async_GroupItemList_ContextMenu_CopyMenuItem_CopyGroupName(object sender, RoutedEventArgs e)
        {
            var selectGroupName = ViewModel!.SelectedGroupName;
            await AsyncSendTextToSystemClipboard(selectGroupName);
            ShowNotification("待办事项组名已复制", NotificationType.Information);
        }

        private async Task AsyncSendTextToSystemClipboard(string text)
        {
            var clipboard = Clipboard;
            if (clipboard is not null) {
                await clipboard.SetTextAsync(text);
            }
        }

        private async void Async_CopySelectedYieldTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("YieldFinishTodoItemListBox");

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                await AsyncSendTextToSystemClipboard(currentTodoItem.Title);
                ShowNotification("待办事项标题已复制", NotificationType.Information);
            }
        }

        private void RemoveSelectedYieldTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("YieldFinishTodoItemListBox");

            bool result = false;

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                result = ViewModel!.RemoveTodoItem(currentTodoItem.ObjectId);
            }

            if (result) {
                ShowNotification("选中待办事项已删除", NotificationType.Success);
            } else {
                ShowNotification("选中待办事项未删除", NotificationType.Error);
            }
        }

        private async void Async_CopySelectedFinishTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("FinishedTodoItemListBox");

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                await AsyncSendTextToSystemClipboard(currentTodoItem.Title);
                ShowNotification("待办事项标题已复制", NotificationType.Information);
            }
        }

        private void RemoveSelectedFinishTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("FinishedTodoItemListBox");

            bool result = false;

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                result = ViewModel!.RemoveTodoItem(currentTodoItem.ObjectId);
            }

            if (result) {
                ShowNotification("选中待办事项已删除", NotificationType.Success);
            } else {
                ShowNotification("选中待办事项未删除", NotificationType.Error);
            }
        }

        private void GroupItemList_ContextMenu_RemoveMeunItem_RemoveSelectedGroup(object sender, RoutedEventArgs e)
        {
            if (ViewModel!.RemoveSelectedGroup(ViewModel!.SelectedGroupName)) {
                ShowNotification("选中待办事项组已删除", NotificationType.Success);
            } else {
                ShowNotification("选中待办事项组未删除", NotificationType.Error);
            }
        }

        private void InputGroupTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox t)
                return;

            if (!string.IsNullOrWhiteSpace(t.Text)) {
                CommitGroupNameButton.IsEnabled = true;
            } else {
                CommitGroupNameButton.IsEnabled = false;
            }
        }

        private void InputGroupNameTextBox_OnEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not Key.Enter) {
                return;
            }

            var result = InputGroupNameTextBox?.Text?.Trim();
            if (result is null)
                return;

            InputGroupName.Text = string.Empty;
            if (ViewModel!.AddGroup(result)) {
                ShowNotification("待办事项组添加成功", NotificationType.Success);
            } else {
                ShowNotification("待办事项组添加失败", NotificationType.Error);
            }
        }

        private void CommitGroupNmaeButton_CommitClick(object sender, RoutedEventArgs e)
        {
            var result = InputGroupNameTextBox?.Text?.Trim();
            if (result is null)
                return;

            InputGroupName.Text = string.Empty;
            if (ViewModel!.AddGroup(result)) {
                ShowNotification("待办事项组添加成功", NotificationType.Success);
            } else {
                ShowNotification("待办事项组添加失败", NotificationType.Error);
            }
        }

        private void TodoItemListBox_Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander is null)
                return;

            var listBoxItem = expander.FindAncestorOfType<ListBoxItem>();
            if (listBoxItem is not null) {
                listBoxItem.IsSelected |= expander.IsExpanded;
            }
        }

        private void GroupItemList_ContextMenu_TextBoxKeyDown_EnterSubmit(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) {
                return;
            }

            Debug.WriteLine($"{e.Key} {e.KeyModifiers}");
            if (e.Key is not Key.Enter) {
                return;
            }
                
            Debug.WriteLine("Submit");

            var newName = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(newName) ) {
                ShowNotification("需要输入内容", NotificationType.Warning);
                return;
            }

            textBox.Text = string.Empty;

            var parent = this.FindControl<ListBox>("GroupItemList")?.ContextMenu;

            if (parent is null)
                return;

            Debug.WriteLine("GroupItemList > ContextMenu");

            if (ViewModel!.RenameSelectedGroupName(newName)) {
                ShowNotification("待办事项组名已更改", NotificationType.Success);
            } else {
                ShowNotification("待办事项组名未更改", NotificationType.Error);
            }
        }
    
        private void TodoGroupItems_SwitchTodoItemFinishStatus(object sender, RoutedEventArgs e)
        {
            // YieldFinishTodoItemListBox
            if (sender is not CheckBox checkBox) {
                return;
            }

            var listBoxItem = checkBox.FindAncestorOfType<ListBoxItem>();
            if (listBoxItem?.DataContext is not TodoItem item) {
                return;
            }
            
            if (ViewModel!.SwitchTodoItemFinishStatue(item)) {
                ShowNotification($"{item.Title}{(!item.IsFinish ? "已完成" : "未完成")}", NotificationType.Information);
            }
        }

        private void TodoItemList_CommitTodoItemInfo(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            var grid = button.FindAncestorOfType<Grid>();
            if (grid is null)
                return;

            var todoItemListBoxItem = grid.FindAncestorOfType<ListBoxItem>();
            if (todoItemListBoxItem is null) return;

            if (grid.Children[0] is not TextBox todoItemTitleTextBox)
                return;

            if (grid.Children[1] is not CalendarDatePicker todoItemDeadLineCalendarDatePicker)
                return;

            if (grid.Children[2] is not TextBox todoItemDescriptionTextBox)
                return;

            if (todoItemListBoxItem.DataContext is not TodoItem prevItem) {
                return;
            }

            var title = todoItemTitleTextBox.Text?.Trim() ?? string.Empty;

            var deadLineText = todoItemDeadLineCalendarDatePicker.SelectedDate;

            var description = todoItemDescriptionTextBox.Text?.Trim() ?? string.Empty;

            if (ViewModel!.AlterTodoItemInfo(
                    prevItem.ObjectId,
                    title,
                    deadLineText,
                    description,
                    prevItem.IsFinish
                )) {
                ShowNotification("任务更改", NotificationType.Success);
            } else {
                ShowNotification("任务未更改", NotificationType.Error);
            }
        }

        private void AddTodoItemList_InputTextBox_PressEnterCommit(object sender, KeyEventArgs e)
        {
            if (e.Key is not Key.Enter)
                return;

            if (sender is not TextBox textBox)
                return;

            var newItemTitle = textBox.Text;
            textBox.Text = string.Empty;

            if (ViewModel!.AddNewTodoItem(newItemTitle)) {
                //textBox.BorderBrush = new SolidColorBrush(Colors.Aquamarine);
                ShowNotification("任务添加成功", NotificationType.Success);
            } else {
                //textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ShowNotification("任务添加失败", NotificationType.Error);
            }
        }

        private void TodoItemList_TreeViewItem_IgnorePressEnter(object sender, KeyEventArgs e) {
            if (e.Key is Key.Enter) {
                e.Handled = true;
            }
        }
    }

    public class TextLineCaseConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool result && parameter is string str) {
                return result ? str : string.Empty;
            }
            return new BindingNotification(new InvalidCastException(),BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TextColorFinishCaseConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool result) {
                return result ? "Gray" : "WhiteSmoke";
            }
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}