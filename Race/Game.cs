using Race.Util;
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
        private static readonly Random _random = new Random();
        private static readonly double _lineThickness = 0.8;

        private Line _previewLine = new Line();
        private Line _centerLine = new Line() { StrokeThickness = _lineThickness, Stroke = Brushes.Brown, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7 };
        private Line _steeringLine = new Line() { StrokeThickness = _lineThickness, Stroke = Brushes.Red, StrokeDashArray = { 1, 0.5 }, Opacity = 0.7 };
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

            NewGame(@"Tracks\Track1.svg", Car.DefaultCars, 0 == _random.Next() % 2 ? RaceDirection.Clockwise : RaceDirection.Counterclockwise);
        }

        public void NewGame(string filename, IEnumerable<Car> cars, RaceDirection direction)
        {
            Canvas.Children.Clear();
            _cars.Clear();
            _trails.Clear();

            try
            {
                _track = SvgHelper.Load(filename);

                double i = 0.5;
                foreach (Car car in cars.ToDictionary(p => _random.Next()).OrderBy(p => p.Key).Select(p => p.Value))
                {
                    car.Position = _track.Start.StartPoint() + (i / cars.Count()) * _track.Start.Vector();
                    car.Angle = RaceDirection.Clockwise == direction ? -Math.Atan2(_track.Start.Vector().X, _track.Start.Vector().Y) : -Math.Atan2(-_track.Start.Vector().X, -_track.Start.Vector().Y);
                    car.Velocity = new Vector();
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
                StrokeThickness = _lineThickness,
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

            Canvas.SetLeft(_targetPowerShape, center.X);
            Canvas.SetTop(_targetPowerShape, center.Y);
            _targetPowerShape.RenderTransform = new RotateTransform(180 * ActiveCar.Angle / Math.PI);

            _trails[ActiveCar].Add(_previewLine);
        }

        private bool IsWin()
        {
            return 2 < _trails[ActiveCar].Count && _track.Goal.CollidesWith(_previewLine);
        }

        private bool IsCrash()
        {
            IntersectionDetail onTrack = _track.Bounds.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

            if (onTrack != IntersectionDetail.FullyContains)
            {
                return true;
            }

            foreach (Shape s in _track.Obstacles)
            {
                if(s.CollidesWith(_previewLine))
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
                //_previewLine.Stroke = Brushes.Red;
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
            Vector request = clicked - (ActiveCar.Position + ActiveCar.Velocity);

            ActiveCar.TargetAngle = Math.Atan2(request.Y, request.X) - ActiveCar.Angle;
            ActiveCar.TargetPower = Math.Min(1, request.Length / _targetPowerShape.GetRadius(ActiveCar.TargetAngle));
        }
    }
}
