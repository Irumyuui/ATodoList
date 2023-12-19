using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Threading.Tasks;

using ATodoList.Models;
using ATodoList.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

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
        }

        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="e"></param>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _manager = new WindowNotificationManager(this) {
                MaxItems = 3,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };
        }

        /// <summary>
        /// 提示消息
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="type">消息类型</param>
        private void ShowNotification(string? title, string? message, NotificationType type)
        {
            _manager?.Show(new Avalonia.Controls.Notifications.Notification(title, message, type));
        }

        /// <summary>
        /// 提示消息，消息标题由消息类型确定
        /// </summary>
        /// <param name="message">消息标题</param>
        /// <param name="type">消息类型</param>
        private void ShowNotification(string? message, NotificationType type) => ShowNotification(type.ToString(), message, type);

        /// <summary>
        /// 绑定组名切换事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupItemList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectGroupName = ((sender as ListBox)?.SelectedItem as TodoGroupItem)?.Name ?? string.Empty;

            ViewModel!.SelectedGroupName = selectGroupName;
            ViewModel!.ReloadTodoItemsFromCurrentSelectedGroup();
        }

        /// <summary>
        /// 异步复制当前选中的组名事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Async_GroupItemList_ContextMenu_CopyMenuItem_CopyGroupName(object sender, RoutedEventArgs e)
        {
            var selectGroupName = ViewModel!.SelectedGroupName;
            await AsyncSendTextToSystemClipboard(selectGroupName);
            ShowNotification("待办事项组名已复制", NotificationType.Information);
        }

        /// <summary>
        /// 异步将字符串输入至系统剪贴板
        /// </summary>
        /// <param name="text">期望复制的内容</param>
        /// <returns></returns>
        private async Task AsyncSendTextToSystemClipboard(string text)
        {
            var clipboard = Clipboard;
            if (clipboard is not null) {
                await clipboard.SetTextAsync(text);
            }
        }

        /// <summary>
        /// 异步复制选中未完成待办事项的标题事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Async_CopySelectedYieldTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("YieldFinishTodoItemListBox");

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                await AsyncSendTextToSystemClipboard(currentTodoItem.Title);
                ShowNotification("待办事项标题已复制", NotificationType.Information);
            }
        }

        /// <summary>
        /// 移除选中的未完成待办事项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 异步复制选中已完成的待办事项标题事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Async_CopySelectedFinishTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("FinishedTodoItemListBox");

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                await AsyncSendTextToSystemClipboard(currentTodoItem.Title);
                ShowNotification("待办事项标题已复制", NotificationType.Information);
            }
        }

        /// <summary>
        /// 移除选中已完成的待办事项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 删除当前选中的组事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupItemList_ContextMenu_RemoveMeunItem_RemoveSelectedGroup(object sender, RoutedEventArgs e)
        {
            if (ViewModel!.RemoveSelectedGroup(ViewModel!.SelectedGroupName)) {
                ShowNotification("选中待办事项组已删除", NotificationType.Success);
            } else {
                ShowNotification("选中待办事项组未删除", NotificationType.Error);
            }
        }

        /// <summary>
        /// 侦测添加组输入框文本变动事件，当文本框内有内容时启用提交按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 添加待办事项组回车提交事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 添加待办事项组提交按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 侦测待办事项下拉框展开事件，当待办事项下拉框展开时，将所选的待办事项设置为选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodoItemListBox_Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expander)
                return;

            var listBoxItem = expander.FindAncestorOfType<ListBoxItem>();
            if (listBoxItem is not null) {
                listBoxItem.IsSelected |= expander.IsExpanded;
            }
        }

        /// <summary>
        /// 添加待办事项回车提交事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupItemList_ContextMenu_TextBoxKeyDown_EnterSubmit(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) {
                return;
            }

            //Debug.WriteLine($"{e.Key} {e.KeyModifiers}");
            if (e.Key is not Key.Enter) {
                return;
            }
                
            //Debug.WriteLine("Submit");

            var newName = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(newName) ) {
                ShowNotification("需要输入内容", NotificationType.Warning);
                return;
            }

            textBox.Text = string.Empty;

            var parent = this.FindControl<ListBox>("GroupItemList")?.ContextMenu;

            if (parent is null)
                return;

            //Debug.WriteLine("GroupItemList > ContextMenu");

            if (ViewModel!.RenameSelectedGroupName(newName)) {
                ShowNotification("待办事项组名已更改", NotificationType.Success);
            } else {
                ShowNotification("待办事项组名未更改", NotificationType.Error);
            }
        }
    
        /// <summary>
        /// 切换待办事项完成状态事件
        /// </summary>
        /// <param name="sender">待办事项的CheckBox</param>
        /// <param name="e"></param>
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
                ShowNotification($"任务 [{item.Title}] {(!item.IsFinish ? "已完成" : "未完成")}", NotificationType.Information);
            }
        }

        /// <summary>
        /// 提交待办事项的修改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 回车添加待办事项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTodoItemList_InputTextBox_PressEnterCommit(object sender, KeyEventArgs e)
        {
            if (e.Key is not Key.Enter)
                return;

            if (sender is not TextBox textBox)
                return;

            var newItemTitle = textBox.Text;
            textBox.Text = string.Empty;

            if (ViewModel!.AddNewTodoItemToCurrentGroup(newItemTitle)) {
                //textBox.BorderBrush = new SolidColorBrush(Colors.Aquamarine);
                ShowNotification("任务添加成功", NotificationType.Success);
            } else {
                //textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ShowNotification("任务添加失败", NotificationType.Error);
            }
        }

        /// <summary>
        /// TreeViewItem忽略回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodoItemList_TreeViewItem_IgnorePressEnter(object sender, KeyEventArgs e) {
            if (e.Key is Key.Enter) {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 移动未完成事项到其他组事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YieldFinishTodoItemList_ContextMenu_MoveItem_PointerEnter(object sender, PointerEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            menuItem.Items.Clear();
            foreach (var item in ViewModel!.GroupItems) {
                if (item.Name == ViewModel!.SelectedGroupName) {
                    continue;
                }

                var subItem = new MenuItem() {
                    Header = item.Name,
                };
                subItem.Click += (object? sender, RoutedEventArgs e) => {
                    var listBox = this.FindControl<ListBox>("YieldFinishTodoItemListBox");

                    bool result = false;
                    if (listBox?.SelectedItem is TodoItem currentTodoItem && subItem.Header is string groupName) {
                        result = ViewModel!.RemoveTodoItem(currentTodoItem.ObjectId);
                        result = ViewModel!.AddNewTodoItemToGroup(groupName,
                                                                  currentTodoItem.Title,
                                                                  currentTodoItem.DeadLine,
                                                                  currentTodoItem.Description,
                                                                  currentTodoItem.IsFinish);
                    }

                    if (result) {
                        ShowNotification($"选中待办事项已经移动至 {subItem.Header}", NotificationType.Success);
                    } else {
                        ShowNotification("选中待办事项移动失败", NotificationType.Error);
                    }
                };

                menuItem.Items.Add(subItem);
            }
        }

        /// <summary>
        /// 移动已完成事项到其他组事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishTodoItemList_ContextMenu_MoveItem_PointerEnter(object sender, PointerEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            menuItem.Items.Clear();
            foreach (var item in ViewModel!.GroupItems) {
                if (item.Name == ViewModel!.SelectedGroupName) {
                    continue;
                }

                var subItem = new MenuItem() {
                    Header = item.Name,
                };
                subItem.Click += (object? sender, RoutedEventArgs e) => {
                    var listBox = this.FindControl<ListBox>("FinishedTodoItemListBox");

                    bool result = false;
                    if (listBox?.SelectedItem is TodoItem currentTodoItem && subItem.Header is string groupName) {
                        result = ViewModel!.RemoveTodoItem(currentTodoItem.ObjectId);
                        result = ViewModel!.AddNewTodoItemToGroup(groupName,
                                                                  currentTodoItem.Title,
                                                                  currentTodoItem.DeadLine,
                                                                  currentTodoItem.Description,
                                                                  currentTodoItem.IsFinish);
                    }

                    if (result) {
                        ShowNotification($"选中待办事项已经移动至 {subItem.Header}", NotificationType.Success);
                    } else {
                        ShowNotification("选中待办事项移动失败", NotificationType.Error);
                    }
                };

                menuItem.Items.Add(subItem);
            }
        }

        /// <summary>
        /// 打开设置对话框事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenNewSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsWindow();
            settingsDialog.ViewModel = new SettingsWindowViewModel();

            (string?, string?) prev = (
                    (Services.DatabaseService.DB as Services.MongoDBHelper)?.ConnectionString?? null,
                    (Services.DatabaseService.DB as Services.MongoDBHelper)?.DatabaseName ?? null
                );
            
            await settingsDialog.ShowDialog<Unit>(this);
        
            if (prev != (
                    (Services.DatabaseService.DB as Services.MongoDBHelper)?.ConnectionString ?? null,
                    (Services.DatabaseService.DB as Services.MongoDBHelper)?.DatabaseName ?? null
                )) {
                ViewModel!.ResetStatus();
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