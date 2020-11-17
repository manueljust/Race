using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Race
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public List<Car> Cars = new List<Car>() { new Car() { Color = Colors.Blue }, new Car() { Color = Colors.Brown } };

        public Car Car { get; set; }
        private Track Track { get; set; } = Tracks.GetTrack(1);

        Dictionary<Car, List<Tuple<Ellipse, Line>>> _trails = new Dictionary<Car, List<Tuple<Ellipse, Line>>>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Track = Track.FromSvg(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Track2.svg");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Track = Tracks.GetTrack(1);
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

            Car = Cars[0];
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Car)));

            DrawTrack();

            Loaded += (o, e) => DrawTargetEllipse();
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
                case nameof(Car.TargetAngle):
                case nameof(Car.TargetPower):
                    Point target = Car.GetTarget();
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
        private Ellipse _targetEllipse = new Ellipse() { StrokeThickness = 1, Stroke = Tracks.ParseBrush("#330088FF") };

        private void DrawTargetEllipse()
        {
            Point target = Car.GetTarget();
            Point center = Car.Position + Car.Velocity;

            _previewLine = new Line()
            {
                StrokeThickness = 1.2,
                Stroke = new SolidColorBrush(Car.Color),
                X1 = Car.Position.X,
                Y1 = Car.Position.Y,
                X2 = target.X,
                Y2 = target.Y,
                StrokeStartLineCap = PenLineCap.Triangle,
            };
            _centerLine.Stroke = new SolidColorBrush(Car.Color);
            _centerLine.X1 = Car.Position.X;
            _centerLine.Y1 = Car.Position.Y;
            _centerLine.X2 = center.X;
            _centerLine.Y2 = center.Y;

            _steeringLine.Stroke = new SolidColorBrush(Car.Color);
            _steeringLine.X1 = center.X;
            _steeringLine.Y1 = center.Y;
            _steeringLine.X2 = target.X;
            _steeringLine.Y2 = target.Y;

            Canvas.Children.Add(_previewLine);

            Canvas.SetLeft(_targetEllipse, center.X - Car.Acceleration);
            Canvas.SetTop(_targetEllipse, center.Y - Car.TurnRatio);
            _targetEllipse.Height = 2 * Car.TurnRatio;
            _targetEllipse.Width = 2 * Car.Acceleration;
            _targetEllipse.RenderTransform = new RotateTransform(180 * Car.Angle / Math.PI, Car.Acceleration, Car.TurnRatio);

            Ellipse pos = new Ellipse()
            {
                Fill = new SolidColorBrush(Car.Color),
                Height = 3,
                Width = 3,
            };
            _trails[Car].Add(new Tuple<Ellipse, Line>(pos, _previewLine));

            Canvas.Children.Add(pos);
            Canvas.SetLeft(pos, Car.Position.X - pos.Width / 2);
            Canvas.SetTop(pos, Car.Position.Y - pos.Height / 2);
        }

        private int _winCount = 0;
        private bool IsWin()
        {
            IntersectionDetail win = Track.Goal.RenderedGeometry.FillContainsWithDetail(_previewLine.RenderedGeometry);

            if(IntersectionDetail.Empty != win)
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

            foreach(Path p in Track.Obstacles)
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            _previewLine.Stroke = new SolidColorBrush(Car.Color);

            double penalty = 1;
            if (IsCrash())
            {
                _previewLine.Stroke = Brushes.Red;
                _previewLine.StrokeDashArray = new DoubleCollection(new double[] { 1.2, 0.8 });
                penalty = 0.5;
            }

            foreach(Tuple<Ellipse, Line> t in _trails[Car])
            {
                t.Item1.Opacity *= 0.6;
                t.Item2.Opacity *= 0.6;
            }

            Car.Move(penalty);

            if(IsWin())
            {
                MessageBox.Show("Winner!");
            }

            Car = Cars[++CarIndex % Cars.Count];
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Car)));

            DrawTargetEllipse();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetAngleAndPower(e.GetPosition(Canvas));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(MouseButtonState.Pressed == e.LeftButton)
            {
                SetAngleAndPower(e.GetPosition(Canvas));
            }
        }

        private void SetAngleAndPower(Point clicked)
        {
            Vector request = clicked - Car.Position - Car.Velocity;
            Car.TargetAngle = Math.Atan2(request.Y, request.X) - Car.Angle;
            double maxPower = Car.Acceleration * Car.TurnRatio / Math.Sqrt(Math.Pow(Car.Acceleration * Math.Sin(Car.TargetAngle), 2) + Math.Pow(Car.TurnRatio * Math.Cos(Car.TargetAngle), 2));
            Car.TargetPower = Math.Min(1, request.Length / maxPower);
        }

        private void thisMW_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Move_Click(this, null);
            }
        }
    }
}
