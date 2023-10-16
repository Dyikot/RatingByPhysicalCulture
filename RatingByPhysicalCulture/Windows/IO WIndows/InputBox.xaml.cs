using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RatingByPhysicalCulture.Windows
{
	public partial class InputBox : Window
    {
		private readonly SolidColorBrush DefaultColor =
			new SolidColorBrush(Color.FromArgb(0xff, 0xab, 0xad, 0xb3));

		public InputBox(string messageBoxText, string caption)
        {
            InitializeComponent();
            Title = caption;
            _message.Text = messageBoxText;
		}

        public static string? Show(string messageBoxText, string caption)
        {
			var inputBox = new InputBox(messageBoxText, caption);
			return inputBox.ShowDialog() is true ? inputBox._answer.Text : null;
		}

		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{
			if (IsAnswered())
			{
				DialogResult = true;
			}
		}
		private void OnOkButtonMouseLeave(object sender, MouseEventArgs e)
		{
			UnHighlightControl(_answer);
		}

		private bool IsAnswered()
		{
			if (_answer.Text != string.Empty)
			{
				return true;
			}

			HighlightTextBox(_answer);
			return false;
		}
		private void HighlightTextBox(Control control)
		{
			control.BorderBrush = Brushes.Red;
		}
		private void UnHighlightControl(Control control)
		{
			control.BorderBrush = DefaultColor;
		}
	}
}
