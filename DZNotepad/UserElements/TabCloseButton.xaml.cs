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
    /// Логика взаимодействия для TabCloseButton.xaml
    /// </summary>
    public partial class TabCloseButton : UserControl
    {
        public event EventHandler Click;

        public TabCloseButton()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(sender, e);
            }
        }
    }
}
