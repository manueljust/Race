using Race.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Race
{
    public class Car : PropertyChangedAware
    {
        public static Car[] DefaultCars { get; } = new Car[]
        {
            new Car() { Driver = "Olaf", Color = Colors.Teal },
            new Car() { Driver = "Sina", Color = Colors.Red, PowerShape = new PowerShape() { Edgyness = 1 } },
            new Car() { Driver = "Nero", Color = Colors.Black, PowerShape = new PowerShape() { Acceleration = 1, Deceleration = 0.6, TurnRatio = 0.5, Edgyness = 0.2 } },
        };

        public static Color[] PredefinedColors { get; } = new Color[]
        {
            Colors.Red,
            Colors.Teal,
            Colors.Blue,
            Colors.Orange,
            Colors.Olive,
            Colors.Chartreuse,
        };

        public Geometry Geometry { get; set; } = Geometry.Parse("m 0,1 L1,0 L0,-1 z");
        //public Geometry Geometry { get; set; } = Geometry.Parse("M 20.865234 45.246094 C 17.809455 45.246094 15.347656 47.705939 15.347656 50.761719 L 15.347656 109.03906 L 15.347656 167.31641 C 15.347656 170.37218 17.809455 172.83203 20.865234 172.83203 L 39.570312 172.83203 C 42.626091 172.83203 45.085937 170.37218 45.085938 167.31641 L 45.085938 121.99023 L 65.65625 121.99023 L 68.650391 132.05469 L 58.732422 132.05469 C 54.62259 132.05469 51.314453 135.36282 51.314453 139.47266 L 51.314453 160.58789 C 51.314453 164.69773 54.62259 168.00586 58.732422 168.00586 L 96.671875 168.00586 C 100.78171 168.00586 104.08984 164.69773 104.08984 160.58789 L 104.08984 139.47266 C 104.08984 135.36282 100.78171 132.05469 96.671875 132.05469 L 86.650391 132.05469 L 89.644531 121.99023 L 111.6582 121.99023 L 111.6582 157.73242 C 111.6582 160.7882 114.12 163.24805 117.17578 163.24805 L 149.64062 163.24805 C 152.69641 163.24805 154.13502 160.61251 155.15625 157.73242 L 167.83008 121.99023 L 183.4082 121.99023 L 185.97656 130.62305 L 174.10938 130.62305 C 170.655 130.62305 167.875 133.405 167.875 136.85938 L 167.875 151.24805 C 167.875 154.70242 170.655 157.48242 174.10938 157.48242 L 216.79688 157.48242 C 220.25122 157.48242 223.0332 154.70242 223.0332 151.24805 L 223.0332 136.85938 C 223.0332 133.405 220.25122 130.62305 216.79688 130.62305 L 204.82812 130.62305 L 207.39648 121.99023 L 229.58984 121.99023 L 229.58984 162.0293 C 229.58984 163.86225 230.25114 165.33789 231.07227 165.33789 L 239.79688 165.33789 C 240.61797 165.33789 241.00487 163.75686 241.2793 162.0293 L 247.73047 121.42383 C 253.08241 119.81094 256.9734 114.90458 257.07812 109.03906 C 256.97341 103.17354 253.08241 98.267179 247.73047 96.654297 L 241.2793 56.048828 C 241.00488 54.321267 240.61797 52.740234 239.79688 52.740234 L 231.07227 52.740234 C 230.25114 52.740234 229.58984 54.215878 229.58984 56.048828 L 229.58984 96.087891 L 207.39648 96.087891 L 204.82812 87.455078 L 216.79688 87.455078 C 220.25122 87.455078 223.0332 84.673123 223.0332 81.21875 L 223.0332 66.830078 C 223.0332 63.375705 220.25122 60.595703 216.79688 60.595703 L 174.10938 60.595703 C 170.655 60.595703 167.875 63.375705 167.875 66.830078 L 167.875 81.21875 C 167.875 84.673123 170.655 87.455078 174.10938 87.455078 L 185.97656 87.455078 L 183.4082 96.087891 L 167.83008 96.087891 L 155.15625 60.345703 C 154.13502 57.465616 152.6964 54.830078 149.64062 54.830078 L 117.17578 54.830078 C 114.12 54.830078 111.6582 57.289921 111.6582 60.345703 L 111.6582 96.087891 L 89.644531 96.087891 L 86.650391 86.023438 L 96.671875 86.023438 C 100.78171 86.023437 104.08984 82.715304 104.08984 78.605469 L 104.08984 57.490234 C 104.08984 53.380399 100.78171 50.072266 96.671875 50.072266 L 58.732422 50.072266 C 54.622587 50.072266 51.314453 53.380399 51.314453 57.490234 L 51.314453 78.605469 C 51.314453 82.715304 54.622587 86.023437 58.732422 86.023438 L 68.650391 86.023438 L 65.65625 96.087891 L 45.085938 96.087891 L 45.085938 50.761719 C 45.085938 47.705939 42.626092 45.246094 39.570312 45.246094 L 20.865234 45.246094 z M 137.74414 96.087891 A 28.085939 12.951169 0 0 1 165.83008 109.03906 A 28.085939 12.951169 0 0 1 137.74414 121.99023 A 28.085939 12.951169 0 0 1 121.74805 119.67969 L 121.74805 98.414062 A 28.085939 12.951169 0 0 1 137.74414 96.087891 z");

        // fixed for car
        public PowerShape PowerShape { get; private set; } = new PowerShape();
        public Color Color { get; set; } = Colors.Red;

        // car state
        public Point Position { get; set; } = new Point(0, 0);
        public Vector Velocity { get; set; } = new Vector(0, 0);
        public double Angle { get; set; } = 0;

        // influencable variables
        private double _targetAngle = 0;
        public double TargetAngle
        {
            get { return _targetAngle; }
            set { SetProperty(ref _targetAngle, value); }
        }

        private double _targetPower = 1;
        public double TargetPower
        {
            get { return _targetPower; }
            set { SetProperty(ref _targetPower, value); }
        }

        private string _driver = "None";
        public string Driver
        {
            get { return _driver; }
            set { SetProperty(ref _driver, value); }
        }

        private PlayerType _playerType = PlayerType.Human;
        public PlayerType PlayerType
        {
            get { return _playerType; }
            set { SetProperty(ref _playerType, value); }
        }

        private NetworkConnector _networkConnector = null;
        public NetworkConnector NetworkConnector
        {
            get { return _networkConnector; }
            set { SetProperty(ref _networkConnector, value); }
        }

        public string GetStringRepresentation()
        {
            return $"driver:{Driver},color:{Color},ps.acc:{PowerShape.Acceleration},ps.dec:{PowerShape.Deceleration},ps.tr:{PowerShape.Deceleration},ps.edg:{PowerShape.Edgyness},ps.ar:{PowerShape.Area}";
        }

        public static Car FromString(string s)
        {
            try
            {
                Dictionary<string, string> info = s.ToDictionary();
                Car car = new Car
                {
                    Driver = info["driver"],
                    Color = (Color)ColorConverter.ConvertFromString(info["color"]),
                    PlayerType = PlayerType.Online,
                };
                car.PowerShape.Acceleration = double.Parse(info["ps.acc"]);
                car.PowerShape.Deceleration = double.Parse(info["ps.dec"]);
                car.PowerShape.TurnRatio = double.Parse(info["ps.tr"]);
                car.PowerShape.Edgyness = double.Parse(info["ps.edg"]);
                car.PowerShape.Area = double.Parse(info["ps.ar"]);
                return car;
            }
            catch
            {
                return null;
            }
        }

        public async Task WaitForMove()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            TargetAngle = 0.0;
            TargetPower = 1.0;
        }

        private string _statusText = "Initialized";
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        public Point GetTarget()
        {
            Point target = Position + Velocity;
            double targetRadius = TargetPower * PowerShape.GetRadius(TargetAngle);
            target.X += targetRadius * Math.Cos(TargetAngle + Angle);
            target.Y += targetRadius * Math.Sin(TargetAngle + Angle);

            return target;
        }

        public void Move(double crashLength)
        {
            Point target = GetTarget();

            Velocity = (target - Position);
            double penalty = 1;
            if(0 != crashLength)
            {
                penalty = Math.Pow(crashLength, 3) / (PowerShape.Area + Math.Pow(crashLength, 3));
                Velocity *= (crashLength / Velocity.Length);
            }

            if (0 != Velocity.LengthSquared)
            {
                Angle = Math.Atan2(Velocity.Y, Velocity.X);
            }

            Position += Velocity;

            if(1.0 > penalty)
            {
                Velocity = new Vector(0, 0);
                PowerShape.Area *= penalty;
            }

            StatusText = $"Position: {Position}, Velocity: {Velocity}, Angle: {Angle}";
        }
    }
}
