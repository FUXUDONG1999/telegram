using System.Windows;

namespace telegram_downloader_desktop;

public partial class Login : Window
{
    public Login()
    {
        InitializeComponent();
    }

    private void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}