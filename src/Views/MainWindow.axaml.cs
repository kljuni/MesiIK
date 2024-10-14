using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MesiIK.Controls;
using MesiIK.Services;
using System;
using System.IO;
using Avalonia.Threading;

namespace MesiIK.Views
{
    public partial class MainWindow : Window
    {
        private readonly ConfigItemManager _configManager;
        private readonly ServerManager _serverManager;
        private readonly MessageHandler _messageHandler;
        private readonly MessageBox _messageBox;
        private readonly TextBox _messageBodyTextBox;
        private readonly TextBox _httpHeadersTextBox;
        private readonly TextBox _receivedMessagesTextBox;
        private readonly Button _startButton;
        private readonly Button _stopButton;

        public MainWindow()
        {
            InitializeComponent();

            // Find and initialize all controls
            _messageBox = FindControlOrThrow<MessageBox>("MessageBox");
            var configCanvas = FindControlOrThrow<Canvas>("ConfigCanvas");
            _messageBodyTextBox = FindControlOrThrow<TextBox>("MessageBody");
            _httpHeadersTextBox = FindControlOrThrow<TextBox>("HttpHeaders");
            _receivedMessagesTextBox = FindControlOrThrow<TextBox>("ReceivedMessages");
            _startButton = FindControlOrThrow<Button>("StartButton");
            _stopButton = FindControlOrThrow<Button>("StopButton");

            // Initialize managers and handlers
            _configManager = new ConfigItemManager(configCanvas, _messageBox);
            var httpServer = new HttpServer();
            var httpClient = new MesiHttpClient();
            _serverManager = new ServerManager(httpServer, httpClient, _messageBox);
            _messageHandler = new MessageHandler(httpClient, _messageBox);

            httpServer.RequestReceived += OnRequestReceived;

            UpdateButtonStates();
        }

        private T FindControlOrThrow<T>(string name) where T : Control
        {
            return this.FindControl<T>(name) 
                ?? throw new InvalidOperationException($"{name} control not found in XAML.");
        }

        private void Start_Click(object? sender, RoutedEventArgs e)
        {
            var serverAddress = _configManager.GetConfigItemValue("Server Inbound Address");
            var clientAddress = _configManager.GetConfigItemValue("Client Outbound Address");
            var serverPortString = _configManager.GetConfigItemValue("Server Inbound Port");
            var clientPortString = _configManager.GetConfigItemValue("Client Outbound Port");

            _serverManager.Start(serverAddress, clientAddress, serverPortString, clientPortString);
            UpdateButtonStates();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _serverManager.Stop();
            UpdateButtonStates();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!_serverManager.IsServerRunning)
            {
                _messageBox.ShowMessage("The server is not running. Please start the server before sending messages.", MessageType.Info);
                return;
            }

            var messageBody = _messageBodyTextBox.Text ?? string.Empty;
            var headers = _httpHeadersTextBox.Text ?? string.Empty;

            var (success, response) = await _messageHandler.SendMessage(messageBody, headers);
            if (success)
            {
                _messageBox.ShowMessage("Message sent successfully", MessageType.Success, TimeSpan.FromSeconds(3));
            }
            else
            {
                _messageBox.ShowMessage($"Failed to send message: {response}", MessageType.Error, TimeSpan.FromSeconds(5));
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _configManager.SaveSettings();
        }

        private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            _configManager.ResetConfigItems();
            _receivedMessagesTextBox.Text = string.Empty;
            _messageBodyTextBox.Text = string.Empty;
            _httpHeadersTextBox.Text = string.Empty;

            if (_serverManager.IsServerRunning)
            {
                _serverManager.Stop();
                UpdateButtonStates();
            }
        }

        private void UpdateButtonStates()
        {
            _startButton.Classes.Clear();
            _startButton.Classes.Add(_serverManager.IsServerRunning ? "disabled" : "enabled");

            _stopButton.Classes.Clear();
            _stopButton.Classes.Add(_serverManager.IsServerRunning ? "enabled" : "disabled");
        }

        private async void OnRequestReceived(string requestInfo)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _receivedMessagesTextBox.Text += $"{requestInfo}\n";
            });
        }
    }
}
