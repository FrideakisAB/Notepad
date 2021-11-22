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
using Microsoft.Data.Sqlite;

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

        public delegate void UpdateStyle(ResourceDictionary dictionary);
        public static event UpdateStyle UpdateStyleObservers;
        public static ResourceDictionary CurrentDictionary;

        private IEditorPage currentEditor = null;

        public SelectStyle()
        {
            InitializeComponent();

            previewFrame.Navigate(preview);

            SqliteDataReader reader = DBContext.CommandReader("SELECT styleName FROM stylesNames");

            if (!reader.HasRows)
            {
                //TODO: сделать скрипт для добавления стандартных тем
                //TODO: запустить скрипт для добавления стандартных тем
            }

            if (reader.HasRows)
            {
                while (reader.Read())
                    StyleList.Items.Add(new StyleItem(reader.GetString(0), this));
            }

            UpdateStyleObservers += SelectStyle_UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, CurrentDictionary);

            editableItem.SelectedIndex = 0;
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
            Item?.DeleteElement();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            CreateStyle createStyle = new CreateStyle(this);
            createStyle.Owner = this;
            createStyle.ShowDialog();
        }

        private void RenameStyle_Click(object sender, RoutedEventArgs e)
        {
            Item?.RequestRename();
        }

        public void Notify(ResourceDictionary dictionary)
        {
            CurrentDictionary = dictionary;
            UpdateStyleObservers?.Invoke(dictionary);
        }

        private void StyleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Item != null)
            {
                DictionaryProvider.LoadStyleFromDB(preview.Resources, Item.Text);
                currentEditor?.ChangePreview();
            }
        }

        private void ApplyChangedStyle_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
                Notify(preview.Resources);
        }

        private void SaveChangedStyle_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                DBContext.Command($"DELETE FROM styles WHERE styleNameId = (SELECT styleNameId FROM stylesNames WHERE styleName = '{Item.Text}')");
                DictionaryProvider.SaveStyleInDB(preview.Resources, Item.Text);
            }
        }

        private void EditableItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeFrame.NavigationService.RemoveBackEntry();
            switch ((string)(editableItem.SelectedItem as ComboBoxItem).Content)
            {
                case "Фон":
                    currentEditor = new BackgroundEditor(preview);
                    changeFrame.Navigate(currentEditor);
                    break;

                case "Поле ввода":
                    break;

                case "Кнопка":
                    break;

                case "Вкладка":
                    currentEditor = new TabItemEditor(preview);
                    changeFrame.Navigate(currentEditor);
                    break;

                case "Список":
                    break;
            }
        }
    }
}
