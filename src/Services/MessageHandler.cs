using MesiIK.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MesiIK.Services
{
    public class MessageHandler
    {
        private readonly MesiHttpClient _httpClient;
        private MessageBox _messageBox;

        public MessageHandler(MesiHttpClient httpClient, MessageBox messageBox)
        {
            _httpClient = httpClient;
            _messageBox = messageBox;
        }

        public async Task<(bool Success, string Response)> SendMessage(string messageBody, string headers)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                _messageBox.ShowMessage("Message body cannot be empty.", MessageType.Error);
                return (false, "Message body cannot be empty.");
            }

            try
            {
                string response = await _httpClient.SendRequest(messageBody, headers);
                
                if (response != null)
                {
                    _messageBox.ShowMessage("", MessageType.Error);
                    _messageBox.ShowMessage("", MessageType.Info);
                    return (true, response);
                }
                else
                {
                    return (false, "No response received from server.");
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused"))
            {
                string errorMessage = "Connection refused. Make sure the server is running and the address/port are correct.";
                _messageBox.ShowMessage(errorMessage, MessageType.Error);
                return (false, errorMessage);
            }
            catch (TaskCanceledException)
            {
                string errorMessage = "The request timed out after 5 seconds.";
                _messageBox.ShowMessage(errorMessage, MessageType.Error);
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                _messageBox.ShowMessage(errorMessage, MessageType.Error);
                return (false, errorMessage);
            }
        }
    }
}
