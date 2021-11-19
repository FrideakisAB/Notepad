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
    /// Логика взаимодействия для ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        EditableFile editableFile;

        public ReplaceWindow(EditableFile file)
        {
            InitializeComponent();
            editableFile = file;

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
            editableFile = file;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            findetText.Text = string.Empty;
            replaceText.Text = string.Empty;
        }

        private void replace_Click(object sender, RoutedEventArgs e)
        {
            if (findetText.Text != string.Empty)
                editableFile.TextSource.Text = editableFile.TextSource.Text.Replace(findetText.Text, replaceText.Text);
        }
    }
}
