using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;

namespace MesiIK.Controls
{
    public class DraggableTextBox : Border
    {
        private Canvas _canvas;
        private TextBlock _labelTextBlock;
        private TextBox _textBox;


        public string Label
        {
            get => _labelTextBlock?.Text ?? string.Empty;
            set
            {
                if (_labelTextBlock != null)
                    _labelTextBlock.Text = value;
            }
        }

        public string Text
        {
            get => _textBox?.Text ?? string.Empty;
            set
            {
                if (_textBox != null)
                    _textBox.Text = value;
            }
        }

        public DraggableTextBox()
        {
            Background = Brushes.White;
            BorderBrush = Brushes.Black;
            BorderThickness = new Thickness(1);
            Cursor = new Cursor(StandardCursorType.Hand);

            _canvas = new Canvas();
            Child = _canvas;

            _labelTextBlock = new TextBlock
            {
                FontSize = 12,
                Margin = new Thickness(5, 5, 5, 0)
            };

            _textBox = new TextBox
            {
                FontSize = 14,
                Margin = new Thickness(5, 25, 5, 5)
            };

            _canvas.Children.Add(_labelTextBlock);
            _canvas.Children.Add(_textBox);

            PropertyChanged += (sender, args) =>
            {
                if (args.Property.Name == nameof(Bounds))
                {
                    UpdateChildrenBounds();
                }
            };
        }

        public void SetDraggingCursor(bool isDragging)
        {
            Cursor = new Cursor(isDragging ? StandardCursorType.DragLink : StandardCursorType.Hand);
        }

        private void UpdateChildrenBounds()
        {
            _canvas.Width = Width;
            _canvas.Height = Height;
            _labelTextBlock.Width = Width - 10;
            _textBox.Width = Width - 10;
            _textBox.Height = Height - 30;
        }
    }
}