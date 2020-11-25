using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Race
{
    public static class Tracks
    {
        public static Brush ParseBrush(string s)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
        }

        public static PathGeometry ParsePathGeometry(string s)
        {
            return new PathGeometry(
                        (PathFigureCollection)(new PathFigureCollectionConverter()).ConvertFromString(s),
                        FillRule.Nonzero,
                        Transform.Identity);
        }


        public static Track GetTrack(int index)
        {
            switch (index)
            {
                default:
                    return new Track()
                    {
                        Width = 210,
                        Height = 150,
                        Bounds = new Path()
                        {
                            Fill = ParseBrush("#FFCCCCCC"),
                            StrokeThickness = 1,
                            Stroke = ParseBrush("#FF000000"),
                            Data = ParsePathGeometry("m 185.08805 10.553897 c -4.21007 -0.124989 -9.69262 5.256659 -18.31206 11.362098 -12.82893 9.087162 -80.180909 -9.086984 -99.691586 -7.216098 -19.510676 1.870887 -35.565592 4.222624 -42.495599 14.432712 -6.930002 10.210085 -7.483561 22.183166 -4.009056 28.864903 6.507218 12.513876 73.766536 12.828982 96.751711 12.027173 22.98518 -0.80181 41.6937 -31.003261 45.43547 -26.994218 3.74177 4.009044 -34.74479 28.59755 -43.83195 34.477481 -9.08717 5.87993 -103.433381 7.483552 -106.373346 28.330572 -2.9399642 20.84703 57.730129 28.33056 89.267936 26.99422 31.5378 -1.33635 82.85357 -43.03029 90.33712 -55.591958 7.48354 -12.561667 3.4746 -52.117576 -1.60352 -62.00655 -1.66626 -3.244819 -3.41904 -4.619293 -5.47512 -4.680335 z M 96.384939 28.322322 c 27.417541 -0.07788 63.208591 1.845654 67.986031 2.948139 6.94901 1.603618 12.82847 6.681798 14.69936 16.036231 3.59268 17.96343 -8.55231 28.597812 -27.79572 49.979378 -19.24341 21.38156 -81.517334 20.04518 -92.742655 18.9761 C 47.306634 115.1931 36.61571 114.65842 48.1083 105.83852 c 11.49259 -8.81989 48.643084 17.64025 91.67347 -13.363008 43.03039 -31.003264 29.39975 -52.652305 26.72705 -56.126808 -2.67269 -3.474506 -27.79579 -1.069359 -47.841 7.483266 -20.045223 8.552624 -54.523339 12.294639 -67.886813 8.820131 -13.363474 -3.4745 0.1338 -19.109854 17.105416 -22.717993 5.303628 -1.127543 16.035998 -1.576387 28.498516 -1.611786 z"),
                        },
                        Obstacles = new Path[]
                        {
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF0000"),
                                Data = ParsePathGeometry("m 52.117555 37.685001 a 6.1471992 5.3453903 0 0 1 -4.496084 5.148963 6.1471992 5.3453903 0 0 1 -6.911348 -2.382979 6.1471992 5.3453903 0 0 1 0.783361 -6.42908 6.1471992 5.3453903 0 0 1 7.332163 -1.070676 l -2.855291 4.733772 z"),
                            },
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF0000"),
                                Data = PathGeometry.CreateFromGeometry(new EllipseGeometry()
                                {
                                    Center = new Point(98.4, 56.1),
                                    RadiusX = 6.15,
                                    RadiusY = 5.35,
                                }),
                            }
                        },
                        Decorations = new Path[]
                        {
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF00FF"),
                                Data = ParsePathGeometry("m 24.588797 81.784476 -5.058002 -4.233101 -6.354585 1.766851 2.462909 -6.118546 -3.644051 -5.497582 6.580164 0.451631 4.102439 -5.164545 1.603855 6.39767 6.179497 2.305719 -5.588927 3.502346 z"),
                            },
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF00FF"),
                                Data = ParsePathGeometry("m 90.604366 135.77292 -6.969942 -2.89366 -6.335091 4.10121 0.5982 -7.52299 -5.858137 -4.75769 7.33965 -1.75581 2.714563 -7.04163 3.937954 6.43785 7.535829 0.40572 -4.905861 5.73462 z"),
                            },
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF00FF"),
                                Data = ParsePathGeometry("m 139.78196 101.02788 1.96577 3.58206 4.06524 0.41136 -2.79928 2.97648 0.86499 3.99339 -3.69582 -1.74249 -3.53064 2.05668 0.51513 -4.05339 -3.04705 -2.72229 4.0142 -0.76265 z"),
                            },
                            new Path()
                            {
                                Fill = ParseBrush("#FFFF00FF"),
                                Data = ParsePathGeometry("m 120.27129 45.703089 2.40383 -4.353124 -2.22324 -4.448069 4.88289 0.940988 3.54335 -3.488949 0.61397 4.934686 4.41314 2.29178 -4.50344 2.108816 -0.81588 4.905347 -3.39724 -3.631366 z"),
                            }, new Path()
                            {
                                Fill = ParseBrush("#FFFF00FF"),
                                Data = ParsePathGeometry("m 68.688269 11.759861 3.644127 1.368279 3.193068 -2.226247 -0.175214 3.888593 3.103998 2.34884 -3.752415 1.035003 -1.274691 3.677909 -2.143907 -3.248926 -3.891801 -0.07577 2.427409 -3.04295 z"),
                            },
                        },
                        Start = new Line()
                        {
                            Fill = ParseBrush("#FF00FF00"),
                            X1 = 43.118299,
                            Y1 = 89.690132,
                            X2 = -8.480901,
                            Y2 = -14.110948,
                    },
                    Goal = new Line()
                    {
                        Fill = Brushes.Transparent,
                        X1 = 43.118299,
                        Y1 = 89.690132,
                        X2 = -8.480901,
                        Y2 = -14.110948,
                    },
                    Background = Brushes.ForestGreen,
                };
                case 1:
                    return new Track()
                    {
                        Width = 210,
                        Height = 150,
                        Bounds = new Path()
                        {
                            Fill = ParseBrush("#FFCCCCCC"),
                            StrokeThickness = 1,
                            Stroke = ParseBrush("#FF000000"),
                            Data = ParsePathGeometry("M 162.72135 11.419681 C 152.07058 12.48963 141.37641 18.452968 124.49283 30.46265 118.3119 34.859285 109.21666 36.295448 98.174473 35.489993 87.132283 34.684539 74.7988 31.698572 63.590485 29.146243 52.382169 26.593915 43.206762 23.151979 32.104157 26.751712 c -5.551303 1.799867 -11.394603 8.278866 -12.21875 14.697266 -0.824148 6.418399 0.882441 12.026198 3.671874 18.794922 3.897003 9.456305 12.584939 14.047943 20.158204 16.53125 7.573265 2.483306 15.253371 3.521648 21.623046 4.802734 4.875786 0.98063 11.528889 2.670116 13.516576 3.584399 -2.45982 0.289831 -8.005776 0.07526 -11.272435 0.478101 -9.93721 1.225441 -22.457966 2.217053 -32.095703 11.761715 -2.453387 2.429701 -4.481297 6.937481 -4.15625 10.966801 0.325047 4.02932 2.132338 6.79733 3.814453 8.78711 3.36423 3.97956 7.138371 6.10953 11.689453 8.35937 9.102164 4.49969 21.461355 8.29911 35.667969 10.85743 28.413226 5.11664 64.122446 5.74915 89.666016 -12.46094 7.5049 -5.35026 12.56549 -12.93514 16.2168 -20.74609 3.65131 -7.810961 6.0891 -15.509733 5.08594 -23.931646 -0.50159 -4.210956 -2.31753 -9.5861 -7.39649 -12.96875 -5.07896 -3.38265 -11.31667 -3.036414 -16.06836 -1.421875 -9.50338 3.229079 -18.33609 10.697725 -31.26758 24.556641 -2.26265 2.424926 -3.49795 2.413953 -4.88086 2.232421 -1.3829 -0.181531 -2.9091 -1.205263 -3.57617 -2.257812 -0.66707 -1.052549 -0.94676 -1.528189 -0.0898 -3.333984 0.85691 -1.805796 3.85979 -5.461041 12.07617 -8.925782 10.1139 -4.264897 18.60185 -11.536461 25.3457 -19.349609 6.74386 -7.813148 11.81213 -15.776458 13.72657 -24.517578 0.95722 -4.37056 1.40863 -9.88405 -2.375 -15.251953 -3.78364 -5.367903 -10.91876 -7.111147 -16.24414 -6.576172 z m -2.0052 19.60203 c -1.30312 2.040133 -7.99336 10.325944 -9.75652 12.368673 -5.02247 5.818823 -11.62847 11.087539 -17.23828 13.453125 -11.49712 4.848182 -19.45778 11.449048 -23.4043 19.765625 -3.94652 8.316577 -2.87156 17.828183 1.38281 24.541016 4.25437 6.71283 11.21669 11.23451 19.29492 12.29492 8.07824 1.06042 17.27706 -2.01215 23.83008 -9.03515 5.85342 -6.273221 14.90433 -16.432196 18.73107 -19.446389 -0.37637 1.097492 -4.58872 7.791642 -5.10021 8.885837 -2.45106 5.243362 -6.33052 10.205542 -9.05859 12.150392 -16.74214 11.93552 -47.90522 13.23906 -72.996099 8.7207 -8.785723 -1.58213 -23.378495 -4.77144 -29.552681 -7.02622 2.171144 -0.47239 11.11136 0.0658 13.427681 -0.21987 4.482872 -0.55282 8.861274 -0.91515 12.892579 -1.56445 2.015652 -0.32466 3.917099 -0.65747 6.236328 -1.53321 2.319231 -0.87573 6.517001 -2.3766 8.458985 -8.341791 1.65996 -5.098893 2.285717 -10.251552 1.16016 -15.255863 C 97.898523 75.774745 94.855029 71.40463 91.410799 68.558353 84.52233 62.865799 76.884859 61.459304 69.676422 60.009525 62.467986 58.559745 55.414224 57.45767 50.569 55.8689 c -4.845223 -1.58877 -6.257749 -3.002913 -6.671875 -4.007813 -0.674041 -1.635601 -0.886636 -2.634116 -1.205078 -3.787109 3.465829 0.315529 8.790905 0.878688 16.013672 2.523437 10.895382 2.481068 24.1906 5.838321 37.86719 6.835938 13.676581 0.997617 28.388891 -0.305796 40.671871 -9.042969 7.42755 -5.283385 18.78598 -14.995123 23.47137 -17.368673 z"),
                        },
                        Decorations = new Path[0],
                        Obstacles = new Path[]
                        {
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(157.6, 27.6), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(154.9, 33.6), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(151.1, 38.8), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(147.3, 43.6), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(142.6, 47.8), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(40.5, 47.1), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(45.5, 51.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(51.9, 54), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(58, 55.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(76.1, 82.7), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(69.7, 81.1), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(63.3, 79.9), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(56.9, 78.7), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(128.3, 79.7), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(126.2, 85.8), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(132.1, 88.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(137.4, 84.6), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(134.5, 77.3), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(140.2, 74.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(142.4, 80.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(169.9, 82.7), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(166.1, 88), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(162.1, 93), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(158.3, 97.9), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(153.5, 102.2), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(148.3, 106.3), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(142.1, 109.1), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(135.7, 111.2), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(129.1, 112.4), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(122.4, 112.7), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(53.5, 105.1), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(59.9, 106), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(66.8, 106.5), RadiusX = 3, RadiusY = 3 }) },
                            new Path() { Fill = ParseBrush("#FFA84900"), Stroke = ParseBrush("#FF000000"), StrokeThickness = 0.3, Data = PathGeometry.CreateFromGeometry(new EllipseGeometry() { Center = new Point(73.2, 107.7), RadiusX = 3, RadiusY = 3 }) },
                        },
                        Start = new Line()
                        {
                            Fill = ParseBrush("#FF00FF00"),
                            X1 = 43.118299,
                            Y1 = 89.690132,
                            X2 = -8.480901,
                            Y2 = -14.110948,
                        },
                        Goal = new Line()
                        {
                            Fill = Brushes.Transparent,
                            X1 = 43.118299,
                            Y1 = 89.690132,
                            X2 = -8.480901,
                            Y2 = -14.110948,
                        },
                        Background = Brushes.ForestGreen,
                    };
            }
        }
    }
}
