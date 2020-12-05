using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Race.Util
{
    public static class Parser
    {
        public static Brush ParseBrush(string s, Dictionary<string, object> definitions = null)
        {
            try
            {
                if(s.StartsWith("url(#") && null != definitions)
                {
                    return (Brush)definitions[s.Substring(s.IndexOf("#") + 1).TrimEnd(')')];
                }
                return "none" == s.ToLower() ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
            }
            catch
            {
                return Brushes.Black;
            }
        }

        public static PenLineCap ParseLineCap(string s)
        {
            switch (s.ToLower())
            {
                case "round":
                    return PenLineCap.Round;
                case "square":
                    return PenLineCap.Square;
                default:
                    return PenLineCap.Flat;
            }
        }

        public static PenLineJoin ParseLineJoin(string s)
        {
            switch (s.ToLower())
            {
                case "bevel":
                    return PenLineJoin.Bevel;
                case "miter":
                    return PenLineJoin.Miter;
                default:
                    return PenLineJoin.Round;
            }
        }

        public static DoubleCollection ParseDashArray(string s)
        {
            return "none" == s.ToLower() ? null : new DoubleCollection(s.Split(',').Select(n => ParseDoubleIgnoreNonDigits(n)));
        }

        public static FontStyle ParseFontStyle(string s)
        {
            switch (s.ToLower())
            {
                case "italic":
                    return FontStyles.Italic;
                case "oblique":
                    return FontStyles.Oblique;
                default:
                    return FontStyles.Normal;
            }
        }

        public static FontWeight ParseFontWeight(string s)
        {
            switch (s.ToLower())
            {
                case "bold":
                    return FontWeights.Bold;
                case "bolder":
                    return FontWeights.ExtraBold;
                case "lighter":
                    return FontWeights.Light;
                default:
                    return FontWeights.Normal;
            }
        }

        public static double ParseDoubleIgnoreNonDigits(string s, double defaultValue = 0.0)
        {
            if(null != s && double.TryParse(string.Join("", s.Where(c => char.IsDigit(c) || '.' == c || '-' == c)), out double value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static GradientSpreadMethod ParseGradiendSpreadMethod(string value)
        {
            switch(value.ToLower())
            {
                case "reflect":
                    return GradientSpreadMethod.Reflect;
                case "repeat":
                    return GradientSpreadMethod.Repeat;
                default:
                    return GradientSpreadMethod.Pad;

            }
        }
    }
}
