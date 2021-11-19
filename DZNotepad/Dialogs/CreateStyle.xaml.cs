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
    /// Логика взаимодействия для CreateStyle.xaml
    /// </summary>
    public partial class CreateStyle : Window
    {
        SelectStyle selectStyle;

        public CreateStyle(SelectStyle selectStyleWin)
        {
            InitializeComponent();

            selectStyle = selectStyleWin;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);
        }

        ~CreateStyle()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }

        private void saveStyle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameStyle.Text) || (long)DBContext.CommandScalar($"SELECT COUNT(styleNameId) FROM stylesNames WHERE styleName = '{nameStyle.Text}'") != 0)  
                MessageBox.Show("Введите уникальное имя!");
            else 
            {
                selectStyle.StyleList.Items.Add(new StyleItem(nameStyle.Text, selectStyle));
                DBContext.Command($"INSERT INTO stylesNames(styleName) VALUES('{nameStyle.Text}');");

                long id = (long)DBContext.CommandScalar($"SELECT styleNameId FROM stylesNames WHERE styleName = '{nameStyle.Text}'");
                DBContext.Command(string.Format(DBContext.LoadScriptFromResource("DZNotepad.SQLScripts.LightThemeSetup.sql"), id));
                this.Close();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();      
        }
    }
}
