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
        public static IEnumerable<Player> DefaultPlayers { get; } = new Player[]
        {
            new Player() { Name = "Olaf", Color = Colors.Teal },
            new Player() { Name = "Sina", Color = Colors.Red },
            new Player() { Name = "Nero", Color = Colors.Black },
        };

        private readonly Random _random = new Random();

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

        private List<Car> _cars = new List<Car>() { new Car() { Color = Colors.Blue }, new Car() { Color = Colors.Brown } };

        private Car _activeCar;
        public Car ActiveCar
        {
            get { return _activeCar; }
            set { SetProperty(ref _activeCar, value); }
        }

        private Track Track { get; set; }

        Dictionary<Car, List<Shape>> _trails = new Dictionary<Car, List<Shape>>();

        public Game()
        {
            Canvas = new Canvas();

            NewGame(@"Tracks\Track1.svg", DefaultPlayers, 0 == _random.Next() % 2 ? RaceDirection.Clockwise : RaceDirection.Counterclockwise);
        }

        public void NewGame(string filename, IEnumerable<Player> players, RaceDirection direction)
        {
            Canvas.Children.Clear();
            _cars.Clear();

            try
            {
                Track = SvgHelper.Load(filename);

                double i = 1;
                foreach (Player player in players.ToDictionary(p => _random.Next()).OrderBy(p => p.Key).Select(p => p.Value))
                {
                    Car car = new Car() { Color = player.Color, Driver = player.Name };
                    car.Position = Track.StartPoint + (i / (players.Count() + 1)) * Track.StartLine;
                    car.Angle = RaceDirection.Clockwise == direction ? -Math.Atan2(Track.StartLine.X, Track.StartLine.Y) : -Math.Atan2(-Track.StartLine.X, -Track.StartLine.Y);
                    car.Acceleration = player.Handicap * Math.Sqrt(player.Ratio * 250 / (1.0 - player.Ratio));
                    car.TurnRatio = player.Handicap * Math.Sqrt(250 * (1.0 - player.Ratio) / player.Ratio);
                    car.PropertyChanged += Car_PropertyChanged;
                    _trails[car] = new List<Shape>();
                    _cars.Add(car);
                    i += 1;
                }

                ActiveCar = _cars[0];

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

            foreach(Car car in _cars)
            {
                DrawCar(car);
            }
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

        private void DrawCar(Car car)
        {
            Path path = new Path()
            {
                Data = car.Geometry,
                Fill = new SolidColorBrush(car.Color),
                RenderTransform = new RotateTransform(180 * car.Angle / Math.PI, 3, 1.5),
                Stretch = Stretch.Uniform,
                Stroke = Brushes.Black,
                StrokeThickness = 0.1,
                Height = 3,
                Width = 6,
                ToolTip = car.Driver,
            };
            _trails[car].ForEach(s => s.ToolTip = null);
            _trails[car].Add(path);

            Canvas.Children.Add(path);
            Canvas.SetLeft(path, car.Position.X - path.Width / 2);
            Canvas.SetTop(path, car.Position.Y - path.Height / 2);
        }

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

            _trails[ActiveCar].Add(_previewLine);
        }

        private int _winCount = 0;
        private bool IsWin()
        {
            IntersectionDetail win = Track.Goal.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

            if (IntersectionDetail.Empty != win)
            {
                _winCount++;
                return _winCount > _cars.Count;
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

            foreach (Shape s in _trails[ActiveCar])
            {
                s.Opacity *= 0.9;
            }

            ActiveCar.Move(penalty);

            if (IsWin())
            {
                MessageBox.Show("Winner!");
            }

            DrawCar(ActiveCar);

            ActiveCar = _cars[++CarIndex % _cars.Count];

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
            // ellipse as bezier:
            // https://math.stackexchange.com/questions/11698/how-elliptic-arc-can-be-represented-by-cubic-b%C3%A9zier-curve
            // http://www.spaceroots.org/documents/ellipse/node22.html

            Vector request = clicked - (ActiveCar.Position + ActiveCar.Velocity);
            // correct that angle...
            ActiveCar.TargetAngle = Math.Atan2(request.Y, request.X) - ActiveCar.Angle;
            double maxPower = ActiveCar.Acceleration * ActiveCar.TurnRatio / Math.Sqrt(Math.Pow(ActiveCar.Acceleration * Math.Sin(ActiveCar.TargetAngle), 2) + Math.Pow(ActiveCar.TurnRatio * Math.Cos(ActiveCar.TargetAngle), 2));
            ActiveCar.TargetPower = Math.Min(1, request.Length / maxPower);
        }
    }
}
