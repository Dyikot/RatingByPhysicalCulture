using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RatingByPhysicalCulture.Converters
{
    public class StringToIntConverter : IValueConverter
	{
		public object Convert(
			object value,
			Type targetType,
			object parameter,
			CultureInfo culture)
		{
			if (value is not null)
			{
				int intValue = (int)value;
				string strValue = intValue.ToString();
				return strValue;
			}
			else
			{
				return 0;
			}
		}

		public object ConvertBack(
			object value,
			Type targetType,
			object parameter,
			CultureInfo culture)
		{
			var strValue = value as string;
			if (int.TryParse(strValue, out int intValue))
			{
				return intValue;
			}
			else
			{
				return 0;
			}
		}
	}
}
