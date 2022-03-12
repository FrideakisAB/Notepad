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

namespace DZNotepad
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

        private SelectStyle SelectStyleObject;

        public StyleItem(string label, SelectStyle selectStyleWin)
        {
            InitializeComponent();
            DataContext = this;

            SelectStyleObject = selectStyleWin;
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
            Apply();
        }

        private void DropItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteElement();
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            RequestRename();
        }

        public void RequestRename()
        {
            OneFieldDialog renameStyle = new OneFieldDialog();
            renameStyle.InputName = "Введите новое название стиля";
            renameStyle.ValidatorCallback += ValidateRenameStyle;
            renameStyle.ShowDialog();

            if (renameStyle.DialogResult == true)
            {
                DBContext.Command($"UPDATE stylesNames SET styleName = '{renameStyle.Result}' WHERE styleNameId = (SELECT styleNameId FROM stylesNames WHERE styleName = '{text}')");
                text = renameStyle.Result;
            }
        }

        private MessageBoxResult ValidateRenameStyle(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || (long)DBContext.CommandScalar($"SELECT COUNT(styleNameId) FROM stylesNames WHERE styleName = '{input}'") != 0)
            {
                MessageBox.Show("Введите уникальное имя!");
                return MessageBoxResult.No;
            }
            else if (input == text)
            {
                MessageBox.Show("Введите имя, отличное от текущего!");
                return MessageBoxResult.No;
            }
            else
            {
                var result = MessageBox.Show("Вы хотите переименовать стиль " + input, "Переименование " + input, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result;
            }
        }

        public void DeleteElement()
        {
            var result = MessageBox.Show("Вы хотите удалить стиль " + Text, "Удаление " + Text, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SelectStyleObject.StyleList.Items.Remove(this);
                DBContext.Command($"DELETE FROM stylesNames WHERE styleNameId = (SELECT styleNameId FROM stylesNames WHERE styleName = '{text}')");
            }
        }

        public void Apply()
        {
            ResourceDictionary dictionary = new ResourceDictionary();
            DictionaryProvider.LoadStyleFromDB(dictionary, text);
            SelectStyleObject.Notify(dictionary);
        }
    }
}
