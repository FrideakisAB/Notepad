using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using DZNotepad.Pages;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для SelectStyle.xaml
    /// </summary>
    public partial class SelectStyle : Window
    {
        public StyleItem SelectedItem
        {
            get
            {
               return (StyleItem)StyleList.SelectedItem;
            }
        }

        public delegate void UpdateStyle(ResourceDictionary dictionary);
        public static event UpdateStyle UpdateStyleObservers;
        public static ResourceDictionary CurrentDictionary;

        private IEditorPage CurrentEditor = null;
        private PreviewPage Preview = new PreviewPage();

        public SelectStyle()
        {
            InitializeComponent();

            PreviewFrame.Navigate(Preview);

            SqliteDataReader reader = DBContext.CommandReader("SELECT styleName FROM stylesNames");

            if (!reader.HasRows)
            {
                DBContext.Command(DBContext.LoadScriptFromResource("DZNotepad.SQLScripts.SetupBaseStyles.sql"));
                reader = DBContext.CommandReader("SELECT styleName FROM stylesNames");
            }

            if (reader.HasRows)
            {
                while (reader.Read())
                    StyleList.Items.Add(new StyleItem(reader.GetString(0), this));
            }

            UpdateStyleObservers += SelectStyle_UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, CurrentDictionary);

            EditableItem.SelectedIndex = 0;
        }

        ~SelectStyle()
        {
            UpdateStyleObservers -= SelectStyle_UpdateStyleObservers;
        }

        private void SelectStyle_UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }

        private void DropItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem?.DeleteElement();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            OneFieldDialog createStyle = new OneFieldDialog();
            createStyle.InputName = "Введите название стиля";
            createStyle.ValidatorCallback += ValidateNewStyle;
            createStyle.ShowDialog();

            if (createStyle.DialogResult == true)
            {
                StyleList.Items.Add(new StyleItem(createStyle.Result, this));
                DBContext.Command($"INSERT INTO stylesNames(styleName) VALUES('{createStyle.Result}');");

                long id = (long)DBContext.CommandScalar($"SELECT styleNameId FROM stylesNames WHERE styleName = '{createStyle.Result}'");
                DBContext.Command(string.Format(DBContext.LoadScriptFromResource("DZNotepad.SQLScripts.LightThemeSetup.sql"), id));
            }
        }

        private MessageBoxResult ValidateNewStyle(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || (long)DBContext.CommandScalar($"SELECT COUNT(styleNameId) FROM stylesNames WHERE styleName = '{input}'") != 0)
            {
                MessageBox.Show("Введите уникальное имя!");
                return MessageBoxResult.No;
            }

            return MessageBoxResult.OK;
        }

        private void RenameStyle_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem?.RequestRename();
        }

        public void Notify(ResourceDictionary dictionary)
        {
            CurrentDictionary = dictionary;
            UpdateStyleObservers?.Invoke(dictionary);
        }

        private void StyleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
            {
                DictionaryProvider.LoadStyleFromDB(Preview.Resources, SelectedItem.Text);
                CurrentEditor?.ChangePreview();
            }
        }

        private void ApplyChangedStyle_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                ResourceDictionary dictionary = new ResourceDictionary();
                DictionaryProvider.ApplyDictionary(dictionary, Preview.Resources);
                Notify(dictionary);
            }
        }

        private void SaveChangedStyle_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                DBContext.Command($"DELETE FROM styles WHERE styleNameId = (SELECT styleNameId FROM stylesNames WHERE styleName = '{SelectedItem.Text}')");
                DictionaryProvider.SaveStyleInDB(Preview.Resources, SelectedItem.Text);
            }
        }

        private void EditableItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeFrame.NavigationService.RemoveBackEntry();
            switch ((string)(EditableItem.SelectedItem as ComboBoxItem).Content)
            {
                case "Фон":
                    CurrentEditor = new BackgroundEditor(Preview);
                    ChangeFrame.Navigate(CurrentEditor);
                    break;

                case "Поле ввода":
                    CurrentEditor = new TextBoxEditor(Preview);
                    ChangeFrame.Navigate(CurrentEditor);
                    break;

                case "Кнопка":
                    CurrentEditor = new ButtonEditor(Preview);
                    ChangeFrame.Navigate(CurrentEditor);
                    break;

                case "Вкладка":
                    CurrentEditor = new TabItemEditor(Preview);
                    ChangeFrame.Navigate(CurrentEditor);
                    break;

                case "Список":
                    CurrentEditor = new ComboBoxEditor(Preview);
                    ChangeFrame.Navigate(CurrentEditor);
                    break;
            }
        }
    }
}
