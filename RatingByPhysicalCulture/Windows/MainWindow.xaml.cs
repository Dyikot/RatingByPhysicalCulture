using Microsoft.Win32;
using RatingByPhysicalCulture.Model;
using RatingByPhysicalCulture.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RatingByPhysicalCulture
{
	public enum PasteMode { WithoutRating, WithRating };
}

namespace RatingByPhysicalCulture.Windows
{
	public partial class MainWindow : Window
	{
		private PasteMode _pasteMode = PasteMode.WithoutRating;
		private TableModel _tableModel;

		public MainWindow()
		{
			InitializeComponent();			
		}

		private void OnAddModeItemClick(object sender, RoutedEventArgs e)
		{
			var mode = (MenuItem)e.Source;
			if (!mode.IsChecked)
			{
				mode.IsChecked = true;
				return;
			}

			// При выборе режима вставки у другого режима поле "IsChecked" становится "false".
			// Индексы режимов вставки:
			// '0' - без учета столбцов рейтинга.
			// '1' - с учетом столбцов рейтинга.
			var modeCollection = (MenuItem)mode.Parent;
			_pasteMode = (PasteMode)modeCollection.Items.IndexOf(mode);

			mode = _pasteMode switch
			{
				PasteMode.WithoutRating => (MenuItem)modeCollection.Items[1],
				PasteMode.WithRating => (MenuItem)modeCollection.Items[0],
				_ => throw new NotImplementedException(),
			};
			mode.IsChecked = false;
		}
		private void OnResultColumnHeaderClick(object sender, RoutedEventArgs e)
		{
			int columnIndex = Grid.GetColumn(e.Source as Button);
			SortDataGridColumn(tableBody, columnIndex);
		}
		private void OnTableBodyInitNewItem(object sender, InitializingNewItemEventArgs e)
		{
			_tableModel.InitializeResultsAtNewRow();
		}
		private void OnTableBodyScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.HorizontalChange != 0)
			{
				headerScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
			}
		}
		private void OnWindowLoaded(object sender, RoutedEventArgs e)
		{
			ProjectInfo.GetInstance.IsProjectUpToLoad = false;
			_tableModel = new TableModel(tableHeader, tableBody);
		}
		private void OnWindowClosed(object sender, EventArgs e)
		{
			if (ProjectInfo.GetInstance.IsProjectUpToLoad)
			{
				Application.Current.MainWindow.Show();
			}
		}

		private void OnAddColumnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var columnName = InputBox.Show(
									"Введите название столбца:",
									"Добавление столбца");
			if (columnName is not null)
			{
				_tableModel.AddColumn(columnName);
			}
		}
		private void OnCloseWindowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}
		private void OnHelpCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			new HelpWindow().Show();
		}
		private void OnMakeRatingCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			_tableModel.MakeRating();
		}
		private void OnNewProjectCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			new StartWindow(OpenMode.NewProject) { Visibility = Visibility.Visible };
		}
		private void OnOpenProjectCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			void OpenProject(string filePath)
			{
				ProjectInfo.GetInstance.ProjectPath = filePath;
				Application.Current.MainWindow = new MainWindow();
				Close();
			}

			var filePath = Project.GetProjectPathInDialog();
			if (filePath is not null)
			{
				OpenProject(filePath);
			}				
		}
		private void OnPasteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			_tableModel.Paste(Clipboard.GetText(), _pasteMode);
		}
		private void OnSaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			_tableModel.SerializeData();
		}

		private void SortDataGridColumn(DataGrid dataGrid, int columnIndex)
		{
			var performSort =
				typeof(DataGrid).GetMethod("PerformSort",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);
			performSort?.Invoke(dataGrid, new[] { dataGrid.Columns[columnIndex] });
		}
	}
}
