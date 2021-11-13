using DZNotepad.UserElements;
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
using System.Windows.Shapes;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для NameStyle.xaml
    /// </summary>
    public partial class NameStyle : Window
    {
        SelectStyle selectStyle; 

        public NameStyle(SelectStyle selectStyleWin)
        {
            InitializeComponent();

            selectStyle = selectStyleWin;
        }

        private void saveStyle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameStyle.Text))  
                MessageBox.Show("Введите имя!");
            else 
            {
                selectStyle.StyleList.Items.Add(new StyleItem(nameStyle.Text, selectStyle));
                //TODO: БД добавление
                this.Close();
            }
        }

        private void CanpelCall_Click(object sender, RoutedEventArgs e)
        {
            this.Close();      
        }
    }
}
