using ATodoList.Models;
using ATodoList.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        private WindowNotificationManager? _manager;

        public MainWindow()
        {
            InitializeComponent();
            //_manager = new WindowNotificationManager(this) { MaxItems = 3 };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var topLevel = TopLevel.GetTopLevel(this);
            _manager = new WindowNotificationManager(topLevel) { MaxItems = 3 };
        }

        private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            _manager?.Show(new Notification("123", "This is message", NotificationType.Information));
        }

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
        }

        private async Task AsyncSendTextToSystemClipboard(string text)
        {
            var clipboard = Clipboard;
            if (clipboard is not null) {
                await clipboard.SetTextAsync(text);
            }
        }

        private async void Async_CopySelectedTodoItemTitle(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("YieldFinishTodoItemListBox");

            if (listBox?.SelectedItem is TodoItem currentTodoItem) {
                await AsyncSendTextToSystemClipboard(currentTodoItem.Title);
            }
        }

        private void GroupItemList_ContextMenu_RemoveMeunItem_RemoveSelectedGroup(object sender, RoutedEventArgs e)
        {
            ViewModel!.RemoveSelectedGroup(ViewModel!.SelectedGroupName);
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
            ViewModel!.AddGroup(result);
        }

        private void CommitGroupNmaeButton_CommitClick(object sender, RoutedEventArgs e)
        {
            var result = InputGroupNameTextBox?.Text?.Trim();
            if (result is null)
                return;

            InputGroupName.Text = string.Empty;
            ViewModel!.AddGroup(result);
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
                return;
            }

            textBox.Text = string.Empty;

            var parent = this.FindControl<ListBox>("GroupItemList")?.ContextMenu;

            if (parent is null)
                return;

            Debug.WriteLine("GroupItemList > ContextMenu");

            ViewModel!.RenameSelectedGroupName(newName);

            
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