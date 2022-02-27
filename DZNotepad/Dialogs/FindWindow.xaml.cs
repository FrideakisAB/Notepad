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
using DZNotepad.UserElements;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        MainWindow FileWindow;
        int CurrentIndex = 0;
        EditableFile FileEntry;

        public FindWindow(EditableFile file, MainWindow window)
        {
            InitializeComponent();
            FileEntry = file;
            FileWindow = window;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);
        }

        ~FindWindow()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }

        public void ChangeEditableFile(EditableFile file)
        {
            CurrentIndex = 0;
            FileEntry = file;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SearchText.Text = string.Empty;
            CurrentIndex = 0;
        }

        private void PrevFind_Click(object sender, RoutedEventArgs e)
        {
            CurrentIndex = FileEntry.TextSource.Text.LastIndexOf(SearchText.Text, CurrentIndex);

            if (CurrentIndex != -1)
            {
                FileEntry.TextSource.Select(CurrentIndex, SearchText.Text.Length);
                --CurrentIndex;
                if (CurrentIndex == -1)
                    CurrentIndex = FileEntry.TextSource.Text.Length - 1;
                FileWindow.Focus();
            }
            else
                CurrentIndex = FileEntry.TextSource.Text.Length - 1;
        }

        private void NextFind_Click(object sender, RoutedEventArgs e)
        {
            CurrentIndex = FileEntry.TextSource.Text.IndexOf(SearchText.Text, CurrentIndex);

            if (CurrentIndex == -1)
                CurrentIndex = FileEntry.TextSource.Text.IndexOf(SearchText.Text, 0);

            if (CurrentIndex == -1)
            {
                CurrentIndex = 0;
                return;
            }

            FileEntry.TextSource.Select(CurrentIndex, SearchText.Text.Length);
            ++CurrentIndex;
            FileWindow.Focus();
        }
    }
}
