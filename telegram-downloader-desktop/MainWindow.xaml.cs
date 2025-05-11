using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using downloader.model;
using Telegram.config;
using Telegram.manager;
using Telegram.models;

namespace telegram_downloader_desktop;

public partial class MainWindow : Window
{
    private readonly TelegramDownloadManager _manager;

    public MainWindow()
    {
        InitializeComponent();
        _manager = new TelegramDownloadManager(TelegramConfig.DownloadOutputDir, () => {
            return Dispatcher.Invoke(() => {
                Login loginWindow = new();

                var result = loginWindow.ShowDialog();
                if (result == true) return loginWindow.Code.Text;

                throw new Exception("not found code");
            });
        });

        ThreadPool.QueueUserWorkItem(_ => { _manager.Start(); });

        var dispatcher = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        dispatcher.Tick += (_, _) => {
            DownloadTasks.ItemsSource = _manager.GetTasks();
            Label.Content =
                $"Waiting tasks: {_manager.Queue.WaitingTasksCount}, Downloading tasks: {_manager.Queue.ActiveTasksCount}, Completed tasks: {_manager.Queue.CompletedTasksCount}";
        };
        dispatcher.Start();
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        var request = new AddRangeTaskRequest(BeginUrl.Text, EndUrl.Text);
        foreach (var i in Enumerable.Range(request.BeginMessageId, request.EndMessageId - request.BeginMessageId + 1))
            _manager.AddTask(new DownloadTask<Chat>(new Chat(request.ChatId, i)));

        DownloadTasks.ItemsSource = _manager.GetTasks();
    }

    public readonly struct AddRangeTaskRequest(string startUrl, string endUrl)
    {
        public int ChatId => int.Parse(startUrl.Split("/")[4]);

        public int BeginMessageId => int.Parse(startUrl.Split("/")[5]);

        public int EndMessageId => int.Parse(endUrl.Split("/")[5]);
    }
}