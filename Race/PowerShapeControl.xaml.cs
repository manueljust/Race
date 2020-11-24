﻿using System;
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
    /// Interaction logic for PowerShapeControl.xaml
    /// </summary>
    public partial class PowerShapeControl : UserControl
    {
        public static readonly DependencyProperty PowerShapeProperty = DependencyProperty.Register(
            nameof(PowerShape),
            typeof(PowerShape),
            typeof(PowerShapeControl)//,
//            new PropertyMetadata(new PowerShape() { Stroke = Brushes.LightBlue, StrokeThickness = 2 })
            );

        public PowerShape PowerShape
        {
            get { return (PowerShape)GetValue(PowerShapeProperty); }
            set { SetValue(PowerShapeProperty, value); }
        }

        public PowerShapeControl()
        {
            InitializeComponent();
        }
    }
}
