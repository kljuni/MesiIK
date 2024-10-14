#nullable enable

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace MesiIK.Controls
{
    public partial class MessageBox : UserControl
    {
        private DispatcherTimer _timer;

        public MessageBox()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }

        public void ShowMessage(string message, MessageType type, TimeSpan? displayTime = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBoxBorder.IsVisible = false;
                return;
            }

            MessageBoxBorder.IsVisible = true;
            MessageText.Text = message;

            switch (type)
            {
                case MessageType.Success:
                    MessageBoxBorder.Background = new SolidColorBrush(Colors.LightGreen);
                    MessageText.Foreground = new SolidColorBrush(Colors.DarkGreen);
                    break;
                case MessageType.Error:
                    MessageBoxBorder.Background = new SolidColorBrush(Colors.LightCoral);
                    MessageText.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case MessageType.Info:
                    MessageBoxBorder.Background = new SolidColorBrush(Colors.LightBlue);
                    MessageText.Foreground = new SolidColorBrush(Colors.DarkBlue);
                    break;
            }

            if (displayTime.HasValue)
            {
                _timer.Interval = displayTime.Value;
                _timer.Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            MessageBoxBorder.IsVisible = false;
            _timer.Stop();
        }

        private void CloseMessageBox_Click(object? sender, RoutedEventArgs e)
        {
            MessageBoxBorder.IsVisible = false;
            _timer.Stop();
        }
    }

    public enum MessageType
    {
        Success,
        Error,
        Info
    }
}
