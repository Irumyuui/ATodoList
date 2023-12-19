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
    /// 提交database设置，没有填写则设置为默认host: 127.0.0.1:27017和database name: ATodoList
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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