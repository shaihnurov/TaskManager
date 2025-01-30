using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskManager.Services
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "В процессе" => new SolidColorBrush(Colors.Red),
                    "Завершено" => new SolidColorBrush(Colors.Green),
                    _ => new SolidColorBrush(Colors.DarkRed),
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
