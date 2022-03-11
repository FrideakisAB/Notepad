using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DZNotepad.UserElements;
using LiveCharts;
using Microsoft.Data.Sqlite;

namespace DZNotepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AboutProgram aboutProgramWindow;
        SelectStyle selectStyle;
        WindowHelp windowHelp;
        FindWindow findWindow;
        ReplaceWindow replaceWindow;
        FileInfoBlock fileInfoBlock = new FileInfoBlock();
        TranslateInfoBlock translateInfoBlock = new TranslateInfoBlock();
        Translator translator = new Translator();
        PerformanceAnalyser analyser;
        LastFiles lastFiles;

        private FileTabManager TabManager;

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

            analyser = new PerformanceAnalyser(PerformanceChart, CPUUsage, MemoryUsage, NetworkUsage);

            fileInfoBlock.LineCount = lineCount;
            fileInfoBlock.CharCount = charCount;
            fileInfoBlock.CurrentLine = lineCurrent;
            fileInfoBlock.CurrentChar = charCurrent;
            fileInfoBlock.CurrentEncode = encoding;

            translateInfoBlock.LocalTranslator = translator;
            translateInfoBlock.IsAutoTranslate = false;
            translateInfoBlock.Language = string.Empty;

            lastFiles = new LastFiles();

            LastFiles_OnAddFile(lastFiles, new EventArgs());

            lastFiles.OnAddFile += LastFiles_OnAddFile;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;

            TabManager = new FileTabManager(tabsContainer, lastFiles, fileInfoBlock, translateInfoBlock);
            TabManager.CreateNewTab();

            // Создание подпапок
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "russian"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "english"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "german"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "french"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "bulgarian"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, "polish"));
        }

        private void LastFiles_OnAddFile(object sender, EventArgs e)
        {
            lastFilesMenu.Items.Clear();

            string[] files = lastFiles.GetLastFiles();
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

        ~MainWindow()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(Resources, dictionary);
        }

        private void aboutProgram_Click(object sender, RoutedEventArgs e)
        {
            if (aboutProgramWindow != null)
                aboutProgramWindow.Close();
            aboutProgramWindow = new AboutProgram();
            aboutProgramWindow.Owner = this;
            aboutProgramWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (aboutProgramWindow != null)
                aboutProgramWindow.Close();
            if (findWindow != null)
                findWindow.Close();
            if (replaceWindow != null)
                replaceWindow.Close();
            if (windowHelp != null)
                windowHelp.Close();
            if (selectStyle != null)
                selectStyle.Close();

            analyser.Dispose();
            lastFiles.Dispose();
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

        private void openFile_Executed(object sender, ExecutedRoutedEventArgs e)
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
                if (tabsContainer.Items.Count == 1 && TabManager.SelectedFile.FileName == "" && !TabManager.SelectedFile.IsEditable)
                    isEmpty = true;

                TabManager.CreateNewTab(fileName);
                if (isEmpty)
                    tabsContainer.Items.Remove(TabManager.SelectedFile);
            }
        }

        private void saveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.SelectedFile.OnSave();
        }

        private void saveAsFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.SelectedFile.OnSaveAs();
        }

        private void newFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabManager.CreateNewTab();
        }

        private void findCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (findWindow != null)
                findWindow.Close();
            findWindow = new FindWindow(((EditableFile)(tabsContainer.SelectedItem as CloseableTab).Content), this);
            findWindow.Owner = this;
            findWindow.Show();
        }

        private void replaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (replaceWindow != null)
                replaceWindow.Close();
            replaceWindow = new ReplaceWindow(((EditableFile)(tabsContainer.SelectedItem as CloseableTab).Content), this);
            replaceWindow.Owner = this;
            replaceWindow.Show();
        }

        private void copyItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnCopyCommand();
        }

        private void pasteItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnPasteCommand();
        }

        private void changeSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            TabManager.SelectedFile.OnChangeFontSize(int.Parse((string)item.Header));
        }

        private void tabsContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            findWindow?.ChangeEditableFile(TabManager.SelectedFile);
            replaceWindow?.ChangeEditableFile(TabManager.SelectedFile);

            TabManager.SelectedFile?.OnActivated();

            fileInfoBlock.CurrentFile = TabManager.SelectedFile;
        }

        private void cutItem_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnCutCommand();
        }

        private void closeFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!TabManager.SelectedFile.OnClose())
                TabManager.RemoveFileTab(TabManager.SelectedFile);
        }

        private void closeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditableFile file in TabManager)
            {
                if (file.OnWindowClosing())
                    return;
            }

            TabManager.Clear();
        }

        private void exitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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

        private void stateLineItem_Click(object sender, RoutedEventArgs e)
        {
            if (stateLine.Visibility == Visibility.Hidden)
                stateLine.Visibility = Visibility.Visible;
            else
                stateLine.Visibility = Visibility.Hidden;
        }

        private void translateItem_Click(object sender, RoutedEventArgs e)
        {
            if (translateItem.IsChecked)
            {
                translateWindow.Width = 180;

                if (File.Exists(Directory.GetCurrentDirectory() + "\\config.cfg"))
                {
                    string[] configLines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\config.cfg");

                    string lastLanguage = languagesCombo.SelectedItem as string;
                    languagesCombo.Items.Clear();

                    for (int i = 0; i < configLines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(configLines[i]) || configLines[i][0] == ';')
                            continue;

                        string language = configLines[i].Trim();

                        if (language.IndexOf(';') != -1)
                            language = language.Remove(language.IndexOf(';'));

                        if (translator.IsValidLanguage(language))
                        {
                            languagesCombo.Items.Add(language);

                            if (languagesCombo.SelectedItem == null || language == lastLanguage)
                                languagesCombo.SelectedItem = language;
                        }
                        else
                            MessageBox.Show($"Во время разбора файла конфигурации произошла ошибка! В строке {i + 1} указан не поддерживаемый язык, либо неправильно его описание", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    string startupMessage = "; Файл конфигурации\n" +
                        "\n" +
                        "; Строка начинающаяся с ; является комментарием\n" +
                        "; Для указания языка необходимо прописать его название (официальное) в новой строке как показано ниже\n" +
                        "; (регистр не важен)\n" +
                        "\n" +
                        "english\n" +
                        "german\n" +
                        "french\n" +
                        "bulgarian\n" +
                        "polish\n" +
                        "\n" +
                        "; Доступные языки:\n" +
                        "\n";

                    foreach (var pair in translator.LanguagesWithISO)
                        startupMessage += "; " + pair.Key + "\n";

                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\config.cfg", startupMessage);

                    languagesCombo.Items.Add("english");
                    languagesCombo.Items.Add("german");
                    languagesCombo.Items.Add("french");
                    languagesCombo.Items.Add("bulgarian");
                    languagesCombo.Items.Add("polish");
                }
            }
            else
                translateWindow.Width = 0;
        }

        private void autoSlider_CheckedChange(object sender, RoutedEventArgs e)
        {
            translateInfoBlock.IsAutoTranslate = autoTranslateState.IsChecked;
        }

        private void translateManual_Click(object sender, RoutedEventArgs e)
        {
            TabManager.SelectedFile.OnTranslateRequest();
        }

        private void languagesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            translateInfoBlock.Language = languagesCombo.SelectedItem as string;
        }

        private void performanceItem_Click(object sender, RoutedEventArgs e)
        {
            if (performanceItem.IsChecked)
            {
                performanceWindow.Height = 170;
                Height += 170;
            }
            else
            {
                Height -= 170;
                performanceWindow.Height = 0;
            }
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (EditableFile file in TabManager)
                file.OnSave();
        }

        private void styleItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectStyle != null)
                selectStyle.Close();
            selectStyle = new SelectStyle();
            selectStyle.Owner = this;
            selectStyle.Show();
        }

        private void WindowHelp_Click(object sender, RoutedEventArgs e)
        {
            if (windowHelp != null)
                aboutProgramWindow.Close();
            windowHelp = new WindowHelp();
            windowHelp.Owner = this;
            windowHelp.Show();
        }

        private void ChangeEncoding_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            TabManager.SelectedFile.OnChangeEncoding((string)item.Header);
        }

        private void btnSingIn_Click(object sender, RoutedEventArgs e)
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
