using RatingByPhysicalCulture.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace RatingByPhysicalCulture.Commands
{
	public static class TableCommands
	{
		public static readonly RoutedUICommand AddColumn =
			new (
				text: "Добавить Столбец",
				name: "AddColumn",
				ownerType: typeof(TableCommands),
				inputGestures: new InputGestureCollection()
				{
					new KeyGesture(Key.A, ModifierKeys.Alt)
				});

		public static readonly RoutedUICommand RenameColumn =
			new (
				text: "Переименовать",
				name: "RenameColumn",
				ownerType: typeof(TableCommands));

		public static readonly RoutedUICommand RemoveColumn =
			new (
				text: "Удалить",
				name: "RemoveColumn",
				ownerType: typeof(TableCommands));

		public static readonly RoutedUICommand MakeRating =
			new (
				text: "Составить",
				name: "MakeRating",
				ownerType: typeof(TableCommands),
				inputGestures: new InputGestureCollection()
				{
					new KeyGesture(Key.F5)
				});
	}
}
