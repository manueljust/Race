using Race.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private static readonly Random _random = new Random();
        private static readonly double _lineThickness = 0.8;

        private Line _previewLine = new Line();
        private Line _centerLine = new Line() { StrokeThickness = _lineThickness, Stroke = Brushes.Brown, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7, StrokeEndLineCap = PenLineCap.Round, StrokeStartLineCap = PenLineCap.Round };
        private Line _steeringLine = new Line() { StrokeThickness = _lineThickness, Stroke = Brushes.Red, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7, StrokeEndLineCap = PenLineCap.Round, StrokeStartLineCap = PenLineCap.Round };
        private PowerShape _targetPowerShape = new PowerShape() { StrokeThickness = 1, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#330088FF")) };

        private Track _track = new Track();
        private List<Car> _cars = new List<Car>();
        private Dictionary<Car, List<Shape>> _trails = new Dictionary<Car, List<Shape>>();


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


        private Car _activeCar;
        public Car ActiveCar
        {
            get { return _activeCar; }
            set
            {
                if(SetProperty(ref _activeCar, value))
                {
                    _targetPowerShape.MorphInto(ActiveCar.PowerShape);
                }
            }
        }

        public Game()
        {
            Canvas = new Canvas();

            NewGame(@"Tracks\Track1.svg", Car.DefaultCars, 0 == _random.Next() % 2 ? RaceDirection.Clockwise : RaceDirection.Counterclockwise, "local");
        }

        public async Task NewGame(string filename, IEnumerable<Car> cars, RaceDirection direction, string annotation)
        {
            Canvas.Children.Clear();
            _cars.Clear();
            _trails.Clear();

            try
            {
                _track = SvgHelper.Load(filename);

                double i = 0.5;
                //foreach (Car car in cars.ToDictionary(p => _random.Next()).OrderBy(p => p.Key).Select(p => p.Value))
                foreach (Car car in cars)
                {
                    car.Position = _track.Start.StartPoint() + (i / cars.Count()) * _track.Start.Vector();
                    car.Angle = RaceDirection.Clockwise == direction ? -Math.Atan2(_track.Start.Vector().X, _track.Start.Vector().Y) : -Math.Atan2(-_track.Start.Vector().X, -_track.Start.Vector().Y);
                    car.PowerShape.Area = 100.0;
                    car.Velocity = new Vector();
                    car.PropertyChanged += Car_PropertyChanged;
                    _trails[car] = new List<Shape>();
                    _cars.Add(car);
                    i += 1;
                }


                DrawTrack(annotation);

                ActiveCar = _cars[0];

                await WaitForNextMove();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DrawTrack(string annotation)
        {
            Canvas.Children.Clear();

            Canvas.Height = _track.Height;
            Canvas.Width = _track.Width;
            Canvas.Background = _track.Background;
            Canvas.Clip = new RectangleGeometry(new Rect(0, 0, _track.Width, _track.Height));

            Canvas.Children.Add(_track.Bounds);
            Canvas.Children.Add(_track.Start);
            Canvas.Children.Add(_track.Goal);
            foreach (Shape s in _track.Decorations)
            {
                Canvas.Children.Add(s);
            }
            foreach (Shape s in _track.Obstacles)
            {
                Canvas.Children.Add(s);
            }

            Canvas.Children.Add(_targetPowerShape);
            Canvas.Children.Add(_centerLine);
            Canvas.Children.Add(_steeringLine);


            Path annotationPath = new Path()
            {
                Fill = Brushes.Yellow,
                Stroke = Brushes.Black,
                StrokeThickness = 0.2,
                Data = PathGeometry.CreateFromGeometry(new FormattedText(annotation, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("sans"), 8, Brushes.Red).BuildGeometry(new Point())),
            };

            Canvas.Children.Add(annotationPath);
            Canvas.SetBottom(annotationPath, 2);
            Canvas.SetLeft(annotationPath, 2);

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

        private void DrawCar(Car car)
        {
            double h = 2;
            double w = 4;

            Path path = new Path()
            {
                Data = car.Geometry,
                Fill = new SolidColorBrush(car.Color),
                RenderTransform = new RotateTransform(180 * car.Angle / Math.PI, 0.5*w, 0.5*h),
                Stretch = Stretch.Fill,
                Height = h,
                Width = w,
                ToolTip = car.Driver,
            };
            _trails[car].ForEach(s => s.ToolTip = null);
            _trails[car].Add(path);

            Canvas.Children.Add(path);
            Canvas.SetLeft(path, car.Position.X - 0.5*w);
            Canvas.SetTop(path, car.Position.Y - 0.5*h);
        }

        private void DrawTargetEllipse()
        {
            Point target = ActiveCar.GetTarget();
            Point center = ActiveCar.Position + ActiveCar.Velocity;

            _previewLine = new Line()
            {
                StrokeThickness = _lineThickness,
                Stroke = new SolidColorBrush(ActiveCar.Color),
                X1 = ActiveCar.Position.X,
                Y1 = ActiveCar.Position.Y,
                X2 = target.X,
                Y2 = target.Y,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };
            _centerLine.Stroke = PlayerType.Online == ActiveCar.PlayerType ? Brushes.Transparent : new SolidColorBrush(ActiveCar.Color);
            _centerLine.X1 = ActiveCar.Position.X;
            _centerLine.Y1 = ActiveCar.Position.Y;
            _centerLine.X2 = center.X;
            _centerLine.Y2 = center.Y;

            _steeringLine.Stroke = PlayerType.Online == ActiveCar.PlayerType ? Brushes.Transparent : new SolidColorBrush(ActiveCar.Color);
            _steeringLine.X1 = center.X;
            _steeringLine.Y1 = center.Y;
            _steeringLine.X2 = target.X;
            _steeringLine.Y2 = target.Y;

            Canvas.Children.Add(_previewLine);

            Canvas.SetLeft(_targetPowerShape, center.X);
            Canvas.SetTop(_targetPowerShape, center.Y);
            _targetPowerShape.RenderTransform = new RotateTransform(180 * ActiveCar.Angle / Math.PI);

            _trails[ActiveCar].Add(_previewLine);
        }

        private bool IsWin()
        {
            return 2 < _trails[ActiveCar].Count && _track.Goal.CollidesWith(_previewLine, out _);
        }

        private bool IsCrash(out Point crashPoint)
        {
            List<Point> crashPoints = PowerShape.GetIntersectionPoints(_track.Bounds.RenderedGeometry, _previewLine.RenderedGeometry);

            foreach (Shape s in _track.Obstacles)
            {
                if(s.CollidesWith(_previewLine, out Point collisionPoint))
                {
                    crashPoints.Add(collisionPoint);
                }
            }

            if(0 == crashPoints.Count)
            {
                crashPoint = default;
                return false;
            }
            else
            {
                crashPoint = crashPoints.Aggregate((min, p) => p.DistanceSquared(_previewLine.StartPoint()) < min.DistanceSquared(_previewLine.StartPoint()) ? p : min);
                return true;
            }
        }

        private int CarIndex { get; set; } = 0;

        public async Task Move()
        {
            if(PlayerType.Online != ActiveCar.PlayerType)
            {
                foreach(Car car in _cars)
                {
                    if(PlayerType.Online == car.PlayerType)
                    {
                        car.NetworkConnector.ConfirmMove(new MoveParameter(ActiveCar.TargetAngle, ActiveCar.TargetPower));
                    }
                }
            }

            _previewLine.Stroke = new SolidColorBrush(ActiveCar.Color);

            double crashLength = 0;
            if (IsCrash(out Point crashPoint))
            {
                crashLength = 0.99 * (_previewLine.StartPoint() - crashPoint).Length;

                _previewLine.StrokeDashArray = new DoubleCollection(new double[] { 1.2, 0.8 });
                _previewLine.X2 = crashPoint.X;
                _previewLine.Y2 = crashPoint.Y;
            }

            foreach (Shape s in _trails[ActiveCar])
            {
                s.Opacity *= 0.8;
            }

            ActiveCar.Move(crashLength);
            DrawCar(ActiveCar);

            if (IsWin())
            {
                MessageBox.Show($"{ActiveCar.Driver} Wins the race!", "Winner");
            }

            ActiveCar = _cars[++CarIndex % _cars.Count];

            await WaitForNextMove();
        }

        private async Task WaitForNextMove()
        {
            DrawTargetEllipse();
            await ActiveCar.WaitForMove();
            await Move();
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
            Vector request = clicked - (ActiveCar.Position + ActiveCar.Velocity);

            ActiveCar.TargetAngle = Math.Atan2(request.Y, request.X) - ActiveCar.Angle;
            ActiveCar.TargetPower = Math.Min(1, request.Length / _targetPowerShape.GetRadius(ActiveCar.TargetAngle));
        }
    }
}
