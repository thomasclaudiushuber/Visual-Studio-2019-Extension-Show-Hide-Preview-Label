using System.Windows;
using System.Windows.Media;

namespace VSIXShowHidePreviewLabel.Helpers
{
    public static class FrameworkElementExtensions
    {
        public static T GetChildElementByName<T>(this FrameworkElement parentElement, string elementNameToFind)
            where T : DependencyObject
        {
            T result = null;

            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i) as FrameworkElement;
                if (child != null)
                {
                    if (child.GetType().IsAssignableFrom(typeof(T)) && string.Equals(child.Name, elementNameToFind))
                    {
                        result = child as T;
                        break;
                    }

                    result = child.GetChildElementByName<T>(elementNameToFind);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
