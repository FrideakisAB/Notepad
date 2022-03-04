using System.Windows;

namespace DZNotepad
{
    /// <summary>
    /// Диалоговое окно с возможностью ввода одного текстового параметра и возможностью валидации
    /// </summary>
    public partial class OneFieldDialog : Window
    {
        /// <summary>
        /// Содержит введённое пользователем значение, в случае если <c>DialogResult = true</c>
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Название диалога
        /// </summary>
        public string InputName
        {
            set
            {
                InputNameLabel.Content = value;
            }
        }

        /// <summary>
        /// Делегат для валидации введённого пользователем значения
        /// </summary>
        /// <param name="input">Введённое пользователем значение</param>
        /// <returns>Результат проверки - 
        /// <para><c>MessageBoxResult.OK</c> успешное завершение диалога;</para>
        /// <para><c>MessageBoxResult.No</c> отклонение значения, ожидания повторного ввода;</para>
        /// <para><c>MessageBoxResult.Cancel</c> завершение диалога без возврата значения</para>
        /// </returns>
        public delegate MessageBoxResult Validator(string input);

        /// <summary>
        /// Содержит проверяющие валидаторы
        /// </summary>
        public Validator ValidatorCallback;

        public OneFieldDialog()
        {
            InitializeComponent();

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
            DictionaryProvider.ApplyDictionary(Resources, SelectStyle.CurrentDictionary);
        }

        ~OneFieldDialog()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(Resources, dictionary);
        }

        private void SaveName_Click(object sender, RoutedEventArgs e)
        {
            var isValid = ValidatorCallback?.Invoke(NameText.Text);
            if (isValid.HasValue)
            {
                switch (isValid.Value)
                {
                    case MessageBoxResult.OK:
                        break;

                    case MessageBoxResult.No:
                        return;

                    case MessageBoxResult.Cancel:
                        Close();
                        break;
                }
            }

            Result = NameText.Text;
            DialogResult = true;

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
