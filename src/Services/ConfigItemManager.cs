using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MesiIK.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MesiIK.Services
{
    public class ConfigItemManager
    {
        private List<DraggableTextBox> configItems = new List<DraggableTextBox>();
        private Canvas ConfigCanvas;
        private MessageBox _messageBox;
        private Point _startPoint;
        private DraggableTextBox? _draggedItem;

        public ConfigItemManager(Canvas configCanvas, MessageBox messageBox)
        {
            ConfigCanvas = configCanvas;
            _messageBox = messageBox;
            InitializeConfigItems();
            LoadSettings();
        }

        public void InitializeConfigItems()
        {
            AddConfigItem("Server Inbound Address", "127.0.0.1", 0, 0);
            AddConfigItem("Server Inbound Port", "8080", 1, 0);
            AddConfigItem("Client Outbound Address", "127.0.0.1", 0, 1);
            AddConfigItem("Client Outbound Port", "8080", 1, 1);
        }

        public void AddConfigItem(string label, string defaultValue, int column, int row)
        {
            var item = new DraggableTextBox
            {
                Label = label,
                Text = defaultValue,
                Width = 200,
                Height = 80
            };

            item.PointerPressed += ConfigItem_PointerPressed;
            item.PointerMoved += ConfigItem_PointerMoved;
            item.PointerReleased += ConfigItem_PointerReleased;

            double x = 10 + column * 210;
            double y = 10 + row * 90;

            Canvas.SetLeft(item, x);
            Canvas.SetTop(item, y);

            ConfigCanvas.Children.Add(item);
            configItems.Add(item);
        }

        private void ConfigItem_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(ConfigCanvas).Properties.IsLeftButtonPressed)
            {
                _draggedItem = sender as DraggableTextBox;
                if (_draggedItem != null)
                {
                    _startPoint = e.GetPosition(ConfigCanvas);
                    _draggedItem.SetDraggingCursor(true);
                    ConfigCanvas.PointerMoved += ConfigItem_PointerMoved;
                    ConfigCanvas.PointerReleased += ConfigItem_PointerReleased;
                }
            }
        }

        private void ConfigItem_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_draggedItem != null)
            {
                var currentPosition = e.GetPosition(ConfigCanvas);
                var offset = currentPosition - _startPoint;

                Canvas.SetLeft(_draggedItem, Canvas.GetLeft(_draggedItem) + offset.X);
                Canvas.SetTop(_draggedItem, Canvas.GetTop(_draggedItem) + offset.Y);

                _startPoint = currentPosition;
            }
        }

        private void ConfigItem_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_draggedItem != null)
            {
                _draggedItem.SetDraggingCursor(false);
                _draggedItem = null;
                ConfigCanvas.PointerMoved -= ConfigItem_PointerMoved;
                ConfigCanvas.PointerReleased -= ConfigItem_PointerReleased;
            }
        }

        public void SaveSettings()
        {
            var settings = new Dictionary<string, object>();
            foreach (var item in configItems)
            {
                settings[item.Label] = new
                {
                    Text = item.Text,
                    X = Canvas.GetLeft(item),
                    Y = Canvas.GetTop(item)
                };
            }

            try
            {
                File.WriteAllText("settings.json", JsonSerializer.Serialize(settings));
                _messageBox.ShowMessage("Settings saved successfully!", MessageType.Success, TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                _messageBox.ShowMessage($"Failed to save settings: {ex.Message}", MessageType.Error);
            }
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    var settingsJson = File.ReadAllText("settings.json");
                    var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(settingsJson);
                    
                    if (settings != null)
                    {
                        foreach (var item in configItems)
                        {
                            string key = item.Label ?? string.Empty;
                            if (settings.TryGetValue(key, out var value))
                            {
                                item.Text = value.GetProperty("Text").GetString() ?? string.Empty;
                                Canvas.SetLeft(item, value.GetProperty("X").GetDouble());
                                Canvas.SetTop(item, value.GetProperty("Y").GetDouble());
                            }
                        }
                    }
                    else
                    {
                        _messageBox.ShowMessage("Failed to load settings: Settings file is empty or invalid.", MessageType.Error);
                    }
                }
                catch (JsonException ex)
                {
                    _messageBox.ShowMessage($"Failed to load settings: {ex.Message}", MessageType.Error);
                }
                catch (IOException ex)
                {
                    _messageBox.ShowMessage($"Failed to read settings file: {ex.Message}", MessageType.Error);
                }
            }
        }

        public void ResetConfigItems()
        {
            configItems.Clear();
            ConfigCanvas.Children.Clear();
            InitializeConfigItems();
        }

        public string GetConfigItemValue(string label)
        {
            return configItems.Find(i => i.Label == label)?.Text ?? string.Empty;
        }
    }
}
