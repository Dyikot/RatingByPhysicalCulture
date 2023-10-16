using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RatingByPhysicalCulture.Model
{
	public class Project : INotifyPropertyChanged
	{
		public string? Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}
		public string? TimeOpened
		{
			get => _timeOpened;
			set
			{
				_timeOpened = value;
				OnPropertyChanged();
			}
		}
		public string? Path
		{
			get => _path;
			set
			{
				_path = value;
				OnPropertyChanged();
			}
		}
		public event PropertyChangedEventHandler? PropertyChanged;

		private string? _name;
		private string? _timeOpened;
		private string? _path;

		public static string TimeFortmat { get => "dd/MM/yyyy HH:mm"; }
		public static string? GetProjectPathInDialog()
		{
			var openFileDialog = new OpenFileDialog()
			{
				InitialDirectory = Environment.CurrentDirectory,
				Filter = "Расширение проекта (*.xml)|*.xml|Все файлы (*.*)|*.*",
				FilterIndex = 1,
				Title = "Отрытие проекта"
			};

			if (openFileDialog.ShowDialog() != true &&
				openFileDialog.FileName != "ProjectsList")
				return null;
			else
				return openFileDialog.FileName;
		} 

		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
