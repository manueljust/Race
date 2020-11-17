using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Race
{
    public static class Helper
    {
        public static Brush ParseBrush(string s)
        {
            try
            {
                return "none" == s.ToLower() ? null : new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
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

        public static double ParseDoubleIgnoreNonDigits(string s)
        {
            try
            {
                return double.Parse(string.Join("", s.Where(c => char.IsDigit(c) || '.' == c)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                return 0;
            }
        }

        public static double GetDouble(this XElement element, string name, double defaultValue = 0)
        {
            XAttribute att = element.Attribute(name);
            if (null != att)
            {
                return double.Parse(att.Value);
            }
            else
            {
                return defaultValue;
            }
        }

        public static Brush GetBrush(this XElement element, string name)
        {
            XAttribute att = element.Attribute(name);
            if (null != att)
            {
                return ParseBrush(att.Value);
            }
            else
            {
                return null;
            }
        }

        public static Geometry GetTextGeometry(this XElement element)
        {
            Dictionary<string, string> style = element.GetStyle();

            FormattedText text = new FormattedText(
                element.Value,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(style["font-family"]),
                    ParseFontStyle(style["font-style"]),
                    ParseFontWeight(style["font-weight"]),
                    FontStretches.Normal),
                ParseDoubleIgnoreNonDigits(style["font-size"]),
                ParseBrush(style["fill"])
                );

            return text.BuildGeometry(new Point(element.GetDouble("x"), element.GetDouble("y")));
        }

        public static string GetDescription(this XElement element)
        {
            foreach(XElement child in element.Elements())
            {
                if(child.Name.LocalName == "desc")
                {
                    return child.Value;
                }
            }
            return null;
        }

        public static Dictionary<string, string> GetStyle(this XElement element)
        {
            Dictionary<string, string> style = new Dictionary<string, string>();

            XAttribute st = element.Attribute("style");
            if(null != st)
            {
                string[] styles = st.Value.Split(';');
                foreach(string s in styles)
                {
                    string[] kv = s.Split(':');
                    if(2 == kv.Length)
                    {
                        style[kv[0]] = kv[1];
                    }
                }
            }

            return style;
        }

        public static void ApplyStyle(this Path path, Dictionary<string, string> style)
        {
            foreach (KeyValuePair<string, string> pair in style)
            {
                switch (pair.Key.ToLower())
                {
                    case "fill":
                        path.Fill = ParseBrush(pair.Value);
                        break;
                    case "stroke":
                        path.Stroke = ParseBrush(pair.Value);
                        break;
                    case "stroke-width":
                        path.StrokeThickness = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-linecap":
                        path.StrokeStartLineCap = ParseLineCap(pair.Value);
                        path.StrokeEndLineCap = ParseLineCap(pair.Value);
                        break;
                    case "stroke-linejoin":
                        path.StrokeLineJoin = ParseLineJoin(pair.Value);
                        break;
                    case "stroke-miterlimit":
                        path.StrokeMiterLimit = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "stroke-dasharray":
                        path.StrokeDashArray = ParseDashArray(pair.Value);
                        break;
                    case "stroke-dashoffset":
                        path.StrokeDashOffset = ParseDoubleIgnoreNonDigits(pair.Value);
                        break;
                    case "visibility":
                        switch (pair.Value)
                        {
                            case "hidden":
                                path.Visibility = Visibility.Hidden;
                                break;
                            case "collapsed":
                                path.Visibility = Visibility.Collapsed;
                                break;
                        }
                        break;
                    case "opacity":
                        path.Opacity = double.Parse(pair.Value);
                        break;
                }
            }
        }
    }
}
