using System;
using System.Globalization;
using System.Windows.Data;
using ICSharpCode.AvalonEdit.Document;

namespace ComponentDiffEditor.Converters
{
    public class StringToDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return new TextDocument(text);
            }
            return new TextDocument();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextDocument document)
            {
                return document.Text;
            }
            return string.Empty;
        }
    }

    public class SimilarityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double similarity)
            {
                if (similarity >= 95) return "Green";
                if (similarity >= 80) return "Orange";
                return "Red";
            }
            return "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}