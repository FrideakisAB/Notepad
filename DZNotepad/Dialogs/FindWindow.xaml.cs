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
        MainWindow mainWindow;
        int currentIndex = 0;
        EditableFile editableFile;

        public FindWindow(EditableFile file, MainWindow window)
        {
            InitializeComponent();
            editableFile = file;
            mainWindow = window;

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
            currentIndex = 0;
            editableFile = file;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            findetText.Text = string.Empty;
            currentIndex = 0;
        }

        private void prevFind_Click(object sender, RoutedEventArgs e)
        {
            currentIndex = editableFile.TextSource.Text.LastIndexOf(findetText.Text, currentIndex);

            if (currentIndex != -1)
            {
                editableFile.TextSource.Select(currentIndex, findetText.Text.Length);
                --currentIndex;
                if (currentIndex == -1)
                    currentIndex = editableFile.TextSource.Text.Length - 1;
                mainWindow.Focus();
            }
            else
                currentIndex = editableFile.TextSource.Text.Length - 1;
        }

        private void nextFind_Click(object sender, RoutedEventArgs e)
        {
            currentIndex = editableFile.TextSource.Text.IndexOf(findetText.Text, currentIndex);

            if (currentIndex == -1)
                currentIndex = 0;
            
            editableFile.TextSource.Select(currentIndex, findetText.Text.Length);
            ++currentIndex;
            mainWindow.Focus();
        }
    }
}
