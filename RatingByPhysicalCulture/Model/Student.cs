using RatingByPhysicalCulture.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Xml.Serialization;

namespace RatingByPhysicalCulture.Model
{
	[Serializable]
	public class Student : INotifyPropertyChanged
    {
		public class Result : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler? PropertyChanged;

			public int? Value
			{
				get => _value;
				set
				{
					_value = value;
					OnPropertyChanged();
				}
			}
			public DateTime? Time
			{
				get => _time;
				set
				{
					_time = value;
					OnPropertyChanged();
				}
			}
			public int? Rating
			{
				get => _rating;
				set
				{
					_rating = value;
					OnPropertyChanged();
				}
			}

			private int? _value;
			private DateTime? _time;
			private int? _rating;

			protected void OnPropertyChanged([CallerMemberName] string prop = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
			}
		}
		public event PropertyChangedEventHandler? PropertyChanged;

		public string? Name
		{
			get => _name;
			set
			{
				_name = value == "" ? null : value;
				OnPropertyChanged();
			}
		}
		public string? Group
		{
			get => _group;
			set
			{
				_group = value == "" ? null : value;
				OnPropertyChanged();
			}
		}
		public int? Rating
		{
			get => _rating;
			set
			{
				_rating = value;
				OnPropertyChanged();
			}
		}
		public List<Result> Results { get; set; }
		
		private string? _name;
		private string? _group;
		private int? _rating;
		
		public Student()
		{
			Results = new List<Result>();
		}

		protected void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
