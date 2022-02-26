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
    /// Логика взаимодействия для WindowHelp.xaml
    /// </summary>
    public partial class WindowHelp : Window
    {
        public WindowHelp()
        {
            InitializeComponent();

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);
        }

        ~WindowHelp()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }
    }
}
