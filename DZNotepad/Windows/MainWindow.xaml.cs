using System;
using System.IO;
using LiveCharts;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DZNotepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AboutProgram AboutProgramWindow;
        private SelectStyle SelectStyleWindow;
        private WindowHelp HelpWindow;
        private FindWindow FindWindow;
        private ReplaceWindow ReplaceWindow;

        private Translator Translator = new Translator();
        private PerformanceAnalyser Analyser;

        private FileInfoBlock FileInfoBlockObject = new FileInfoBlock();
        private TranslateInfoBlock TranslateInfoBlockObject = new TranslateInfoBlock();
        private LastFiles LastFilesObject;

        private FileTabManager TabManager;
        private ConfigParser Config;

        public ChartValues<double> CPUUsage { get; set; }
        public ChartValues<double> MemoryUsage { get; set; }
        public ChartValues<double> NetworkUsage { get; set; }

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            SelectStyle.CurrentDictionary = Resources;
            DataContext = this;

            CPUUsage = new ChartValues<double>(new double[120]);
            MemoryUsage = new ChartValues<double>(new double[120]);
            NetworkUsage = new ChartValues<double>(new double[120]);

            Analyser = new PerformanceAnalyser(PerformanceChart, CPUUsage, MemoryUsage, NetworkUsage);

            FileInfoBlockObject.LineCount = LineCount;
            FileInfoBlockObject.CharCount = CharCount;
            FileInfoBlockObject.CurrentLine = LineCurrent;
            FileInfoBlockObject.CurrentChar = CharCurrent;
            FileInfoBlockObject.CurrentEncode = EncodingText;

            TranslateInfoBlockObject.LocalTranslator = Translator;
            TranslateInfoBlockObject.IsAutoTranslate = false;
            TranslateInfoBlockObject.Language = string.Empty;

            LastFilesObject = new LastFiles();

            LastFiles_OnAddFile(LastFilesObject, new EventArgs());

            LastFilesObject.OnAddFile += LastFiles_OnAddFile;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;

            TabManager = new FileTabManager(TabsContainer, LastFilesObject, FileInfoBlockObject, TranslateInfoBlockObject);
            TabManager.CreateNewTab();

            Config = new ConfigParser();

            ParseConfig();
        }

        ~MainWindow()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(Resources, dictionary);
        }

        private void ParseConfig()
        {
            Config.ParseFile(Path.Combine(Directory.GetCurrentDirectory(), "config.cfg"));
            Config.CreateLanguageFolders();
        }

        private void LastFiles_OnAddFile(object sender, EventArgs e)
        {
            lastFilesMenu.Items.Clear();

            string[] files = LastFilesObject.GetLastFiles();
            for (int i = files.Length - 1; i >= 0; i--)
            {
                MenuItem item = new MenuItem();
                item.Header = files[i];
                item.Click += LastFile_Click;

                lastFilesMenu.Items.Add(item);
            }
        }

        private void LastFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            string path = item.Header as string;

            if (path.Contains(UserSingleton.RootPath) || UserSingleton.Get().LoginUser == null)
                TabManager.CreateNewTab(path);
        }

        private void AboutProgram_Click(object sender, RoutedEventArgs e)
        {
            if (AboutProgramWindow != null)
                AboutProgramWindow.Close();
            AboutProgramWindow = new AboutProgram();
            AboutProgramWindow.Owner = this;
            AboutProgramWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (AboutProgramWindow != null)
                AboutProgramWindow.Close();
            if (FindWindow != null)
                FindWindow.Close();
            if (ReplaceWindow != null)
                ReplaceWindow.Close();
            if (HelpWindow != null)
                HelpWindow.Close();
            if (SelectStyleWindow != null)
                SelectStyleWindow.Close();

            Analyser.Dispose();
            LastFilesObject.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (EditableFile file in TabManager)
            {
                if (file.OnWindowClosing())
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool result;
            string fileName;

            if (UserSingleton.Get().LoginUser != null)
            {
                SecureFileDialog dlg = new SecureFileDialog(UserSingleton.RootPath, "Выберите файл...", SecureFileDialogType.Open);
                dlg.FilterIndex = 0;
                dlg.Filter = "Текстовый файл (*.txt)|*.txt|Все файлы (*.*)|*.*";
                result = (bool)dlg.ShowDialog();
                fileName = dlg.FileName;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FilterIndex = 0;
                dlg.Filter = "Текстовый файл (*.txt)|*.txt|Все файлы (*.*)|*.*";
                result = (bool)dlg.ShowDialog();
                fileName = dlg.FileName;
            }

            if (result == true)
            {
                bool isEmpty = false;
                if (TabsContainer.Items.Count == 1 && TabManager.SelectedFile.FileName == "" && !TabManager.SelectedFile.IsEditable)
                    isEmpty = true;

                TabManager.CreateNewTab(fileName);
                if (isEmpty)
                    TabManager.RemoveFileTab(TabManager.SelectedFile);
            }
        }

        private void SaveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.SelectedFile.OnSave();
        }

        private void SaveAsFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.SelectedFile.OnSaveAs();
        }

        private void NewFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.CreateNewTab();
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FindWindow != null)
                FindWindow.Close();
            FindWindow = new FindWindow(TabManager.SelectedFile, this);
            FindWindow.Owner = this;
            FindWindow.Show();
        }

        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ReplaceWindow != null)
                ReplaceWindow.Close();
            ReplaceWindow = new ReplaceWindow(TabManager.SelectedFile, this);
            ReplaceWindow.Owner = this;
            ReplaceWindow.Show();
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnCopyCommand();
        }

        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnPasteCommand();
        }

        private void ChangeSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            TabManager.SelectedFile.OnChangeFontSize(int.Parse((string)item.Header));
        }

        private void TabsContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FindWindow?.ChangeEditableFile(TabManager.SelectedFile);
            ReplaceWindow?.ChangeEditableFile(TabManager.SelectedFile);

            TabManager.SelectedFile?.OnActivated();

            FileInfoBlockObject.CurrentFile = TabManager.SelectedFile;
        }

        private void CutItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnCutCommand();
        }

        private void CloseFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!TabManager.SelectedFile.OnClose())
                TabManager.RemoveFileTab(TabManager.SelectedFile);
        }

        private void CloseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditableFile file in TabManager)
            {
                if (file.OnWindowClosing())
                    return;
            }

            TabManager.Clear();
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (EditableFile file in TabManager)
            {
                if (file.OnWindowClosing())
                    return;
            }

            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TabManager.SelectedFile.OnActivated();
        }

        private void StateLineItem_Click(object sender, RoutedEventArgs e)
        {
            if (StateLine.Visibility == Visibility.Hidden)
                StateLine.Visibility = Visibility.Visible;
            else
                StateLine.Visibility = Visibility.Hidden;
        }

        private void TranslateItem_Click(object sender, RoutedEventArgs e)
        {
            if (TranslateItem.IsChecked)
            {
                TranslateWindow.Width = 180;

                ParseConfig();

                string lastLanguage = LanguagesCombo.SelectedItem as string;
                LanguagesCombo.Items.Clear();

                foreach (string language in Config.Languages)
                {
                    LanguagesCombo.Items.Add(language);
                    if (LanguagesCombo.SelectedItem == null || language == lastLanguage)
                        LanguagesCombo.SelectedItem = language;
                }
            }
            else
                TranslateWindow.Width = 0;
        }

        private void AutoSlider_CheckedChange(object sender, RoutedEventArgs e)
        {
            TranslateInfoBlockObject.IsAutoTranslate = AutoTranslateState.IsChecked;
        }

        private void TranslateManual_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnTranslateRequest();
        }

        private void LanguagesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TranslateInfoBlockObject.Language = LanguagesCombo.SelectedItem as string;
        }

        private void PerformanceItem_Click(object sender, RoutedEventArgs e)
        {
            if (PerformanceItem.IsChecked)
            {
                PerformanceWindow.Height = 170;
                Height += 170;
            }
            else
            {
                Height -= 170;
                PerformanceWindow.Height = 0;
            }
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditableFile file in TabManager)
                file.OnSave();
        }

        private void StyleItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectStyleWindow != null)
                SelectStyleWindow.Close();
            SelectStyleWindow = new SelectStyle();
            SelectStyleWindow.Owner = this;
            SelectStyleWindow.Show();
        }

        private void WindowHelp_Click(object sender, RoutedEventArgs e)
        {
            if (HelpWindow != null)
                AboutProgramWindow.Close();
            HelpWindow = new WindowHelp();
            HelpWindow.Owner = this;
            HelpWindow.Show();
        }

        private void ChangeEncoding_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            TabManager.SelectedFile.OnChangeEncoding((string)item.Header);
        }

        private void BtnSingIn_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(LoginBox.Text))
            {
                if (!String.IsNullOrEmpty(PassBox.Password))
                {
                    var result = DBContext.Entitys.Users.Where((userLocal) => userLocal.Login == LoginBox.Text && userLocal.Password == PassBox.Password);
                    if (result.Count() != 0)
                    {
                        UserSingleton.Get().LoginUser = result.First();
                        MessageBox.Show("Добро пожаловать " + LoginBox.Text);

                        LoginWindow.Width = 0;
                        LoginItem.IsChecked = false;
                    }
                    else
                        MessageBox.Show("Неверный логин или пароль!");
                }
            }
            else
                MessageBox.Show("Проверьте все ли поля заполнены!");
        }

        private void LoginItem_Click(object sender, RoutedEventArgs e)
        {
            if (LoginItem.IsChecked)
                LoginWindow.Width = 200;
            else
                LoginWindow.Width = 0;
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            if (UserSingleton.Get().LoginUser != null &&  MessageBox.Show("Вы уверены что хотите выйти из учётной записи?", "Выход из учётной записи", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                UserSingleton.Get().LoginUser = null;
        }
    }
    public class WindowCommands
    {
        static WindowCommands()
        {
            Exit = new RoutedCommand("Exit", typeof(MainWindow));
        }
        public static RoutedCommand Exit { get; set; }
    }  
}
