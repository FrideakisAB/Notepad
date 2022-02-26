using System.Windows;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для CreateStyle.xaml
    /// </summary>
    public partial class CreateFolder : Window
    {
        public string Name { get; private set; }

        public CreateFolder()
        {
            InitializeComponent();

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);
        }

        ~CreateFolder()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }

        private void SaveName_Click(object sender, RoutedEventArgs e)
        {
            //TODO: make check for correct
            Name = NameFolder.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
