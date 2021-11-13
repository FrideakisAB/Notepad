using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для StyleItem.xaml
    /// </summary>
    public partial class StyleItem : UserControl, INotifyPropertyChanged
    {
        private string text;
        public string Text 
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        SelectStyle selectStyle;

        public StyleItem(string label, SelectStyle selectStyleWin)
        {
            InitializeComponent();
            DataContext = this;

            selectStyle = selectStyleWin;
            Text = label;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetStyle_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DropItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы хотите удалить стиль " + Text, "Удаление " + Text, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                selectStyle.StyleList.Items.Remove(this);
                //TODO: БД удаление
            }
        }
    }
}
