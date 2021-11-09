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
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private string fileName = "";
        private string header = "Без имени";
        private bool isSupportedFormat = true;
        private bool isEditable = false;
        private DateTime lastWriteAcces;
        private int translatePosition = 0;

        public EditableFile(CloseableTab tab, FileInfoBlock fileBblock, TranslateInfoBlock translateBlock)
        {
            InitializeComponent();
            this.DataContext = this;

            parentTab = tab;
            parentTab.Closed += parentTab_TabClosed;
            parentTab.SetHeader(header);

            fileInfoBlock = fileBblock;
            translateInfoBlock = translateBlock;
            updateState();
        }

        public EditableFile(CloseableTab tab, FileInfoBlock fileBblock, TranslateInfoBlock translateBlock, string path) : this(tab, fileBblock, translateBlock)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly) ||
                File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint) ||
                System.IO.Path.GetExtension(path) != ".txt")
            {
                isSupportedFormat = false;
                textSource.IsReadOnly = true;
            }

            fileName = path;
            lastWriteAcces = File.GetLastWriteTime(fileName);
            header = System.IO.Path.GetFileName(fileName);
            textSource.Text = File.ReadAllText(fileName);
            isEditable = false;
            parentTab.SetHeader(header);
            updateState();
        }

        private void updateState()
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
                fileInfoBlock.CurrentChar.Text = charIndex.ToString() ;
            }
        }

        private void textSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEditable = true;
            parentTab.SetHeader(header + "*");
            updateState();

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
                e.PreventDefault = saveChangesDialog(getSaveMessage());
        }

        private void textSource_SelectionChanged(object sender, RoutedEventArgs e)
        {
            updateState();
        }

        private bool saveChangesDialog(string text)
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
               return saveChangesDialog(getSaveMessage());

            return false;
        }

        private string getSaveMessage()
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return $"Сохранить новый файл?";
            else
                return $"Сохранить изменения ({header})?";
        }

        internal bool OnClose()
        {
            if (isEditable)
                return saveChangesDialog(getSaveMessage());

            return false;
        }

        internal void OnSave()
        {
            if (isSupportedFormat)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    OnSaveAs();
                else
                {
                    isEditable = false;
                    File.WriteAllText(fileName, textSource.Text);
                    lastWriteAcces = File.GetLastWriteTime(fileName);
                    parentTab.SetHeader(header);
                }
            }
        }

        internal void OnSaveAs()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                File.WriteAllText(dlg.FileName, textSource.Text);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    isEditable = false;
                    fileName = dlg.FileName;
                    header = System.IO.Path.GetFileName(fileName);
                    File.WriteAllText(fileName, textSource.Text);
                    lastWriteAcces = File.GetLastWriteTime(fileName);
                    parentTab.SetHeader(header);
                }
            }
        }

        internal void OnReOpen()
        {
            if (isEditable)
            {
                bool isCancel = saveChangesDialog("Есть не сохранённые изменения (" + header + "), вы хотите их сохранить прежде чем открыть файл заново?");
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
                    parentTab.SetHeader(header + "*");
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
                }
            }
            updateState();
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
    }
}
