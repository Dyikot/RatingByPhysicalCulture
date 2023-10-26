using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RatingByPhysicalCulture.Converters
{
	public class StringToDateTimeConverter : IValueConverter
	{
		public object? Convert(
			object value,
			Type targetType,
			object parameter,
			CultureInfo culture)
		{
			if(value is not null)
			{
				DateTime date = (DateTime)value;
				string dateString = date.ToString("mm:ss");
				return dateString;
			}
			else
			{
				return null;
			}
		}

		public object? ConvertBack(
			object value,
			Type targetType,
			object parameter,
			CultureInfo culture)
		{
			string? strValue = value as string;
			if (DateTime.TryParseExact(
					s: strValue,
					format:"m:s",
					provider: CultureInfo.InvariantCulture,
					style: DateTimeStyles.None,
					result: out DateTime dateValue
					))
			{
				return dateValue;
			}
			else
			{
				return null;
			}
		}
	}
}
