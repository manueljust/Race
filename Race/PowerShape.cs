using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Shapes;

namespace Race
{
    public class PowerShape : Shape
    {
        private static readonly double MagicCircleNumber = 0.551784d;

        public static readonly DependencyProperty AccelerationProperty = DependencyProperty.Register(nameof(Acceleration), typeof(double), typeof(PowerShape), new PropertyMetadata(0.8d, RecalculateCallback, CoercePower));
        public static readonly DependencyProperty DecelerationProperty = DependencyProperty.Register(nameof(Deceleration), typeof(double), typeof(PowerShape), new PropertyMetadata(0.8d, RecalculateCallback, CoercePower));
        public static readonly DependencyProperty TurnRatioProperty = DependencyProperty.Register(nameof(TurnRatio), typeof(double), typeof(PowerShape), new PropertyMetadata(0.5d, RecalculateCallback, CoercePower));
        public static readonly DependencyProperty AreaProperty = DependencyProperty.Register(nameof(Area), typeof(double), typeof(PowerShape), new PropertyMetadata(100d, RecalculateCallback, CoerceArea));
        public static readonly DependencyProperty EdgynessProperty = DependencyProperty.Register(nameof(Edgyness), typeof(double), typeof(PowerShape), new PropertyMetadata(MagicCircleNumber, RecalculateCallback, CoerceEdgyness));

        public double Acceleration
        {
            get { return (double)GetValue(AccelerationProperty); }
            set { SetValue(AccelerationProperty, value); }
        }

        public double Deceleration
        {
            get { return (double)GetValue(DecelerationProperty); }
            set { SetValue(DecelerationProperty, value); }
        }

        public double TurnRatio
        {
            get { return (double)GetValue(TurnRatioProperty); }
            set { SetValue(TurnRatioProperty, value); }
        }

        public double Area
        {
            get { return (double)GetValue(AreaProperty); }
            set { SetValue(AreaProperty, value); }
        }

        public double Edgyness
        {
            get { return (double)GetValue(EdgynessProperty); }
            set { SetValue(EdgynessProperty, value); }
        }

        private static object CoerceEdgyness(DependencyObject d, object baseValue)
        {
            return Constrained((double)baseValue, 0, 2, MagicCircleNumber);
        }

        private static object CoercePower(DependencyObject d, object baseValue)
        {
            return Constrained((double)baseValue, 0.1, 1, 0.5);
        }

        private static object CoerceArea(DependencyObject d, object baseValue)
        {
            return Constrained((double)baseValue, 10, 100, 100);
        }

        private static double Constrained(double d, double min, double max, double defaultValue)
        {
            return double.IsNaN(d) ? defaultValue : min > d ? min : max < d ? max : d;
        }


        private static void RecalculateCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PowerShape ps = (PowerShape)d;
            if (null != ps)
            {
                ps.ReCalculate();
            }
        }

        private double _scaledAcceleration = 0;
        private double _scaledDeceleration = 0;
        private double _scaledTurnRatio = 0;

        public PowerShape()
        {
            Stroke = Brushes.Black;
            StrokeThickness = 1;
            ReCalculate();
        }

        private Geometry GetGeometry(double a, double d, double t)
        {
            PathSegmentCollection segments = new PathSegmentCollection(new PathSegment[]
            {
                new BezierSegment(new Point(Edgyness * a, -t), new Point(a, Edgyness * -t), new Point(a, 0), true),
                new BezierSegment(new Point(a, Edgyness * t), new Point(Edgyness * a, t), new Point(0, t), true),
                new BezierSegment(new Point(Edgyness * -d, t), new Point(-d, Edgyness * t), new Point(-d, 0), true),
                new BezierSegment(new Point(-d, Edgyness * -t), new Point(Edgyness * -d, -t), new Point(0, -t), true),
            });
            return new PathGeometry(new PathFigure[] { new PathFigure(new Point(0, -t), segments, true), new PathFigure(new Point(0, 0), new PathSegmentCollection(new PathSegment[] { new LineSegment(new Point(0, 0), true) }), true) });
        }

        private void ReCalculate()
        {
            double scale = Math.Sqrt(Area / GetGeometry(Acceleration, Deceleration, TurnRatio).GetArea());
            _scaledAcceleration = scale * Acceleration;
            _scaledDeceleration = scale * Deceleration;
            _scaledTurnRatio = scale * TurnRatio;

            _definingGeometry = GetGeometry(_scaledAcceleration, _scaledDeceleration, _scaledTurnRatio);

            InvalidateVisual();
        }

        public void MorphInto(PowerShape ps)
        {
            Acceleration = ps.Acceleration;
            Deceleration = ps.Deceleration;
            TurnRatio = ps.TurnRatio;
            Edgyness = ps.Edgyness;
            Area = ps.Area;
        }

        public double GetRadius(double Angle)
        {
            double max = 2 * Math.Max(_scaledTurnRatio, Math.Max(_scaledAcceleration, _scaledDeceleration));
            double min = 0.5 * Math.Min(_scaledTurnRatio, Math.Min(_scaledAcceleration, _scaledDeceleration));
            LineGeometry lg = new LineGeometry(
                new Point(min * Math.Cos(Angle), min * Math.Sin(Angle)),
                new Point(max * Math.Cos(Angle), max * Math.Sin(Angle))
            );

            Point p = GetIntersectionPoints(lg, _definingGeometry).FirstOrDefault();
            return new Vector(p.X, p.Y).Length;
        }

        private Geometry _definingGeometry = null;
        protected override Geometry DefiningGeometry
        {
            get
            {
                return _definingGeometry;
            }
        }

        public static Point[] GetIntersectionPoints(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 0.01));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 0.01));
            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);
            PathGeometry pg = cg.GetFlattenedPathGeometry();
            Point[] result = new Point[pg.Figures.Count];
            for (int i = 0; i < pg.Figures.Count; i++)
            {
                Rect fig = new PathGeometry(new PathFigure[] { pg.Figures[i] }).Bounds;
                result[i] = new Point(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
            }
            return result;
        }
    }
}
