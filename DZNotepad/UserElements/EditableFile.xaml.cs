using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DZNotepad
{
    public class FileInfoBlock
    {
        public TextBlock LineCount;
        public TextBlock CharCount;
        public TextBlock CurrentLine;
        public TextBlock CurrentChar;
        public TextBlock CurrentEncode;
        public EditableFile CurrentFile;
    }

    public class TranslateInfoBlock
    {
        public Translator LocalTranslator;
        public string Language;
        public bool IsAutoTranslate;
    }

    /// <summary>
    /// Логика взаимодействия для EditableFile.xaml
    /// </summary>
    public partial class EditableFile : UserControl
    {
        public string FileName { get; private set; } = string.Empty;
        public bool IsEditable { get; private set; } = false;

        private CloseableTab ParentTab;

        private LastFiles LastFilesObject;
        private FileInfoBlock FileInfoBlockObject;
        private TranslateInfoBlock TranslateInfoBlockObject;

        private string Header = "Без имени";
        private bool IsSupportedFormat = true;
        private DateTime LastWriteAcces;
        private int TranslatePosition = 0;

        private string EncodingString = Encoding.Default.BodyName.ToUpper();
        private Encoding SrcEncoding = Encoding.Default;
        private Encoding DstEncoding = Encoding.Default;

        public EditableFile(LastFiles lastFiles, CloseableTab tab, FileInfoBlock fileBlock, TranslateInfoBlock translateBlock)
        {
            InitializeComponent();
            DataContext = this;

            ParentTab = tab;
            ParentTab.Closed += ParentTab_TabClosed;
            ParentTab.SetHeader(Header);

            LastFilesObject = lastFiles;
            FileInfoBlockObject = fileBlock;
            TranslateInfoBlockObject = translateBlock;
            
            UpdateState();
        }

        public EditableFile(LastFiles lastFiles, CloseableTab tab, FileInfoBlock fileBblock, TranslateInfoBlock translateBlock, string path) : this(lastFiles, tab, fileBblock, translateBlock)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly) ||
                File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint) ||
                Path.GetExtension(path) != ".txt")
            {
                IsSupportedFormat = false;
                TextSource.IsReadOnly = true;
            }

            FileName = path;
            LastWriteAcces = File.GetLastWriteTime(FileName);
            Header = Path.GetFileName(FileName);

            using (FileStream fs = File.OpenRead(FileName))
            {
                Ude.CharsetDetector detector = new Ude.CharsetDetector();
                detector.Feed(fs);
                detector.DataEnd();
                if (detector.Charset != null)
                    EncodingString = detector.Charset;
            }

            SrcEncoding = EncodingUtils.StringToSystemEncoding(EncodingString);

            TextSource.Text = File.ReadAllText(FileName, SrcEncoding);
            IsEditable = false;

            ParentTab.SetHeader(Header);
            UpdateState();
        }

        private void UpdateState()
        {
            if (this == FileInfoBlockObject.CurrentFile)
            {
                string[] lines = TextSource.Text.Split('\n');

                FileInfoBlockObject.LineCount.Text = lines.Length.ToString();
                FileInfoBlockObject.CharCount.Text = TextSource.Text.Length.ToString();

                int totalCount = 0, line = 0, charIndex = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    charIndex = TextSource.CaretIndex - totalCount - i;
                    totalCount += lines[i].Length;
                    if (totalCount >= TextSource.CaretIndex - i)
                    {
                        line = i + 1;
                        break;
                    }
                }

                FileInfoBlockObject.CurrentLine.Text = line.ToString();
                FileInfoBlockObject.CurrentChar.Text = charIndex.ToString();
                FileInfoBlockObject.CurrentEncode.Text = EncodingString;
            }
        }

        private bool SaveChangesDialog(string text)
        {
            if (IsSupportedFormat)
            {
                MessageBoxResult result = MessageBox.Show(text, "Сохранение", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                    OnSave();
                else if (result == MessageBoxResult.Cancel)
                    return true;
            }

            return false;
        }

        private void TextSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsEditable = true;
            ParentTab.SetHeader(Header + "*");
            UpdateState();

            int changeOffset = -1;
            foreach (var change in e.Changes)
                changeOffset = change.Offset;

            if (TranslateInfoBlockObject.IsAutoTranslate)
            {
                int indexComma = TextSource.Text.LastIndexOf('.');
                int indexExclamation = TextSource.Text.LastIndexOf('!');
                int indexQuestion = TextSource.Text.LastIndexOf('?');

                if (indexComma != -1 || indexExclamation != -1 || indexQuestion != -1)
                {
                    int index = -1;
                    if (indexComma != -1) index = indexComma;
                    if (indexExclamation != -1 && indexExclamation > index) index = indexExclamation;
                    if (indexQuestion != -1 && indexQuestion > index) index = indexQuestion;

                    if (index != -1 && (index > TranslatePosition || changeOffset != -1 && changeOffset < TranslatePosition))
                    {
                        int relativeCursor = TextSource.CaretIndex - index;
                        string translateSuggestion = TranslateInfoBlockObject.LocalTranslator.Translate("russian", TranslateInfoBlockObject.Language, TextSource.Text.Substring(0, index));
                        TextSource.Text = translateSuggestion + TextSource.Text.Substring(index);
                        TranslatePosition = translateSuggestion.Length;
                        TextSource.CaretIndex = TranslatePosition + relativeCursor;
                    }
                }
            }
        }

        private void TextSource_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void ParentTab_TabClosed(object sender, EventTabClosedArgs e)
        {
            if (IsEditable)
                e.PreventDefault = SaveChangesDialog(GetSaveMessage());
        }

        private string GetSaveMessage()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                return $"Сохранить новый файл?";
            else
                return $"Сохранить изменения ({Header})?";
        }

        private void SaveFile()
        {
            if (UserSingleton.Get().LoginUser != null)
            {
                DBContext.Command($"INSERT INTO fileHistory (filePath, userId, changeTime) VALUES ('{FileName}', {UserSingleton.Get().LoginUser.UserId}, CURRENT_TIMESTAMP)");

                long value = (long)DBContext.CommandScalar($"SELECT COUNT(*) FROM mainFiles WHERE path = '{FileName}';");

                if (value == 0)
                {
                    long id;
                    try
                    {
                        id = (long)DBContext.CommandScalar($"SELECT MAX(fileId) FROM mainFiles") + 1;
                    }
                    catch
                    {
                        id = 1;
                    }

                    DBContext.Command($"INSERT INTO mainFiles VALUES ({id},'{FileName}');");
                }
            }

            IsEditable = false;
            File.WriteAllText(FileName, TextSource.Text, DstEncoding);
            LastFilesObject.RegisterNewFile(FileName);
            LastWriteAcces = File.GetLastWriteTime(FileName);
            ParentTab.SetHeader(Header);

            SrcEncoding = DstEncoding;
            EncodingString = EncodingUtils.SystemEncodingToString(SrcEncoding);
            UpdateState();
        }

        // Menu events
        internal bool OnWindowClosing()
        {
            if (IsEditable)
               return SaveChangesDialog(GetSaveMessage());

            return false;
        }

        internal bool OnClose()
        {
            if (IsEditable)
                return SaveChangesDialog(GetSaveMessage());

            return false;
        }

        internal void OnSave()
        {
            if (IsSupportedFormat)
            {
                if (string.IsNullOrWhiteSpace(FileName))
                    OnSaveAs();
                else
                    SaveFile();
            }
        }

        internal void OnSaveAs()
        {
            bool result;
            string dlgFileName;
            if (UserSingleton.Get().LoginUser != null)
            {
                SecureFileDialog dlg = new SecureFileDialog(Path.Combine(UserSingleton.RootPath, "russian"), "Выберите расположение...", SecureFileDialogType.Save);
                dlg.Filter = "Текстовый файл (*.txt)|*.txt";

                result = (bool)dlg.ShowDialog();
                dlgFileName = dlg.FileName;
            }
            else
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.Filter = "Текстовый файл (*.txt)|*.txt";

                result = (bool)dlg.ShowDialog();
                dlgFileName = dlg.FileName;
            }

            if (result == true)
            {
                File.WriteAllText(dlgFileName, TextSource.Text);

                if (string.IsNullOrWhiteSpace(FileName))
                {
                    FileName = dlgFileName;
                    Header = Path.GetFileName(FileName);

                    SaveFile();
                }
            }
        }

        internal void OnReOpen()
        {
            if (IsEditable)
            {
                bool isCancel = SaveChangesDialog("Есть не сохранённые изменения (" + Header + "), вы хотите их сохранить прежде чем открыть файл заново?");
                if (!isCancel)
                    TextSource.Text = File.ReadAllText(FileName);
            }
        }

        internal void OnCopyCommand()
        {
            Clipboard.SetText(TextSource.SelectedText);
        }

        internal void OnPasteCommand()
        {
            if (IsSupportedFormat)
            {
                int insertPosition = TextSource.CaretIndex;
                if (TextSource.SelectionLength != 0)
                {
                    insertPosition = TextSource.SelectionStart;
                    TextSource.Text = TextSource.Text.Remove(insertPosition, TextSource.SelectionLength);
                }

                TextSource.Text.Insert(insertPosition, Clipboard.GetText());
            }
        }

        internal void OnCutCommand()
        {
            if (IsSupportedFormat)
            {
                if (TextSource.SelectionLength != 0)
                {
                    Clipboard.SetText(TextSource.SelectedText);
                    TextSource.Text = TextSource.Text.Remove(TextSource.SelectionStart, TextSource.SelectionLength);
                }
            }
        }

        internal void OnChangeFontSize(int fontSize)
        {
            TextSource.FontSize = fontSize;
        }

        internal void OnActivated()
        {
            if (!string.IsNullOrWhiteSpace(FileName))
            {
                if (!File.Exists(FileName))
                {
                    MessageBox.Show("Файл был удалён", "Файл", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditable = true;
                    FileName = "";
                    ParentTab.SetHeader(Header + "*");
                    Header = "Без имени";
                }
                else if (File.GetLastWriteTime(FileName) > LastWriteAcces)
                {
                    MessageBoxResult result = MessageBox.Show("Файл был изменён вне, вы хотите его обноаить (текущие изменения потеряются)?", "Файл", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        TextSource.Text = File.ReadAllText(FileName);
                        IsEditable = false;
                        ParentTab.SetHeader(Header);
                    }
                    else
                    {
                        IsEditable = true;
                        ParentTab.SetHeader(Header + "*");
                    }

                    LastWriteAcces = File.GetLastWriteTime(FileName);
                }
            }
            UpdateState();
        }

        internal void OnTranslateRequest()
        {
            if (TranslatePosition != TextSource.Text.Length)
            {
                string translate = TranslateInfoBlockObject.LocalTranslator.Translate("russian", TranslateInfoBlockObject.Language, TextSource.Text);
                TranslatePosition = TextSource.Text.Length;

                if (UserSingleton.Get().LoginUser != null)
                {
                    try
                    {
                        SqliteDataReader reader = DBContext.CommandReader($"SELECT fileId FROM mainFiles WHERE path = '{FileName}'");
                        if (reader.HasRows)
                        {
                            string filePath = FileName.Replace("russian", TranslateInfoBlockObject.Language);
                            File.WriteAllText(filePath, translate);
                            reader.Read();
                            DBContext.Command($"INSERT INTO translateFiles (fileId, translatePath, language) VALUES ({reader.GetInt64(0)}, '{filePath}', '{TranslateInfoBlockObject.Language}')");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("При создании файла произошла ошибка, попробуйте другое имя", "Ошибка создания", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                    TextSource.Text = translate;
            }
        }

        internal void OnChangeEncoding(string encoding)
        {
            Encoding sysEncoding = EncodingUtils.StringToSystemEncoding(encoding);
            if (DstEncoding.CodePage != sysEncoding.CodePage && IsSupportedFormat)
            {
                DstEncoding = sysEncoding;
                IsEditable = true;
                ParentTab.SetHeader(Header + "*");
            }
        }
    }
}
