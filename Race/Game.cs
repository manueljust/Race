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

        Dictionary<Car, List<Tuple<Path, Line>>> _trails = new Dictionary<Car, List<Tuple<Path, Line>>>();

        public Game()
        {
            Canvas = new Canvas();

            NewGame(@"Tracks\Track1.svg", new Color[] { Colors.Blue, Colors.Aquamarine }, RaceDirection.Clockwise);
        }

        public void NewGame(string filename, IEnumerable<Color> colors, RaceDirection direction)
        {
            Canvas.Children.Clear();
            Cars.Clear();

            try
            {
                Track = SvgHelper.Load(filename);

                double i = 1;
                foreach (Color color in colors)
                {
                    Car car = new Car() { Color = color };
                    car.Position = Track.StartPoint + (i / (colors.Count() + 1)) * Track.StartLine;
                    car.Angle = RaceDirection.Clockwise == direction ? -Math.Atan2(Track.StartLine.X, Track.StartLine.Y) : -Math.Atan2(-Track.StartLine.X, -Track.StartLine.Y);
                    car.PropertyChanged += Car_PropertyChanged;
                    _trails[car] = new List<Tuple<Path, Line>>();
                    Cars.Add(car);
                    i += 1;
                }

                ActiveCar = Cars[0];

                DrawTrack();

                DrawTargetEllipse();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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

            Path pos = new Path()
            {
                Data = Geometry.Parse("M 20.865234 45.246094 C 17.809455 45.246094 15.347656 47.705939 15.347656 50.761719 L 15.347656 109.03906 L 15.347656 167.31641 C 15.347656 170.37218 17.809455 172.83203 20.865234 172.83203 L 39.570312 172.83203 C 42.626091 172.83203 45.085937 170.37218 45.085938 167.31641 L 45.085938 121.99023 L 65.65625 121.99023 L 68.650391 132.05469 L 58.732422 132.05469 C 54.62259 132.05469 51.314453 135.36282 51.314453 139.47266 L 51.314453 160.58789 C 51.314453 164.69773 54.62259 168.00586 58.732422 168.00586 L 96.671875 168.00586 C 100.78171 168.00586 104.08984 164.69773 104.08984 160.58789 L 104.08984 139.47266 C 104.08984 135.36282 100.78171 132.05469 96.671875 132.05469 L 86.650391 132.05469 L 89.644531 121.99023 L 111.6582 121.99023 L 111.6582 157.73242 C 111.6582 160.7882 114.12 163.24805 117.17578 163.24805 L 149.64062 163.24805 C 152.69641 163.24805 154.13502 160.61251 155.15625 157.73242 L 167.83008 121.99023 L 183.4082 121.99023 L 185.97656 130.62305 L 174.10938 130.62305 C 170.655 130.62305 167.875 133.405 167.875 136.85938 L 167.875 151.24805 C 167.875 154.70242 170.655 157.48242 174.10938 157.48242 L 216.79688 157.48242 C 220.25122 157.48242 223.0332 154.70242 223.0332 151.24805 L 223.0332 136.85938 C 223.0332 133.405 220.25122 130.62305 216.79688 130.62305 L 204.82812 130.62305 L 207.39648 121.99023 L 229.58984 121.99023 L 229.58984 162.0293 C 229.58984 163.86225 230.25114 165.33789 231.07227 165.33789 L 239.79688 165.33789 C 240.61797 165.33789 241.00487 163.75686 241.2793 162.0293 L 247.73047 121.42383 C 253.08241 119.81094 256.9734 114.90458 257.07812 109.03906 C 256.97341 103.17354 253.08241 98.267179 247.73047 96.654297 L 241.2793 56.048828 C 241.00488 54.321267 240.61797 52.740234 239.79688 52.740234 L 231.07227 52.740234 C 230.25114 52.740234 229.58984 54.215878 229.58984 56.048828 L 229.58984 96.087891 L 207.39648 96.087891 L 204.82812 87.455078 L 216.79688 87.455078 C 220.25122 87.455078 223.0332 84.673123 223.0332 81.21875 L 223.0332 66.830078 C 223.0332 63.375705 220.25122 60.595703 216.79688 60.595703 L 174.10938 60.595703 C 170.655 60.595703 167.875 63.375705 167.875 66.830078 L 167.875 81.21875 C 167.875 84.673123 170.655 87.455078 174.10938 87.455078 L 185.97656 87.455078 L 183.4082 96.087891 L 167.83008 96.087891 L 155.15625 60.345703 C 154.13502 57.465616 152.6964 54.830078 149.64062 54.830078 L 117.17578 54.830078 C 114.12 54.830078 111.6582 57.289921 111.6582 60.345703 L 111.6582 96.087891 L 89.644531 96.087891 L 86.650391 86.023438 L 96.671875 86.023438 C 100.78171 86.023437 104.08984 82.715304 104.08984 78.605469 L 104.08984 57.490234 C 104.08984 53.380399 100.78171 50.072266 96.671875 50.072266 L 58.732422 50.072266 C 54.622587 50.072266 51.314453 53.380399 51.314453 57.490234 L 51.314453 78.605469 C 51.314453 82.715304 54.622587 86.023437 58.732422 86.023438 L 68.650391 86.023438 L 65.65625 96.087891 L 45.085938 96.087891 L 45.085938 50.761719 C 45.085938 47.705939 42.626092 45.246094 39.570312 45.246094 L 20.865234 45.246094 z M 137.74414 96.087891 A 28.085939 12.951169 0 0 1 165.83008 109.03906 A 28.085939 12.951169 0 0 1 137.74414 121.99023 A 28.085939 12.951169 0 0 1 121.74805 119.67969 L 121.74805 98.414062 A 28.085939 12.951169 0 0 1 137.74414 96.087891 z"),
                Fill = new SolidColorBrush(ActiveCar.Color),
                RenderTransform = new RotateTransform(180 * ActiveCar.Angle / Math.PI, 3, 1.5),
                Stretch = Stretch.Uniform,
                Stroke = Brushes.Black,
                StrokeThickness = 0.1,
                Height = 3,
                Width = 6,
            };
            _trails[ActiveCar].Add(new Tuple<Path, Line>(pos, _previewLine));

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

            foreach (Tuple<Path, Line> t in _trails[ActiveCar])
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
