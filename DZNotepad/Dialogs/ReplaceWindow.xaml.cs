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
using System.Text.RegularExpressions;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        MainWindow FileWindow;
        private int CurrentIndex = 0;
        private EditableFile FileEntry;

        public ReplaceWindow(EditableFile file, MainWindow window)
        {
            InitializeComponent();
            FileEntry = file;
            FileWindow = window;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);
        }

        ~ReplaceWindow()
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
            ReplaceText.Text = string.Empty;
            CurrentIndex = 0;
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text != string.Empty)
            {
                FileEntry.TextSource.Text = ReplaceFirst(FileEntry.TextSource.Text, SearchText.Text, ReplaceText.Text, CurrentIndex - 1);

                if (FileEntry.TextSource.Text.Length <= CurrentIndex)
                    CurrentIndex = FileEntry.TextSource.Text.Length - 1;

                NextFind_Click(null, null);
            }
        }

        private string ReplaceFirst(string text, string search, string replace, int startPosition=0)
        {
            int pos = text.IndexOf(search, startPosition);
            if (pos < 0)
                return text;

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text != string.Empty)
                FileEntry.TextSource.Text = FileEntry.TextSource.Text.Replace(SearchText.Text, ReplaceText.Text);
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
