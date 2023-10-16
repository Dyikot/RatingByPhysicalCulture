using RatingByPhysicalCulture.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Win32;
using RatingByPhysicalCulture.Model;

namespace RatingByPhysicalCulture.Windows
{
	public enum OpenMode { Default, NewProject }

	public partial class StartWindow : Window
	{
		private readonly ProjectListModel _projectListModel;

		public StartWindow()
		{
			InitializeComponent();
			_projectListModel = new ProjectListModel(_recentProjectList);
		}

		public StartWindow(OpenMode openMode)
		{
			InitializeComponent();
			_projectListModel = new ProjectListModel(_recentProjectList);

			if (openMode is OpenMode.NewProject)
			{
				_tabControl.SelectedItem = createNewProjectMenu;
				_backButton.Visibility = Visibility.Collapsed;
			}
		}

		private void OnWindowClosed(object sender, EventArgs e)
		{
			if (ProjectInfo.GetInstance.IsProjectUpToLoad)
			{
				Application.Current.MainWindow.Show();
			}
		}

		// Стартовое меню
		private void OnCreateNewProjectButtonClick(object sender, RoutedEventArgs e)
		{
			// Переход на вкладку создания нового проекта
			_tabControl.SelectedItem = createNewProjectMenu;
		}
		private void OnOpenProjectButtonClick(object sender, RoutedEventArgs e)
		{
			var filePath = Project.GetProjectPathInDialog();
			if (filePath is not null)
			{
				OpenProject(filePath);
			}
				
		}
		private void OnProjectListDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var project = ((ListBoxItem)sender).Content as Project;
			if (File.Exists(project?.Path))
			{
				_projectListModel.UpdateTime(project);
				_projectListModel.SerializeProjects();
				OpenProject(project.Path);
			}	
			else
			{
				if(MessageBox.Show(
						$"Файл \"{project?.Name}\" не найден. Удалить из списка проектов?",
						"Ошибка при отрытии проекта",
						MessageBoxButton.YesNo,
						MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					_projectListModel.RemoveProject(project);
					_projectListModel.SerializeProjects();
				}
			}
		}

		// Меню создания проекта
		private void OnProjectNameTextBoxLoaded(object sender, RoutedEventArgs e)
		{
			if (_projectName.Focus())
			{
				_projectName.SelectAll();
			}
		}
		private void OnProjectLocationButtonClick(object sender, RoutedEventArgs e)
		{
			var GetNewProjectPath = () =>
			{
				var saveFileDialog = new SaveFileDialog()
				{
					InitialDirectory = Environment.CurrentDirectory,
					FileName = _projectName.Text,
					Filter = "Расширение проекта (*.xml)|*.xml",
					Title = "Создание проекта"
				};

				if (saveFileDialog.ShowDialog() is true)
				{
                    _projectName.Text = Path.GetFileNameWithoutExtension(
											saveFileDialog.SafeFileName);
					return Path.GetDirectoryName(saveFileDialog.FileName);
				}
				else
				{
					return null;
				}
			};

			var path = GetNewProjectPath();
			if (path is not null)
			{
				_projectPath.Text = path;
			}
		}
		private void OnBackButtonClick(object sender, RoutedEventArgs e)
		{
			// Переход на стартовую вкладку
			_tabControl.SelectedItem = startMenu;
		}
		private void OnCreateButtonClick(object sender, RoutedEventArgs e)
		{
			void HighlightTextBox(TextBox textBox)
			{
				textBox.BorderBrush = new SolidColorBrush(Colors.Red);
			}

			if (!Path.IsPathRooted(_projectPath.Text))
			{
				HighlightTextBox(_projectPath);
			}
			else
			{
				bool replaceProject = false;
				var directoryPath = $"{_projectPath.Text}\\{_projectName.Text}";

				if (Directory.Exists(directoryPath))
				{
					if (MessageBox.Show(
							"Проект с таким именем уже существует. Заменить на новый?",
							"Создание проекта",
							MessageBoxButton.YesNo,
							MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						replaceProject = true;
					}
					else
					{
						return;
					}
				}

				var projectPath = $"{directoryPath}\\{_projectName.Text}.xml";

				ProjectInfo CreateProject(string projectPath)
				{
					ProjectInfo projectInfo = ProjectInfo.GetInstance;
					projectInfo.ProjectPath = projectPath;

					Directory.CreateDirectory(projectInfo.ProjectDirectory);
					using (var fileStream = File.Create(projectInfo.ProjectPath)){ }
					using (var fileStream = File.Create(projectInfo.ColumnsHeaderInfoPath)){ }

					return projectInfo;
				}
				void AddProject(ProjectInfo projectInfo)
				{
					if (replaceProject)
					{
						_projectListModel.RemoveProject(project =>
											project.Path ==
											ProjectInfo.GetInstance.ProjectPath);
					}

					_projectListModel.AddProject(new Project()
					{
						Name = projectInfo.ProjectName,
						Path = projectInfo.ProjectPath,
						TimeOpened = new FileInfo(
											projectInfo.ProjectPath)
											.LastAccessTime
											.ToString(Project.TimeFortmat)
					});

					_projectListModel.SerializeProjects();
				}

				AddProject(CreateProject(projectPath));
				OpenProject(projectPath);
			}
		}
		private void OnProjectPathInitialized(object sender, EventArgs e)
		{
			_projectPath.Text = Environment.CurrentDirectory;
		}

		private void OpenProject(string filePath)
		{
			ProjectInfo.GetInstance.ProjectPath = filePath;
			Application.Current.MainWindow = new MainWindow();
			foreach (Window window in Application.Current.Windows)
			{
				if (window != Application.Current.MainWindow &&
					(window.GetType() == typeof(MainWindow) ||
					 window.GetType() == typeof(StartWindow)))
				{
					window.Close();
				}
			}
		}
	}
}
