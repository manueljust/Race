using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UIPlayground
{
    public class PowerShape : Shape
    {
        public static readonly DependencyProperty AccelerationProperty = DependencyProperty.Register(nameof(Acceleration), typeof(double), typeof(PowerShape), new PropertyMetadata(20d, RecalculateCallback));
        public static readonly DependencyProperty DecelerationProperty = DependencyProperty.Register(nameof(Deceleration), typeof(double), typeof(PowerShape), new PropertyMetadata(15d, RecalculateCallback));
        public static readonly DependencyProperty TurnRatioProperty = DependencyProperty.Register(nameof(TurnRatio), typeof(double), typeof(PowerShape), new PropertyMetadata(10d, RecalculateCallback));
        public static readonly DependencyProperty AreaProperty = DependencyProperty.Register(nameof(Area), typeof(double), typeof(PowerShape), new PropertyMetadata(2000d, RecalculateCallback));
        public static readonly DependencyProperty GeometryAreaProperty = DependencyProperty.Register(nameof(GeometryArea), typeof(double), typeof(PowerShape));
        public static readonly DependencyProperty EdgynessProperty = DependencyProperty.Register(nameof(Edgyness), typeof(double), typeof(PowerShape), new PropertyMetadata(0.5d, RecalculateCallback, CoerceEdgyness));

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

        public double GeometryArea
        {
            get { return (double)GetValue(GeometryAreaProperty); }
            set { SetValue(GeometryAreaProperty, value); }
        }

        public double Edgyness
        {
            get { return (double)GetValue(EdgynessProperty); }
            set { SetValue(EdgynessProperty, value); }
        }

        private static object CoerceEdgyness(DependencyObject d, object baseValue)
        {
            double val = (double)baseValue;
            if (double.IsNaN(val))
            {
                return 0.5d;
            }
            if (0d > val)
            {
                return 0d;
            }
            if (2d < val)
            {
                return 2d;
            }
            return val;
        }


        private static void RecalculateCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PowerShape ps = (PowerShape)d;
            if (null != ps)
            {
                ps.ReCalculate();
            }
        }

        public PowerShape()
        {
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
            return new PathGeometry(new PathFigure[] { new PathFigure(new Point(0, -t), segments, true) });
        }

        private void ReCalculate()
        {
            double scale = Math.Sqrt(Area / GetGeometry(Acceleration, Deceleration, TurnRatio).GetArea());
            _definingGeometry = GetGeometry(Acceleration * scale, Deceleration * scale, TurnRatio * scale);
            GeometryArea = _definingGeometry.GetArea();
            InvalidateVisual();
        }

        private Geometry _definingGeometry = null;
        protected override Geometry DefiningGeometry
        {
            get
            {
                return _definingGeometry;
            }
        }
    }
}
