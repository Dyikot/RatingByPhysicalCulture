using RatingByPhysicalCulture.Commands;
using RatingByPhysicalCulture.Converters;
using RatingByPhysicalCulture.Model;
using RatingByPhysicalCulture.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace RatingByPhysicalCulture.ViewModel
{
	public class TableModel
	{
		enum Column { Name, Group, Value, Time, Rating };
		private const int MainColumnsAmount = 3;
		private const int ResultColumnSizeInChildren = 4;
		private const int FirstResultColumnIndexInChildren = 3;

		public int ColumnsAmount { get => _header.ColumnDefinitions.Count; }
		public int ResultColumnsAmount { get => (ColumnsAmount - MainColumnsAmount) / 3; }

		private ObservableCollection<Student> _students;
		private ObservableCollection<Student> Students
		{
			get => _students;
			set
			{
				_students = value;
				_body.ItemsSource = value;
			}
		}
		private readonly Grid _header;
		private readonly DataGrid _body;
		private Button? _ratingColumn;

		public TableModel(Grid gridHeader, DataGrid dataGrid)
		{
			_header = gridHeader;
			_body = dataGrid;
			Students = new ObservableCollection<Student>();

			InitMainColumns();
			if (!TryDeserializeData())
			{
				AddDefaultColumns();
			}
		}

		public void AddColumn(string? name)
		{
			const int NumberOfMainColumnsBehindResultColumns = 1;
			string[] SubColumnsText =
			{
				"р\nа\nз",			// Раз
				"в\nр\nе\nм\nя",	// Время
				"м\nе\nс\nт\nо"		// Место
			};

			// Добавление разметки столбцов в Header
			int lastResultColumnsIndex = 
				ColumnsAmount - NumberOfMainColumnsBehindResultColumns;

			for (int i = 0; i < SubColumnsText.Length; i++)
			{
				_header.ColumnDefinitions.Insert(
					lastResultColumnsIndex + i,
					new ColumnDefinition()
					{
						Style = 
							Application.Current
									   .MainWindow
									   .FindResource("SubColumnLayout") as Style
					});
			}

			// Сдвиг столбца "Место" в конец Header
			Grid.SetColumn(_ratingColumn, ColumnsAmount - 1);

			// Создание столбца для Header
			ContextMenu CreateColumnContextMenu()
			{
				var renameBinding = new CommandBinding
				{
					Command = TableCommands.RenameColumn
				};
				renameBinding.Executed += OnRenameColumnCommandExecuted;

				var removeBinding = new CommandBinding
				{
					Command = TableCommands.RemoveColumn
				};
				removeBinding.Executed += OnRemoveColumnCommandExecuted;

				var contextMenu = new ContextMenu()
				{
					Style =
						Application.Current
								   .MainWindow
								   .FindResource("ColumnHeaderContextMenu") as Style
				};

				contextMenu.CommandBindings.AddRange(new CommandBinding[]
				{
					renameBinding, removeBinding
				});

				contextMenu.Items.Add(new MenuItem()
				{
					Command = TableCommands.RenameColumn
				});

				contextMenu.Items.Add(new MenuItem()
				{
					Command = TableCommands.RemoveColumn
				});

				return contextMenu;
			}

			const int ResultColumnsHeaderRow = 0;
			const int ResultColumnsHeaderRowSpan = 3;
			Border resultColumnHeader = new Border()
			{
				Child = new TextBlock()
				{
					Style =
						Application.Current
								   .MainWindow
								   .FindResource("ResultColumnHeaderContent") as Style,
					Text = name
				},
				Style = 
					Application.Current
							   .MainWindow
							   .FindResource("ResultColumnHeader") as Style,
				ContextMenu = CreateColumnContextMenu()
			};

			resultColumnHeader.ContextMenu.PlacementTarget = resultColumnHeader;

			// Добавление столбца в Header
			Grid.SetColumn(resultColumnHeader, lastResultColumnsIndex);
			Grid.SetRow(resultColumnHeader, ResultColumnsHeaderRow);
			Grid.SetColumnSpan(resultColumnHeader, ResultColumnsHeaderRowSpan);
			_header.Children.Add(resultColumnHeader);

			// Создание подсталбцов для Header
			const int SubColumnRow = 1;
			Button subColumn;
			for (int i = 0; i < SubColumnsText.Length; i++)
			{
				subColumn = new Button()
				{
					Content = new TextBlock()
					{
						Text = SubColumnsText[i],
						Style = 
							Application.Current
									   .MainWindow
									   .FindResource("SubColumnContent") as Style
					},
					Style = 
						Application.Current
								   .MainWindow
								   .FindResource("SubColumn") as Style
				};

				// Добавление подсталбцов в Header
				Grid.SetColumn(subColumn, lastResultColumnsIndex + i);
				Grid.SetRow(subColumn, SubColumnRow);
				_header.Children.Add(subColumn);
			}

			// Добавление пустого результата в источник данных
			foreach (var student in Students)
			{
				student.Results.Add(new Student.Result());
			}

			// Добавление подсталбцов в DataGrid
			int lastResultColumnIndex = ResultColumnsAmount - 1;
			_body.Columns.Insert(
				lastResultColumnsIndex,
				new DataGridTextColumn()
				{
					Binding = new Binding($"Results[{lastResultColumnIndex}].Value")
					{
						Converter = new StringToIntConverter()
					},
					HeaderStyle = 
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubColumn") as Style,
					CellStyle = 
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubCell") as Style
				});

			_body.Columns.Insert(
				lastResultColumnsIndex + 1,
				new DataGridTextColumn()
				{
					Binding = new Binding($"Results[{lastResultColumnIndex}].Time")
					{
						StringFormat = "m:s",
						Converter = new StringToDateTimeConverter()
					},
					HeaderStyle = 
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubColumn") as Style,
					CellStyle = 
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubCell") as Style
				});

			_body.Columns.Insert(
				lastResultColumnsIndex + 2,
				new DataGridTextColumn()
				{
					Binding = new Binding($"Results[{lastResultColumnIndex}].Rating"),
					HeaderStyle =
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubColumn") as Style,
					CellStyle = 
						Application.Current
								   .MainWindow
								   .FindResource("DataGridSubCell") as Style,
					IsReadOnly = true
				});

			_body.Items.Refresh();
		}
		public void InitializeResultsAtNewRow()
		{
			for (int i = 0; i < ResultColumnsAmount; i++)
			{
				Students.Last().Results.Add(new Student.Result());
			}
		}
		public void MakeRating()
		{
			if (Students.Count == 0 || ResultColumnsAmount == 0)
			{
				return;
			}

			// Определение рейтинга для каждого результата
			List<Student> students = Students.ToList();
			for (int i = 0; i < Students[0].Results.Count; i++)
			{
				students.Sort((left, right) =>
				{
					bool isLeftNull = 
						left.Results[i].Value is null || left.Results[i].Time is null;
					bool isRightNull = 
						right.Results[i].Value is null || right.Results[i].Time is null;

					if (isLeftNull && isRightNull)
					{
						return 0;
					}
					if (!isLeftNull && isRightNull)
					{
						return -1;
					}
					if (isLeftNull && !isRightNull)
					{
						return 1;
					}

					double leftTime =
						  left.Results[i].Time.Value.Minute
						+ left.Results[i].Time.Value.Second / 60.0;
					double rightTime = 
						  right.Results[i].Time.Value.Minute
					    + right.Results[i].Time.Value.Minute / 60.0;

					double leftAvg = (double)(left.Results[i].Value / leftTime);
					double rightAvg = (double)(right.Results[i].Value / rightTime);

					 return leftAvg.CompareTo(rightAvg) * (-1);
				});

				for (int j = 0; j < students.Count; j++)
				{
					if (students[j].Results[i].Value is not null &&
						students[j].Results[i].Time is not null)
					{
						students[j].Results[i].Rating = j + 1;
					}
					else if (students[j].Results[i].Value is null ||
							 students[j].Results[i].Time is null)
					{
						students[j].Results[i].Rating = null;
					}
				}
			}

			// Определение общего рейтинга
			double leftAvg, rightAvg;
			bool isLeftNull, isRightNull;
			if (students.Count > 1)
			{
				students.Sort((leftStudent, rightStudent) =>
				{
					leftAvg = rightAvg = 0;
					isRightNull = isLeftNull = false;

					leftStudent.Results.ForEach(result =>
					{
						if (result.Rating is not null)
						{
							leftAvg += (double)result.Rating;
						}
						else
						{
							isLeftNull = true;
						}
					});

					leftAvg /= leftStudent.Results.Count;

					rightStudent.Results.ForEach(result =>
					{
						if (result.Rating is not null)
						{
							rightAvg += (double)result.Rating;
						}
						else
						{
							isRightNull = true;
						}
					});

					rightAvg /= rightStudent.Results.Count;

					if (isRightNull)
					{
						rightStudent.Rating = 0;
					}

					if (isLeftNull)
					{
						leftStudent.Rating = 0;
					}

					return leftAvg.CompareTo(rightAvg);
				});
			}
			else
			{
				isLeftNull = false;
				leftAvg = 0;
				var leftStudent = students[0];
				leftStudent.Results.ForEach(result =>
				{
					if (result.Rating != null)
					{
						leftAvg += (double)result.Rating;
					}
					else
					{
						isLeftNull = true;
					}
				});

				leftAvg /= leftStudent.Results.Count;

				if (isLeftNull)
				{
					leftStudent.Rating = 0;
				}
			}

			// Выставление общего рейтинга
			int rating = 0;
			for (int i = 0; i < students.Count; i++)
			{
				if (students[i].Rating == 0)
				{
					rating--;
				}

				students[i].Rating = students[i].Rating == 0 ? null : rating + i + 1;
			}

			Students = new ObservableCollection<Student>(students);
		}
		public void SerializeData()
		{
			var projectInfo = ProjectInfo.GetInstance;

			// Сериализация данных таблицы
			var xmlSerializer = new XmlSerializer(typeof(List<Student>));
			using (var fileStream =
				new FileStream(
					projectInfo.ProjectPath,
					FileMode.Create))
			{
				xmlSerializer.Serialize(fileStream, Students.ToList());
			}

			// Сериализация заголовков столбцов таблицы
			TextBlock? columnHeader;
			using (var streamWriter = new StreamWriter(projectInfo.ColumnsHeaderInfoPath))
			{
				for (int i = FirstResultColumnIndexInChildren;
					i < _header.Children.Count;
					i += ResultColumnSizeInChildren)
				{
					columnHeader = (_header.Children[i] as Border)?.Child as TextBlock;
					streamWriter.WriteLine(columnHeader.Text);
				}
			}

		}
		public void Paste(string clipbordData, PasteMode pasteMode)
		{
			if (clipbordData == string.Empty)
			{
				return;
			}

			// Преобразование данных Clipboard в табличный вид
			var rawRows = 
				clipbordData.Split(
					separator: new string[] { "\r\n" },
					StringSplitOptions.RemoveEmptyEntries);

			var rows = new List<List<string>>();
			foreach (string row in rawRows)
			{
				rows.Add(new List<string>());
				rows.Last()
					.AddRange(
						row.Split(
							separator: new string[] { "\t" },
							options: StringSplitOptions.None));
			}

			// Добавление данных в таблицу
			Student student;
			int resultValue;
			int notEmptyCellIndex;
			Column column;
			Column firstColumn;

			Column GetNextColumn()
			{
				return pasteMode switch
				{
					PasteMode.WithoutRating => 
						column is Column.Time ?
						Column.Value :
						column + 1,
					PasteMode.WithRating => 
						column is Column.Rating ?
						Column.Value :
						column + 1,
					_ => column,
				};
			}

			Column GetFirstColumn(List<string> row)
			{
				// Определяется индекс первой не пустой клетки
				notEmptyCellIndex = 0;
				while (notEmptyCellIndex < row.Count - 1 &&
					   row[notEmptyCellIndex] == string.Empty)
				{
					notEmptyCellIndex++;
				}

				string firstCell = row[notEmptyCellIndex];

				// Определяется тип клетки
				if (firstCell.Contains(' '))
				{
					return Column.Name;
				}
				if (int.TryParse(firstCell, out int intValue))
				{
					return Column.Value;
				}
				if (DateTime.TryParseExact(
						firstCell,
						"m:s",
						CultureInfo.InvariantCulture,
						DateTimeStyles.None,
						out var dateValue))
				{
					return Column.Time;
				}
				else
				{
					return Column.Group;
				}
			}

			foreach (var row in rows)
			{
				column = firstColumn = GetFirstColumn(row);
				student = new Student();

				for (int i = notEmptyCellIndex;
					 i < row.Count &&
					 i < GetColumnsForAddAmount(pasteMode)
						 + notEmptyCellIndex
						 - (int)firstColumn;
					 i++)
				{
					if (row[i] == string.Empty)
					{
						column = GetNextColumn();
						continue;
					}

					switch (column)
					{
						case Column.Name:
							student.Name = row[i];
							break;

						case Column.Group:
							student.Group = row[i];
							break;

						case Column.Value:
							student.Results.Add(new Student.Result());
							if (int.TryParse(row[i], out resultValue))
							{
								student.Results.Last().Value = resultValue;
							}
							break;

						case Column.Time:
							if (DateTime.TryParseExact(
									s: row[i],
									format: "m:s",
									provider: CultureInfo.InvariantCulture,
									style: DateTimeStyles.None,
									result: out var resultTime))
							{
								if(student.Results.Count == 0)
								{
									student.Results.Add(new Student.Result());
								}
								student.Results.Last().Time = resultTime;
							}
							break;

						case Column.Rating:
							if(int.TryParse(row[i], out resultValue))
							{
								student.Results.Last().Rating = resultValue;
							}
							break;
					}

					column = GetNextColumn();
				}

				while (student.Results.Count < ResultColumnsAmount)
				{
					student.Results.Add(new Student.Result());
				}

				Students.Add(student);
			}
		}

		private void OnRenameColumnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var columnName = 
				InputBox.Show("Введите название столбца:", "Изменение столбца");
			if (columnName is not null)
			{
				var contextMenu = sender as ContextMenu;
				var textBlocksBorder = contextMenu?.PlacementTarget as Border;
				var textBlock = textBlocksBorder?.Child as TextBlock;
				textBlock.Text = columnName;
			}
		}
		private void OnRemoveColumnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			void RemoveColumn(Border? columnsHeader)
			{
				const int FirstResultColumnsIndex = 3;
				const int SubColumnsAmount = 3;
				const int ResultColumnPartsAmount = SubColumnsAmount + 1; // + ResultColumnHeader
				int columnIndexInColumnDefenition = Grid.GetColumn(columnsHeader);
				int columnIndexInChildren = 
					  FirstResultColumnsIndex
					+ (columnIndexInColumnDefenition / SubColumnsAmount)
					* ResultColumnPartsAmount;

				// Удаление разметки
				_header.ColumnDefinitions.RemoveRange(
					columnIndexInColumnDefenition,
					SubColumnsAmount);
				_header.Children.RemoveRange(columnIndexInChildren, ResultColumnPartsAmount);

				// Сдвиг столбцов результата, находящимися после удаленного
				Button? subColumn;
				Border? column;
				int indexInColumnDefenition = 0;
				const int ResultColumnRowSpan = 3;

				for (int i = columnIndexInChildren;
					i <= _header.Children.Count - 1;
					i += ResultColumnPartsAmount)
				{
					column = _header.Children[i] as Border;
					Grid.SetColumn(
						column,
						columnIndexInColumnDefenition
							+ indexInColumnDefenition
							* SubColumnsAmount);
					Grid.SetColumnSpan(column, ResultColumnRowSpan);

					for (int j = 1; j <= SubColumnsAmount; j++)
					{
						subColumn = _header.Children[i + j] as Button;
						Grid.SetColumn(
							subColumn,
							columnIndexInColumnDefenition
								+ (indexInColumnDefenition * SubColumnsAmount) + j - 1);
					}

					indexInColumnDefenition++;
				}

				// Сдвиг столбца "Место"
				int lastColumnIndex = ColumnsAmount - 1;
				Grid.SetColumn(_ratingColumn, lastColumnIndex);

				// Удаление столбцов из DataGrid
				for (int i = 0; i < SubColumnsAmount; i++)
				{
					_body.Columns.RemoveAt(columnIndexInColumnDefenition);
				}

				// Перепревязка данных
				for (int i = columnIndexInColumnDefenition;
					i <= ColumnsAmount - ResultColumnPartsAmount;
					i += SubColumnsAmount)
				{
					((DataGridTextColumn)_body.Columns[i]).Binding =
						new Binding($"Results[{i / SubColumnsAmount}].Value")
						{
							Converter = new StringToIntConverter()
						};
					((DataGridTextColumn)_body.Columns[i + 1]).Binding =
						new Binding($"Results[{i / SubColumnsAmount}].Time")
						{
							Converter = new StringToDateTimeConverter(),
							StringFormat = "m:s"
						};
					((DataGridTextColumn)_body.Columns[i + 2]).Binding =
						new Binding($"Results[{i / SubColumnsAmount}].Rating");
				}

				// Удаление стоблцов из источника данных
				foreach (var student in Students)
				{
					student.Results.RemoveAt(columnIndexInColumnDefenition / SubColumnsAmount);
				}

				_body.Items.Refresh();
			}

			if (MessageBox.Show(
					"Удалить этот столбец?",
					"Внимание!",
					MessageBoxButton.YesNo,
					MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				var contextMenu = sender as ContextMenu;
				var columnsHeader = contextMenu?.PlacementTarget as Border;
				RemoveColumn(columnsHeader);
			}
		}

		private void AddDefaultColumns()
		{
			var columnNames = new string[] { "A", "B", "C", "D", "E" };
			foreach (var columnName in columnNames)
			{
				AddColumn(columnName);
			}
		}
		private int GetColumnsForAddAmount(PasteMode pasteMode)
		{
			var resultColumnsAmount = pasteMode switch
			{
				PasteMode.WithoutRating => 2,
				PasteMode.WithRating => 3,
				_ => throw new NotImplementedException(),
			};

			return 2 + resultColumnsAmount * ResultColumnsAmount;
		}
		private bool TryDeserializeData()
		{
			var projectInfo = ProjectInfo.GetInstance;

			if (!File.Exists(projectInfo.ProjectPath) || 
				new FileInfo(projectInfo.ProjectPath).Length == 0)
			{
				return false;
			}

			// Десериализация заголовков столбцов таблицы
			List<string?> columnHeaderTextCollection = new();

			using (var streamReader = new StreamReader(projectInfo.ColumnsHeaderInfoPath))
			{
				for (int i = FirstResultColumnIndexInChildren;
					 !streamReader.EndOfStream;
					 i += ResultColumnSizeInChildren)
				{
					columnHeaderTextCollection.Add(streamReader.ReadLine());
				}
			}

			// Десериализация данных таблицы
			List<Student>? students;

			using (var fileStream = 
				new FileStream(projectInfo.ProjectPath, FileMode.OpenOrCreate))
			{
				students = new XmlSerializer(typeof(List<Student>))
							   .Deserialize(fileStream) as List<Student>;
			}

			if (students is not null)
			{
				foreach (var columnHeaderText in columnHeaderTextCollection)
				{
					AddColumn(columnHeaderText);
				}

				Students = new ObservableCollection<Student>(students);
			}

			return true;
		}
		private void InitMainColumns()
		{
			// Добавление столбцов "ФИО", "Учебная группа" и "Место" в Header
			Button mainColumn = new();
			const int MainColumnsRow = 0;
			const int MainColumnsRowSpan = 2;

			const int FioColumnPosition = 0;
			const int StudyGroupColumnPosition = 1;
			const int RatingColumnPosition = 5;

			(string style, int position)[] columnsStyleAndPosition = new []
			{
				("MainColumnFio", FioColumnPosition),
				("MainColumnGroup", StudyGroupColumnPosition),
				("MainColumnRating", RatingColumnPosition )
			};

			void SetGrid(UIElement uIElement, int rowIndex, int columnIndex, int rowSpan)
			{
				Grid.SetRow(uIElement, rowIndex);
				Grid.SetColumn(uIElement, columnIndex);
				Grid.SetRowSpan(uIElement, rowSpan);
			}

            foreach (var columnStyleAndPosition in columnsStyleAndPosition)
            {
				mainColumn = new Button()
				{
					Style =
					Application.Current
							   .MainWindow
							   .FindResource(columnStyleAndPosition.style) as Style
				};

				SetGrid(
					mainColumn,
					MainColumnsRow,
					columnStyleAndPosition.position,
					MainColumnsRowSpan);

				_header.Children.Add(mainColumn);
			}

			_ratingColumn = mainColumn;

			(string title, string type, string cell, bool isReadOnly)[] columnsProperties =
				new[]
			{
				("Name", "FioColumn", "DataGridFioCell", false),
				("Group", "GroupColumn", "DataGridGroupCell", false),
				("Rating", "ResultColumn", "DataGridResultCell", true)
			};

            foreach (var columnProperties in columnsProperties)
            {
				_body.Columns.Add(new DataGridTextColumn()
				{
					Binding = new Binding(columnProperties.title),
					HeaderStyle =
						Application.Current
								   .MainWindow
								   .FindResource(columnProperties.type) as Style,
					CellStyle =
						Application.Current
								   .MainWindow
								   .FindResource(columnProperties.cell) as Style,
					IsReadOnly = columnProperties.isReadOnly
				});
			}
		}
	}
}