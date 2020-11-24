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
        public Car Car { get; } = new Car();

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
