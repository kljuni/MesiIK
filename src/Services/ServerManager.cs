using MesiIK.Controls;
using System;

namespace MesiIK.Services
{
    public class ServerManager
    {
        private readonly HttpServer _httpServer;
        private readonly MesiHttpClient _httpClient;
        private bool _isServerRunning = false;
        private MessageBox _messageBox;

        public ServerManager(HttpServer httpServer, MesiHttpClient httpClient, MessageBox messageBox)
        {
            _httpServer = httpServer;
            _httpClient = httpClient;
            _messageBox = messageBox;
        }

        public void Start(string serverAddress, string clientAddress, string serverPortString, string clientPortString)
        {
            if (_isServerRunning) return;

            if (!ValidateInputs(serverAddress, clientAddress, serverPortString, clientPortString, out int serverPort, out int clientPort))
            {
                return;
            }

            try
            {
                _httpServer.Start(serverAddress, serverPort);
                _httpClient.Configure(clientAddress, clientPort);

                _isServerRunning = true;
                _messageBox.ShowMessage("Server started successfully!", MessageType.Success, TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                _messageBox.ShowMessage($"Failed to start server: {ex.Message}", MessageType.Error);
            }
        }

        private bool ValidateInputs(string serverAddress, string clientAddress, string serverPortString, string clientPortString, out int serverPort, out int clientPort)
        {
            serverPort = 0;
            clientPort = 0;

            if (string.IsNullOrWhiteSpace(serverAddress))
            {
                _messageBox.ShowMessage("Server inbound address must not be empty.", MessageType.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(clientAddress))
            {
                _messageBox.ShowMessage("Client outbound address must not be empty.", MessageType.Error);
                return false;
            }

            if (!int.TryParse(serverPortString, out serverPort))
            {
                _messageBox.ShowMessage("Invalid server inbound port. Please enter a valid number.", MessageType.Error);
                return false;
            }

            if (!int.TryParse(clientPortString, out clientPort))
            {
                _messageBox.ShowMessage("Invalid client outbound port. Please enter a valid number.", MessageType.Error);
                return false;
            }

            if (!IsPortValid(serverPort))
            {
                _messageBox.ShowMessage("Server inbound port number must be between 1024 and 49151.", MessageType.Error);
                return false;
            }

            return true;
        }

        public void Stop()
        {
            if (!_isServerRunning) return;

            _httpServer.Stop();
            _isServerRunning = false;
        }

        private bool IsPortValid(int port)
        {
            return port >= 1024 && port <= 49151;
        }

        public bool IsServerRunning => _isServerRunning;
    }
}
