using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace DZNotepad.UserElements
{
    /// <summary>
    /// Логика взаимодействия для EditableFile.xaml
    /// </summary>

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

    public partial class EditableFile : UserControl
    {
        public string FileName { get => fileName; }
        public TextBox TextSource { get => textSource; }
        public bool IsEditable { get => isEditable; }

        private CloseableTab parentTab;
        private FileInfoBlock fileInfoBlock;
        private TranslateInfoBlock translateInfoBlock;

        private static Encoding UTF32BE = new UTF32Encoding(true, true);
        private string fileName = "";
        private string header = "Без имени";
        private bool isSupportedFormat = true;
        private bool isEditable = false;
        private DateTime lastWriteAcces;
        private int translatePosition = 0;
        private string encoding = Encoding.Default.BodyName.ToUpper();
        private Encoding srcEncoding = Encoding.Default;
        private Encoding dstEncoding = Encoding.Default;
        private LastFiles lastFiles;

        public EditableFile(LastFiles lastFiles, CloseableTab tab, FileInfoBlock fileBblock, TranslateInfoBlock translateBlock)
        {
            InitializeComponent();
            DataContext = this;
            this.lastFiles = lastFiles;

            parentTab = tab;
            parentTab.Closed += parentTab_TabClosed;
            parentTab.SetHeader(header);

            fileInfoBlock = fileBblock;
            translateInfoBlock = translateBlock;
            UpdateState();
        }

        public EditableFile(LastFiles lastFiles, CloseableTab tab, FileInfoBlock fileBblock, TranslateInfoBlock translateBlock, string path) : this(lastFiles, tab, fileBblock, translateBlock)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly) ||
                File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint) ||
                Path.GetExtension(path) != ".txt")
            {
                isSupportedFormat = false;
                textSource.IsReadOnly = true;
            }

            fileName = path;
            lastWriteAcces = File.GetLastWriteTime(fileName);
            header = Path.GetFileName(fileName);

            using (FileStream fs = File.OpenRead(fileName))
            {
                Ude.CharsetDetector detector = new Ude.CharsetDetector();
                detector.Feed(fs);
                detector.DataEnd();
                if (detector.Charset != null)
                    encoding = detector.Charset;
            }

            srcEncoding = ToSystemEncoding(encoding);

            textSource.Text = File.ReadAllText(fileName, srcEncoding);
            isEditable = false;

            parentTab.SetHeader(header);
            UpdateState();
        }

        private static Encoding ToSystemEncoding(string encoding)
        {
            Encoding sysEncoding = Encoding.Default;

            switch (encoding)
            {
                case "ASCII":
                    sysEncoding = Encoding.ASCII;
                    break;
                case "UTF-8":
                    sysEncoding = Encoding.UTF8;
                    break;
                case "UTF-16LE":
                    sysEncoding = Encoding.Unicode;
                    break;
                case "UTF-16BE":
                    sysEncoding = Encoding.BigEndianUnicode;
                    break;
                case "UTF-32LE":
                    sysEncoding = Encoding.UTF32;
                    break;
                case "UTF-32BE":
                    sysEncoding = UTF32BE;
                    break;
                case "windows-1251":
                    sysEncoding = Encoding.GetEncoding(1251);
                    break;
            }

            return sysEncoding;
        }

        private static string SystemEncodingToString(Encoding encoding)
        {
            string sysEncoding = Encoding.Default.BodyName.ToUpper();

            if (encoding.CodePage == Encoding.ASCII.CodePage)
                sysEncoding = "ASCII";
            else if (encoding.CodePage == Encoding.UTF8.CodePage)
                sysEncoding = "UTF-8";
            else if (encoding.CodePage == Encoding.Unicode.CodePage)
                sysEncoding = "UTF-16LE";
            else if (encoding.CodePage == Encoding.BigEndianUnicode.CodePage)
                sysEncoding = "UTF-16BE";
            else if (encoding.CodePage == Encoding.UTF32.CodePage)
                sysEncoding = "UTF-32LE";
            else if (encoding.CodePage == UTF32BE.CodePage)
                sysEncoding = "UTF-32BE";
            else if (encoding.CodePage == Encoding.GetEncoding(1251).CodePage)
                sysEncoding = "windows-1251";

            return sysEncoding;
        }

        private void UpdateState()
        {
            if (this == fileInfoBlock.CurrentFile)
            {
                string[] lines = textSource.Text.Split('\n');

                fileInfoBlock.LineCount.Text = lines.Length.ToString();
                fileInfoBlock.CharCount.Text = textSource.Text.Length.ToString();

                int totalCount = 0, line = 0, charIndex = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    charIndex = textSource.CaretIndex - totalCount - i;
                    totalCount += lines[i].Length;
                    if (totalCount >= textSource.CaretIndex - i)
                    {
                        line = i + 1;
                        break;
                    }
                }

                fileInfoBlock.CurrentLine.Text = line.ToString();
                fileInfoBlock.CurrentChar.Text = charIndex.ToString();
                fileInfoBlock.CurrentEncode.Text = encoding;
            }
        }

        private void textSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEditable = true;
            parentTab.SetHeader(header + "*");
            UpdateState();

            int changeOffset = -1;
            foreach (var change in e.Changes)
                changeOffset = change.Offset;

            if (translateInfoBlock.IsAutoTranslate)
            {
                int indexComma = textSource.Text.LastIndexOf('.');
                int indexExclamation = textSource.Text.LastIndexOf('!');
                int indexQuestion = textSource.Text.LastIndexOf('?');

                if (indexComma != -1 || indexExclamation != -1 || indexQuestion != -1)
                {
                    int index = -1;
                    if (indexComma != -1) index = indexComma;
                    if (indexExclamation != -1 && indexExclamation > index) index = indexExclamation;
                    if (indexQuestion != -1 && indexQuestion > index) index = indexQuestion;

                    if (index != -1 && (index > translatePosition || changeOffset != -1 && changeOffset < translatePosition))
                    {
                        int relativeCursor = textSource.CaretIndex - index;
                        string translateSuggestion = translateInfoBlock.LocalTranslator.Translate("russian", translateInfoBlock.Language, textSource.Text.Substring(0, index));
                        textSource.Text = translateSuggestion + textSource.Text.Substring(index);
                        translatePosition = translateSuggestion.Length;
                        textSource.CaretIndex = translatePosition + relativeCursor;
                    }
                }
            }
        }

        private void parentTab_TabClosed(object sender, EventTabClosedArgs e)
        {
            if (isEditable)
                e.PreventDefault = SaveChangesDialog(GetSaveMessage());
        }

        private void textSource_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private bool SaveChangesDialog(string text)
        {
            if (isSupportedFormat)
            {
                MessageBoxResult result = MessageBox.Show(text, "Сохранение", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                    OnSave();
                else if (result == MessageBoxResult.Cancel)
                    return true;
            }

            return false;
        }

        internal bool OnWindowCLosing()
        {
            if (isEditable)
               return SaveChangesDialog(GetSaveMessage());

            return false;
        }

        private string GetSaveMessage()
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return $"Сохранить новый файл?";
            else
                return $"Сохранить изменения ({header})?";
        }

        internal bool OnClose()
        {
            if (isEditable)
                return SaveChangesDialog(GetSaveMessage());

            return false;
        }

        internal void OnSave()
        {
            if (isSupportedFormat)
            {
                if (string.IsNullOrWhiteSpace(fileName))
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
                SecureFileDialog dlg = new SecureFileDialog(UserSingleton.RootPath, "Выберите расположение...", SecureFileDialogType.Save);
                result = (bool)dlg.ShowDialog();
                dlgFileName = dlg.FileName;
            }
            else
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text Files (*.txt)|*.txt";

                result = (bool)dlg.ShowDialog();
                dlgFileName = dlg.FileName;
            }


            if (result == true)
            {
                File.WriteAllText(dlgFileName, textSource.Text);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = dlgFileName;
                    header = Path.GetFileName(fileName);

                    SaveFile();
                }
            }
        }

        private void SaveFile()
        {
            if (UserSingleton.Get().LoginUser != null)
                DBContext.Command($"INSERT INTO fileHistory (filePath, userId, changeTime) VALUES ('{fileName}', {UserSingleton.Get().LoginUser.UserId}, CURRENT_TIMESTAMP)");

            isEditable = false;
            File.WriteAllText(fileName, textSource.Text, dstEncoding);
            lastFiles.RegisterNewFile(fileName);
            lastWriteAcces = File.GetLastWriteTime(fileName);
            parentTab.SetHeader(header);

            srcEncoding = dstEncoding;
            encoding = SystemEncodingToString(srcEncoding);
            UpdateState();
        }

        internal void OnReOpen()
        {
            if (isEditable)
            {
                bool isCancel = SaveChangesDialog("Есть не сохранённые изменения (" + header + "), вы хотите их сохранить прежде чем открыть файл заново?");
                if (!isCancel)
                    textSource.Text = File.ReadAllText(fileName);
            }
        }

        internal void OnCopyCommand()
        {
            Clipboard.SetText(textSource.SelectedText);
        }

        internal void OnPasteCommand()
        {
            if (isSupportedFormat)
            {
                int insertPosition = textSource.CaretIndex;
                if (textSource.SelectionLength != 0)
                {
                    insertPosition = textSource.SelectionStart;
                    textSource.Text = textSource.Text.Remove(insertPosition, textSource.SelectionLength);
                }

                textSource.Text.Insert(insertPosition, Clipboard.GetText());
            }
        }

        internal void OnCutCommand()
        {
            if (isSupportedFormat)
            {
                if (textSource.SelectionLength != 0)
                {
                    Clipboard.SetText(textSource.SelectedText);
                    textSource.Text = textSource.Text.Remove(textSource.SelectionStart, textSource.SelectionLength);
                }
            }
        }

        internal void OnChangeFontSize(int fontSize)
        {
            textSource.FontSize = fontSize;
        }

        internal void OnActivated()
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Файл был удалён", "Файл", MessageBoxButton.OK, MessageBoxImage.Information);
                    isEditable = true;
                    fileName = "";
                    parentTab.SetHeader(header + "*");
                    header = "Без имени";
                }
                else if (File.GetLastWriteTime(fileName) > lastWriteAcces)
                {
                    MessageBoxResult result = MessageBox.Show("Файл был изменён вне, вы хотите его обноаить (текущие изменения потеряются)?", "Файл", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        textSource.Text = File.ReadAllText(fileName);
                        isEditable = false;
                        parentTab.SetHeader(header);
                    }
                    else
                    {
                        isEditable = true;
                        parentTab.SetHeader(header + "*");
                    }

                    lastWriteAcces = File.GetLastWriteTime(fileName);
                }
            }
            UpdateState();
        }

        internal void OnTranslateRequest()
        {
            if (translatePosition != textSource.Text.Length)
            {
                textSource.Text = translateInfoBlock.LocalTranslator.Translate("russian", translateInfoBlock.Language, textSource.Text);
                translatePosition = textSource.Text.Length;
            }
        }

        internal void OnStyleChange(Style style)
        {
            textSource.Style = style;
        }

        internal void OnChangeEncoding(string encoding)
        {
            Encoding sysEncoding = ToSystemEncoding(encoding);
            if (dstEncoding.CodePage != sysEncoding.CodePage && isSupportedFormat)
            {
                dstEncoding = sysEncoding;
                isEditable = true;
                parentTab.SetHeader(header + "*");
            }
        }
    }
}
