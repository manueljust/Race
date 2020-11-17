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
        // fixed for car
        public double Acceleration { get; set; } = 15;
        public double TurnRatio { get; set; } = 10;
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

        private string _statusText = "Initialized";
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        public Point GetTarget()
        {
            // on an ellipse with
            // origin at position + velocity
            // and rotation angle + targetangle

            Point target = Position + Velocity;
            target.X += TargetPower * (Acceleration * Math.Cos(TargetAngle) * Math.Cos(Angle) - TurnRatio * Math.Sin(TargetAngle) * Math.Sin(Angle));
            target.Y += TargetPower * (Acceleration * Math.Cos(TargetAngle) * Math.Sin(Angle) + TurnRatio * Math.Sin(TargetAngle) * Math.Cos(Angle));

            return target;
        }

        public void Move(double penalty)
        {
            Point target = GetTarget();
            Velocity = (target - Position) * penalty;

            if (0 != Velocity.LengthSquared)
            {
                Angle = Math.Atan2(Velocity.Y, Velocity.X);
            }

            Position += Velocity;

            StatusText = $"Position: {Position}, Velocity: {Velocity}, Angle: {Angle}";
        }
    }
}
