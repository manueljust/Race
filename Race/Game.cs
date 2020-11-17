using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Race
{
    public class Game : PropertyChangedAware
    {
        private Canvas _canvas;
        public Canvas Canvas
        {
            get { return _canvas; }
            set
            {
                if (SetProperty(ref _canvas, value))
                {
                    _canvas.MouseDown += Canvas_MouseDown;
                    _canvas.MouseMove += Canvas_MouseMove;
                }
            }
        }

        public List<Car> Cars = new List<Car>() { new Car() { Color = Colors.Blue }, new Car() { Color = Colors.Brown } };

        private Car _activeCar;
        public Car ActiveCar
        {
            get { return _activeCar; }
            set { SetProperty(ref _activeCar, value); }
        }
        private Track Track { get; set; }

        Dictionary<Car, List<Tuple<Ellipse, Line>>> _trails = new Dictionary<Car, List<Tuple<Ellipse, Line>>>();

        public Game()
        {
            Canvas = new Canvas();

            try
            {
                Track = Track.FromSvg(@"Tracks\Track1.svg");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            double i = 1;
            foreach (Car c in Cars)
            {
                c.Position = Track.StartPoint + (i / (Cars.Count + 1)) * Track.StartLine;
                c.Angle = -Math.Atan2(Track.StartLine.X, Track.StartLine.Y);
                c.PropertyChanged += Car_PropertyChanged;
                _trails[c] = new List<Tuple<Ellipse, Line>>();
                i += 1;
            }

            ActiveCar = Cars[0];

            DrawTrack();

            DrawTargetEllipse();
        }

        private void DrawTrack()
        {
            Canvas.Children.Clear();

            Canvas.Height = Track.Height;
            Canvas.Width = Track.Width;
            Canvas.Background = Track.Background;
            Canvas.Clip = new RectangleGeometry(new Rect(0, 0, Track.Width, Track.Height));

            Canvas.Children.Add(Track.Bounds);
            Canvas.Children.Add(Track.Start);
            Canvas.Children.Add(Track.Goal);
            foreach (Path p in Track.Decorations)
            {
                Canvas.Children.Add(p);
            }
            foreach (Path p in Track.Obstacles)
            {
                Canvas.Children.Add(p);
            }

            Canvas.Children.Add(_targetEllipse);
            Canvas.Children.Add(_centerLine);
            Canvas.Children.Add(_steeringLine);
        }

        private void Car_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ActiveCar.TargetAngle):
                case nameof(ActiveCar.TargetPower):
                    Point target = ActiveCar.GetTarget();
                    _previewLine.X2 = target.X;
                    _previewLine.Y2 = target.Y;
                    _steeringLine.X2 = target.X;
                    _steeringLine.Y2 = target.Y;
                    break;
            }
        }

        private Line _previewLine = new Line();
        private Line _centerLine = new Line() { StrokeThickness = 0.8, Stroke = Brushes.Brown, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7 };
        private Line _steeringLine = new Line() { StrokeThickness = 0.8, Stroke = Brushes.Red, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7 };
        private Ellipse _targetEllipse = new Ellipse() { StrokeThickness = 1, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#330088FF")) };

        private void DrawTargetEllipse()
        {
            Point target = ActiveCar.GetTarget();
            Point center = ActiveCar.Position + ActiveCar.Velocity;

            _previewLine = new Line()
            {
                StrokeThickness = 1.2,
                Stroke = new SolidColorBrush(ActiveCar.Color),
                X1 = ActiveCar.Position.X,
                Y1 = ActiveCar.Position.Y,
                X2 = target.X,
                Y2 = target.Y,
                StrokeStartLineCap = PenLineCap.Triangle,
            };
            _centerLine.Stroke = new SolidColorBrush(ActiveCar.Color);
            _centerLine.X1 = ActiveCar.Position.X;
            _centerLine.Y1 = ActiveCar.Position.Y;
            _centerLine.X2 = center.X;
            _centerLine.Y2 = center.Y;

            _steeringLine.Stroke = new SolidColorBrush(ActiveCar.Color);
            _steeringLine.X1 = center.X;
            _steeringLine.Y1 = center.Y;
            _steeringLine.X2 = target.X;
            _steeringLine.Y2 = target.Y;

            Canvas.Children.Add(_previewLine);

            Canvas.SetLeft(_targetEllipse, center.X - ActiveCar.Acceleration);
            Canvas.SetTop(_targetEllipse, center.Y - ActiveCar.TurnRatio);
            _targetEllipse.Height = 2 * ActiveCar.TurnRatio;
            _targetEllipse.Width = 2 * ActiveCar.Acceleration;
            _targetEllipse.RenderTransform = new RotateTransform(180 * ActiveCar.Angle / Math.PI, ActiveCar.Acceleration, ActiveCar.TurnRatio);

            Ellipse pos = new Ellipse()
            {
                Fill = new SolidColorBrush(ActiveCar.Color),
                Height = 3,
                Width = 3,
            };
            _trails[ActiveCar].Add(new Tuple<Ellipse, Line>(pos, _previewLine));

            Canvas.Children.Add(pos);
            Canvas.SetLeft(pos, ActiveCar.Position.X - pos.Width / 2);
            Canvas.SetTop(pos, ActiveCar.Position.Y - pos.Height / 2);
        }

        private int _winCount = 0;
        private bool IsWin()
        {
            IntersectionDetail win = Track.Goal.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

            if (IntersectionDetail.Empty != win)
            {
                _winCount++;
                return _winCount > Cars.Count;
            }

            return false;
        }

        private bool IsCrash()
        {
            IntersectionDetail onTrack = Track.Bounds.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

            if (onTrack != IntersectionDetail.FullyContains)
            {
                return true;
            }

            foreach (Path p in Track.Obstacles)
            {
                IntersectionDetail collision = p.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

                if (collision != IntersectionDetail.Empty)
                {
                    return true;
                }
            }

            return false;
        }

        int CarIndex = 0;

        public void Move()
        {
            _previewLine.Stroke = new SolidColorBrush(ActiveCar.Color);

            double penalty = 1;
            if (IsCrash())
            {
                _previewLine.Stroke = Brushes.Red;
                _previewLine.StrokeDashArray = new DoubleCollection(new double[] { 1.2, 0.8 });
                penalty = 0.5;
            }

            foreach (Tuple<Ellipse, Line> t in _trails[ActiveCar])
            {
                t.Item1.Opacity *= 0.6;
                t.Item2.Opacity *= 0.6;
            }

            ActiveCar.Move(penalty);

            if (IsWin())
            {
                MessageBox.Show("Winner!");
            }

            ActiveCar = Cars[++CarIndex % Cars.Count];

            DrawTargetEllipse();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetAngleAndPower(e.GetPosition(Canvas));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                SetAngleAndPower(e.GetPosition(Canvas));
            }
        }

        private void SetAngleAndPower(Point clicked)
        {
            Vector request = clicked - ActiveCar.Position - ActiveCar.Velocity;
            ActiveCar.TargetAngle = Math.Atan2(request.Y, request.X) - ActiveCar.Angle;
            double maxPower = ActiveCar.Acceleration * ActiveCar.TurnRatio / Math.Sqrt(Math.Pow(ActiveCar.Acceleration * Math.Sin(ActiveCar.TargetAngle), 2) + Math.Pow(ActiveCar.TurnRatio * Math.Cos(ActiveCar.TargetAngle), 2));
            ActiveCar.TargetPower = Math.Min(1, request.Length / maxPower);
        }
    }
}
