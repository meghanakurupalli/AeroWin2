using System.Linq;
using System.Windows;

namespace MainWindowDesign
{
    public static class Helper
    {
        public static bool IsWindowVisible<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name) && w.Visibility == Visibility.Visible);
        }
    }
}
