using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Race
{
    public class Player : PropertyChangedAware
    {
        private string _name = "Name";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private Color _color = Colors.Red;
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }


        public static Color[] PredefinedColors { get; } = new Color[]
        {
            Colors.Red,
            Colors.Teal,
            Colors.Blue,
            Colors.Orange,
            Colors.Olive,
            Colors.Chartreuse,
        };
    }
}
