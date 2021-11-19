using DZNotepad.UserElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Логика взаимодействия для SelectStyle.xaml
    /// </summary>
    public partial class SelectStyle : Window
    {
        PreviewPage preview = new PreviewPage();
        public StyleItem Item
        {
            get
            {
               return (StyleItem)StyleList.SelectedItem;
            }
        }
        public SelectStyle()
        {
            InitializeComponent();

            
            StyleList.Items.Add(new StyleItem("Светлый стиль", this));
            StyleList.Items.Add(new StyleItem("Темный стиль", this));
            StyleList.Items.Add(new StyleItem("Серый стиль", this));
            //TODO: БД инициализация
        }

        private void DropItem_Click(object sender, RoutedEventArgs e)
        {
            if (StyleList.SelectedItem != null)
            {
                var result = MessageBox.Show("Вы хотите удалить стиль " + (StyleList.SelectedItem as StyleItem).Text, "Удаление " + (StyleList.SelectedItem as StyleItem).Text, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    StyleList.Items.Remove(StyleList.SelectedItem);
                    //TODO: БД удаление
                }
            }
            else 
            {
                MessageBox.Show("Выберите стиль для удаления!");
            }        
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            NameStyle nameStyle = new NameStyle(this);
            nameStyle.Owner = this;
            nameStyle.Show();
        }

        private void renameStyle_Click(object sender, RoutedEventArgs e)
        {

            RenameStyle renameStyle = new RenameStyle(this, (StyleList.SelectedItem as StyleItem));
            renameStyle.Owner = this;
            renameStyle.Show();
        }

        private void Elements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Elements.SelectedIndex)
            {
                case 0:
                    CornerValue.IsEnabled = true;
                    break;
                case 1:
                    CornerValue.IsEnabled = true;
                    break;
                case 2:
                    CornerValue.IsEnabled = true;
                    break;
                case 3:
                    CornerValue.IsEnabled = false;
                    break;                  
                case 4:                     
                    CornerValue.IsEnabled = false;
                    break;
                case 5:
                    CornerValue.IsEnabled = true;
                    break;
            }
        }

        private void CornerValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (Elements.SelectedIndex)
            {
                case 0:
                    preview.TestButton.Resources["anyButtonCornerVal"] = new CornerRadius(CornerValue.Value);
                    break;
                case 1:
                    preview.TestComboBox.Resources["anyComboCornerVal"] = new CornerRadius(CornerValue.Value);
                    break;
                case 2:
                    preview.TestTextBox.Resources["anyTBCornerVal"] = new CornerRadius(CornerValue.Value);
                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    preview.TestTab.Resources["anyTabItemCornerVal"] = new CornerRadius(CornerValue.Value);
                    break;
            }
        }

        private void FontFamilyBox_DropDownClosed(object sender, EventArgs e)
        {
            switch (Elements.SelectedIndex)
            {
                case 0:
                    preview.TestButton.Resources["anyButtonFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
                case 1:
                    preview.TestComboBox.Resources["anyComboFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
                case 2:
                    preview.TestTextBox.Resources["anyTBFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
                case 3:
                    preview.TestText.Resources["anyFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
                case 4:
                    preview.TestBackground.Resources["anyFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
                case 5:
                    preview.TestTab.Resources["anyTabItemFontFamilyVal"] = new FontFamily(FontFamilyBox.SelectedItem.ToString());
                    break;
            }
        }
    }
}
