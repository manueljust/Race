using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Race
{
    /// <summary>
    /// Interaction logic for CarControl.xaml
    /// </summary>
    public partial class CarControl : UserControl
    {
        public static readonly DependencyProperty CarProperty = DependencyProperty.Register(
            nameof(Car),
            typeof(Car),
            typeof(CarControl),
            new PropertyMetadata(new Car())
            );

        public Car Car
        {
            get { return (Car)GetValue(CarProperty); }
            set { SetValue(CarProperty, value); }
        }


        public CarControl()
        {
            InitializeComponent();
        }
    }
}
