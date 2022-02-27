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

            CreateNewTab();

            // Создание подпапок
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "russian"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "english"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "german"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "french"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "bulgarian"));
            Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath + "\\..", "polish"));
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
                CreateNewTab(path);
        }

        ~MainWindow()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
            foreach (var tab in tabsContainer.Items)
                (tab as CloseableTab)?.SetStyle(this.Resources["AnyStyleTabItem"] as Style);
        }

        private void CreateNewTab(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                CloseableTab item = new CloseableTab();
                item.SetStyle(this.Resources["AnyStyleTabItem"] as Style);
                tabsContainer.Items.Add(item);
                EditableFile editableFile = new EditableFile(lastFiles, item, fileInfoBlock, translateInfoBlock);
                item.Content = editableFile;
                tabsContainer.SelectedItem = item;
            }
            else
            {
                CloseableTab targetTab = null;
                foreach (var tab in tabsContainer.Items)
                {
                    var closeableTab = tab as CloseableTab;
                    if (closeableTab != null && ((EditableFile)closeableTab.Content).FileName == path)
                        targetTab = closeableTab;
                }

                if (targetTab == null)
                {
                    CloseableTab item = new CloseableTab();
                    item.SetStyle(this.Resources["AnyStyleTabItem"] as Style);
                    tabsContainer.Items.Add(item);
                    EditableFile editableFile = new EditableFile(lastFiles, item, fileInfoBlock, translateInfoBlock, path);
                    item.Content = editableFile;
                    tabsContainer.SelectedItem = item;

                    lastFiles.RegisterNewFile(path);
                }
                else
                {
                    ((EditableFile)targetTab.Content).OnReOpen();
                    tabsContainer.SelectedItem = targetTab;
                }
            }
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
            foreach (var tab in tabsContainer.Items)
            {
                var closeableTab = tab as CloseableTab;
                if (closeableTab != null && ((EditableFile)closeableTab.Content).OnWindowCLosing())
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
                result = (bool)dlg.ShowDialog();
                fileName = dlg.FileName;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";
                result = (bool)dlg.ShowDialog();
                fileName = dlg.FileName;
            }

            if (result == true)
            {
                bool isEmpty = false;
                EditableFile editableFile = ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content);
                if (tabsContainer.Items.Count == 1 && editableFile.FileName == "" && !editableFile.IsEditable)
                    isEmpty = true;

                CreateNewTab(fileName);
                if (isEmpty)
                    tabsContainer.Items.Remove(editableFile);
            }
        }

        private void saveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnSave();
        }

        private void saveAsFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnSaveAs();
        }

        private void newFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CreateNewTab();
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
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnCopyCommand();
        }

        private void pasteItem_Click(object sender, RoutedEventArgs e)
        {
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnPasteCommand();
        }

        private void changeSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnChangeFontSize(int.Parse((string)item.Header));
        }

        private void tabsContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabsContainer.Items.IsEmpty)
            {
                CreateNewTab();
                tabsContainer.SelectedItem = tabsContainer.Items.GetItemAt(0);
            }

            findWindow?.ChangeEditableFile(((EditableFile)(tabsContainer.SelectedItem as CloseableTab).Content));
            replaceWindow?.ChangeEditableFile(((EditableFile)(tabsContainer.SelectedItem as CloseableTab).Content));

            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content)?.OnActivated();

            fileInfoBlock.CurrentFile = (EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content;
        }

        private void cutItem_Click(object sender, RoutedEventArgs e)
        {
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnCutCommand();
        }

        private void closeFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnClose())
                tabsContainer.Items.Remove(tabsContainer.SelectedItem);
        }

        private void closeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tab in tabsContainer.Items)
            {
                var closeableTab = tab as CloseableTab;
                if (closeableTab != null && ((EditableFile)closeableTab.Content).OnWindowCLosing())
                    return;
            }

            tabsContainer.Items.Clear();
        }

        private void exitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var tab in tabsContainer.Items)
            {
                var closeableTab = tab as CloseableTab;
                if (closeableTab != null && ((EditableFile)closeableTab.Content).OnWindowCLosing())
                    return;
            }

            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnActivated();
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
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnTranslateRequest();
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
            foreach (var tab in tabsContainer.Items)
            {
                var closeableTab = tab as CloseableTab;
                if (closeableTab != null)
                    ((EditableFile)closeableTab.Content).OnSave();
            }
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
            ((EditableFile)(tabsContainer.SelectedItem as CloseableTab)?.Content).OnChangeEncoding((string)item.Header);
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
