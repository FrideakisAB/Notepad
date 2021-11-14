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
    /// Логика взаимодействия для RenameStyle.xaml
    /// </summary>
    public partial class RenameStyle : Window
    {
        StyleItem item;
        SelectStyle selectStyle;
        public RenameStyle(SelectStyle selectStyleWin, StyleItem styleItem)
        {
            InitializeComponent();

            selectStyle = selectStyleWin;

            item = styleItem;           
        }

        private void CancelRename_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void applyRename_Click(object sender, RoutedEventArgs e)
        {
            if (item != null)
            {
                if (string.IsNullOrWhiteSpace(NnameStyle.Text))
                    MessageBox.Show("Введите имя!");
                else
                {
                    var result = MessageBox.Show("Вы хотите переименовать стиль " + item.Text, "Переименование " + item.Text, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //TODO: БД изменение
                        item.Text = NnameStyle.Text;
                        this.Close();
                    }
                }
            }
            else 
            {
                MessageBox.Show("Выберите стиль для редактирования!");
            }
            
        }
    }
}
