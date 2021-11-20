using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DZNotepad
{
    public class ColorPicker : Control
    {
        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public event EventHandler SelectedColorChanged;

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ColorPicker));

        internal int UserCustomValue
        {
            get { return (int)GetValue(UserCustomValueProperty); }
            set { SetValue(UserCustomValueProperty, value); }
        }

        internal static readonly DependencyProperty UserCustomValueProperty =
            DependencyProperty.Register("UserCustomValue", typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(UserCustomValue_Changed));

        public string HexValue
        {
            get { return (string)GetValue(HexValueProperty); }
            set { SetValue(HexValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HexValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexValueProperty =
            DependencyProperty.Register("HexValue", typeof(string), typeof(ColorPicker), new PropertyMetadata(GetDefaultColors().FirstOrDefault().ToString()));

        public Brush SelectedColor
        {
            get { return (Brush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(ColorPicker), new FrameworkPropertyMetadata(GetDefaultColors().FirstOrDefault(), SelectedColor_Changed));

        internal Brush DefaultColor
        {
            get { return (Brush)GetValue(DefaultColorProperty); }
            set { SetValue(DefaultColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultColor.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register("DefaultColor", typeof(Brush), typeof(ColorPicker), new FrameworkPropertyMetadata(DefaultColor_Changed));

        internal ObservableCollection<Brush> DefaultColors
        {
            get { return (ObservableCollection<Brush>)GetValue(DefaultColorsProperty); }
            set { SetValue(DefaultColorsProperty, value); }
        }

        internal static readonly DependencyProperty DefaultColorsProperty =
            DependencyProperty.Register("DefaultColors", typeof(ObservableCollection<Brush>), typeof(ColorPicker), new PropertyMetadata(GetDefaultColors()));

        internal int AlphaValue
        {
            get { return (int)GetValue(AlphaValueProperty); }
            set { SetValue(AlphaValueProperty, value); }
        }

        internal static readonly DependencyProperty AlphaValueProperty =
            DependencyProperty.Register("AlphaValue", typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(255, AlphaValue_Changed));

        internal Color StartColor
        {
            get { return (Color)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }

        internal static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register("StartColor", typeof(Color), typeof(ColorPicker));

        internal Color EndColor
        {
            get { return (Color)GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty, value); }
        }

        internal static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register("EndColor", typeof(Color), typeof(ColorPicker));

        private static void AlphaValue_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var cp = obj as ColorPicker;
            var color = (cp.SelectedColor as SolidColorBrush).Color;
            var alpha = Convert.ToByte(e.NewValue);

            cp.SelectedColor = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        }

        private static void DefaultColor_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var cp = obj as ColorPicker;
            var color = (e.NewValue as SolidColorBrush).Color;
            var alpha = Convert.ToByte(cp.AlphaValue);

            cp.SelectedColor = new SolidColorBrush(Color.FromArgb((byte)alpha, color.R, color.G, color.B));
        }

        private static void UserCustomValue_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var cp = obj as ColorPicker;
            var colors = GetUserCustomColors();
            var cc = colors[Convert.ToInt32(e.NewValue)];
            var alpha = Convert.ToByte(cp.AlphaValue);

            cp.SelectedColor = new SolidColorBrush(Color.FromArgb(alpha, Convert.ToByte(cc.Item1), Convert.ToByte(cc.Item2), Convert.ToByte(cc.Item3)));
        }

        private static void SelectedColor_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var cp = obj as ColorPicker;
            var color = (e.NewValue as SolidColorBrush).Color;

            cp.HexValue = (e.NewValue as SolidColorBrush).Color.ToString();

            cp.StartColor = Color.FromArgb(0, color.R, color.G, color.B);
            cp.EndColor = Color.FromArgb(255, color.R, color.G, color.B);
            cp.SelectedColorChanged?.Invoke(cp, new EventArgs());
        }

        private static List<Tuple<int, int, int>> GetUserCustomColors()
        {
            var list = new List<Tuple<int, int, int>>();
            var max = 255;
            var min = 0;


            //#ff0000 -> #ffff00
            for (int i = 0; i <= 255; i++)
            {
                list.Add(Tuple.Create(max, i, min));
            }

            //#ffff00 -> #00ff00
            for (int i = 255; i >= 0; i--)
            {
                list.Add(Tuple.Create(i, max, min));
            }

            //#00ff00 -> #00ffff
            for (int i = 0; i <= 255; i++)
            {
                list.Add(Tuple.Create(min, max, i));
            }

            //#00ffff -> #0000ff
            for (int i = 255; i >= 0; i--)
            {
                list.Add(Tuple.Create(min, i, max));
            }

            //#0000ff -> #ff00ff
            for (int i = 0; i <= 255; i++)
            {
                list.Add(Tuple.Create(i, min, max));
            }

            //#ff00ff -> #ff0000
            for (int i = 255; i >= 0; i--)
            {
                list.Add(Tuple.Create(max, min, i));
            }

            return list;
        }
        private static ObservableCollection<Brush> GetDefaultColors()
        {
            var result = new ObservableCollection<Brush>();
            result.Add(Color.FromRgb(255, 255, 255).ToBrush());
            result.Add(Color.FromRgb(0, 0, 0).ToBrush());
            result.Add(Color.FromRgb(231, 230, 230).ToBrush());
            result.Add(Color.FromRgb(68, 84, 106).ToBrush());
            result.Add(Color.FromRgb(68, 114, 196).ToBrush());
            result.Add(Color.FromRgb(237, 125, 49).ToBrush());
            result.Add(Color.FromRgb(165, 165, 165).ToBrush());
            result.Add(Color.FromRgb(255, 192, 0).ToBrush());
            result.Add(Color.FromRgb(91, 155, 213).ToBrush());
            result.Add(Color.FromRgb(112, 173, 71).ToBrush());
            result.Add(Color.FromRgb(242, 242, 242).ToBrush());
            result.Add(Color.FromRgb(128, 128, 128).ToBrush());
            result.Add(Color.FromRgb(208, 206, 206).ToBrush());
            result.Add(Color.FromRgb(214, 220, 228).ToBrush());
            result.Add(Color.FromRgb(217, 225, 242).ToBrush());
            result.Add(Color.FromRgb(252, 228, 214).ToBrush());
            result.Add(Color.FromRgb(237, 237, 237).ToBrush());
            result.Add(Color.FromRgb(255, 242, 204).ToBrush());
            result.Add(Color.FromRgb(221, 235, 247).ToBrush());
            result.Add(Color.FromRgb(226, 239, 218).ToBrush());
            result.Add(Color.FromRgb(217, 217, 217).ToBrush());
            result.Add(Color.FromRgb(89, 89, 89).ToBrush());
            result.Add(Color.FromRgb(174, 170, 170).ToBrush());
            result.Add(Color.FromRgb(172, 185, 202).ToBrush());
            result.Add(Color.FromRgb(180, 198, 231).ToBrush());
            result.Add(Color.FromRgb(248, 203, 173).ToBrush());
            result.Add(Color.FromRgb(219, 219, 219).ToBrush());
            result.Add(Color.FromRgb(255, 230, 153).ToBrush());
            result.Add(Color.FromRgb(189, 215, 238).ToBrush());
            result.Add(Color.FromRgb(198, 224, 180).ToBrush());
            result.Add(Color.FromRgb(191, 191, 191).ToBrush());
            result.Add(Color.FromRgb(64, 64, 64).ToBrush());
            result.Add(Color.FromRgb(117, 113, 113).ToBrush());
            result.Add(Color.FromRgb(132, 151, 176).ToBrush());
            result.Add(Color.FromRgb(142, 169, 219).ToBrush());
            result.Add(Color.FromRgb(244, 176, 132).ToBrush());
            result.Add(Color.FromRgb(201, 201, 201).ToBrush());
            result.Add(Color.FromRgb(255, 217, 102).ToBrush());
            result.Add(Color.FromRgb(155, 194, 230).ToBrush());
            result.Add(Color.FromRgb(169, 208, 142).ToBrush());
            result.Add(Color.FromRgb(166, 166, 166).ToBrush());
            result.Add(Color.FromRgb(38, 38, 38).ToBrush());
            result.Add(Color.FromRgb(58, 56, 56).ToBrush());
            result.Add(Color.FromRgb(51, 63, 79).ToBrush());
            result.Add(Color.FromRgb(48, 84, 150).ToBrush());
            result.Add(Color.FromRgb(198, 89, 17).ToBrush());
            result.Add(Color.FromRgb(123, 123, 123).ToBrush());
            result.Add(Color.FromRgb(191, 143, 0).ToBrush());
            result.Add(Color.FromRgb(47, 117, 181).ToBrush());
            result.Add(Color.FromRgb(84, 130, 53).ToBrush());
            result.Add(Color.FromRgb(128, 128, 128).ToBrush());
            result.Add(Color.FromRgb(13, 13, 13).ToBrush());
            result.Add(Color.FromRgb(22, 22, 22).ToBrush());
            result.Add(Color.FromRgb(34, 43, 53).ToBrush());
            result.Add(Color.FromRgb(32, 55, 100).ToBrush());
            result.Add(Color.FromRgb(131, 60, 12).ToBrush());
            result.Add(Color.FromRgb(82, 82, 82).ToBrush());
            result.Add(Color.FromRgb(128, 96, 0).ToBrush());
            result.Add(Color.FromRgb(31, 78, 120).ToBrush());
            result.Add(Color.FromRgb(55, 86, 35).ToBrush());

            return result;
        }
    }

    public static class ColorExtensions
    {
        public static Brush ToBrush(this Color color)
        {
            return new SolidColorBrush(color);
        }
    }
}
