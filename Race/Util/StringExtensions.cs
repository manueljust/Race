using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Race.Util
{
    public static class StringExtensions
    {
        private static string FailImageText { get; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAIAAABv85FHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABASURBVBhXY3gro/IfGwCKM0AoCB8OICIgOTgHAuBsqBwQQISQFSHkgABZAgiI0IcsBGeD5NCMAgKICG7/yagAAAxz0z5TKpbwAAAAAElFTkSuQmCC";

        public static Dictionary<string, string> ToDictionary(this string s, string elementDelimiter = ",", string keyValueDelimiter = ":")
        {
            return s.Split(new string[] { elementDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToDictionary(x => x.Substring(0, x.IndexOf(keyValueDelimiter)), x => x.Substring(x.IndexOf(keyValueDelimiter) + keyValueDelimiter.Length));
        }

        public static ImageSource GetImageSourceFromSvgLink(this string str)
        {
            if("" == str)
            {
                return FailImageText.GetImageSourceFromSvgLink();
            }

            try
            {
                // svg link is either link to file or embedded as base64

                // "data:image/png;base64,iVB..."
                // "bla.png"
                // "C:/bla.png"
                // "C:/dir_with,comma/bla.png"

                if(str.StartsWith("data:image/"))
                {
                    string[] metaData = str.Substring(0, str.IndexOf(',')).Split(':', '/', ';');
                    if ("base64" != metaData[3])
                    {
                        return FailImageText.GetImageSourceFromSvgLink();
                    }
                    MemoryStream ms = new MemoryStream(Convert.FromBase64String(str.Substring(str.IndexOf(',') + 1)));

                    switch (metaData[2].ToLower())
                    {
                        case "bmp":
                            return new BmpBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "png":
                            return new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "gif":
                            return new GifBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "jpg":
                        case "jpeg":
                            return new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "tif":
                        case "tiff":
                            return new TiffBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "wmp":
                            return new WmpBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case "ico":
                            return new IconBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        default:
                            return FailImageText.GetImageSourceFromSvgLink();
                    }
                }
                else
                {
                    switch (Path.GetExtension(str).ToLower())
                    {
                        case ".bmp":
                            return new BmpBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".png":
                            return new PngBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".gif":
                            return new GifBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".jpg":
                        case ".jpeg":
                            return new JpegBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".tif":
                        case ".tiff":
                            return new TiffBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".wmp":
                            return new WmpBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        case ".ico":
                            return new IconBitmapDecoder(new Uri(str), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
                        default:
                            return FailImageText.GetImageSourceFromSvgLink();
                    }
                }
            }
            catch
            {
                return FailImageText.GetImageSourceFromSvgLink();
            }
        }
    }
}
