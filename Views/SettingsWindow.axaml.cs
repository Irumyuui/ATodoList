using ATodoList.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ATodoList.Views;

public partial class SettingsWindow : ReactiveWindow<SettingsWindowViewModel>
{
    private WindowNotificationManager? _manager;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _manager = new WindowNotificationManager(this) {
            MaxItems = 3,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
    }

    private void ShowNotification(string? title, string? message, NotificationType type)
    {
        _manager?.Show(new Avalonia.Controls.Notifications.Notification(title, message, type));
    }

    private void ShowNotification(string? message, NotificationType type) => ShowNotification(type.ToString(), message, type);

    private void CommitSetDatabase(object sender, RoutedEventArgs e)
    {
        var host = string.IsNullOrWhiteSpace(MongoDBHostTextBox.Text) ? "127.0.0.1:27017" : MongoDBHostTextBox.Text;
        var db = string.IsNullOrWhiteSpace(DatabaseTextBox.Text) ? "ATodoList" : DatabaseTextBox.Text;

        if (ViewModel!.SwitchDatabase(host, db)) {
            ShowNotification("提交成功", NotificationType.Success);
        } else {
            ShowNotification("提交失败", NotificationType.Error);
        }
    }
}