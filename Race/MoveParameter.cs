using Race.Util;
using System.Collections.Generic;

namespace Race
{
    public class MoveParameter
    {
        public MoveParameter(double targetAngle, double targetPower)
        {
            Angle = targetAngle;
            Power = targetPower;
        }

        public double Angle { get; set; } = 0.0;
        public double Power { get; set; } = 1.0;

        public string GetStringRepresentation()
        {
            return $"angle:{Angle},power:{Power}";
        }

        public static MoveParameter FromString(string s)
        {
            Dictionary<string, string> dict = s.ToDictionary();
            return new MoveParameter(double.Parse(dict["angle"]), double.Parse(dict["power"]));
        }
    }
}
