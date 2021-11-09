using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DZNotepad.UserElements
{
    /// <summary>
    /// Логика взаимодействия для ToggleSlider.xaml
    /// </summary>
    public partial class ToggleSlider : UserControl
    {
        public RoutedEventHandler CheckedChange { get; set; }
        public bool IsChecked
        {
            get { return (bool)btnToogle.IsChecked; }
        }

        public ToggleSlider()
        {
            InitializeComponent();

            btnToogle.Checked += checkedLocal;
            btnToogle.Unchecked += checkedLocal;
        }

        private void checkedLocal(object sender, RoutedEventArgs e)
        {
            CheckedChange?.Invoke(this, e);
        }
    }
}
