using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace DZNotepad
{
    public enum SecureFileDialogType
    {
        Open,
        Save
    }

    /// <summary>
    /// Диалоговое окно открытия/сохранения файла, с возможностью ограничения доступа
    /// </summary>
    public partial class SecureFileDialog : Window
    {
        private class Proxy
        {
            public BitmapSource Icon { get; set; }
            public string Path { get; set; }
            public string FullPath
            {
                get => System.IO.Path.GetFileName(Path);
            }
            public string ChangeDate { get; set; } = string.Empty;
            public string FileSize { get; set; } = string.Empty;

            public GridColumnsSize CurrentColumnsSize { get; set; }
        }

        private class GridColumnsSize : INotifyPropertyChanged
        {
            private string[] Sizes = new string[10];

            public void Set(int i, string value)
            {
                Sizes[i] = value;
                OnPropertyChanged("Get" + (i + 1).ToString());
            }

            public string Get1 { get => Sizes[0]; }
            public string Get2 { get => Sizes[1]; }
            public string Get3 { get => Sizes[2]; }
            public string Get4 { get => Sizes[3]; }
            public string Get5 { get => Sizes[4]; }
            public string Get6 { get => Sizes[5]; }
            public string Get7 { get => Sizes[6]; }
            public string Get8 { get => Sizes[7]; }
            public string Get9 { get => Sizes[8]; }
            public string Get10 { get => Sizes[9]; }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged([CallerMemberName] string prop = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        /// <summary>
        /// В случае если <c>DialogResult = true</c>, содержит путь к выбранному файлу
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Фильтр для проверки расширения, и указания доступных расширений для сохранения (по умолчанию - все файлы)
        /// <para>Формат: Описание|*.ext[;*.ext2]<c>[|Описание2|*.ext]</c></para>
        /// <para>Для указания всех файлов следует использовать: Описание|*.*<c>[|Описание2|*.ext]</c></para>
        /// </summary>
        public string Filter
        {
            set
            {
                ParseFilter(value);
            }
        }

        /// <summary>
        /// Указывает изначально выбранной значение в <c>Filter</c>
        /// </summary>
        public int FilterIndex { get; set; } = 0;

        private readonly string RootPath;
        private readonly SecureFileDialogType Mode;
        private readonly bool DiscardRoot;
        private List<string> FilterExtensions = new List<string>();
        private string CurrentPath;
        private GridColumnsSize CurrentColumnsSize = new GridColumnsSize();

        private static readonly string SecureRootName = "RDC:\\";

        private static readonly string[] SizePrefixs = { "Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЭБ", "ЗБ", "ЙБ" };

        /// <summary>
        /// Конструктор диалога открытия/сохранения файла
        /// </summary>
        /// <param name="rootPath">Корневой путь, верхний предел иерархии, доступ выше него будет ограничен</param>
        /// <param name="name">Название окна</param>
        /// <param name="dialogType">Тип диалога, открытие или сохранение</param>
        /// <param name="discardRoot">Если <c>true</c> - скрывает корневой путь в строке пути</param>
        public SecureFileDialog(string rootPath, string name, SecureFileDialogType dialogType, bool discardRoot=true)
        {
            DataContext = this;
            InitializeComponent();

            Title = name;

            RootPath = rootPath;
            CurrentPath = rootPath;
            Mode = dialogType;
            DiscardRoot = discardRoot;


            if (Mode == SecureFileDialogType.Open)
            {
                FileNameLabel.Visibility = Visibility.Collapsed;
                FileNameField.Visibility = Visibility.Collapsed;
            }

            GridSplitter_DragDelta(null, null);
            DisplayDirectory();
        }

        private void ParseFilter(string filter)
        {
            FilterExtensions.Clear();

            if (string.IsNullOrWhiteSpace(filter))
            {
                FileExtensionCombo.ItemsSource = new List<string> { "Все файлы (*.*)" };
                FileExtensionCombo.SelectedIndex = 0;
                return;
            }

            string[] filters = filter.Split('|');

            if (filters.Length <= 1)
            {
                FileExtensionCombo.ItemsSource = new List<string> { "Все файлы (*.*)" };
                FileExtensionCombo.SelectedIndex = 0;
                return;
            }

            List<string> comboFilter = new List<string>();

            for (int i = 0; i < filters.Length / 2; i++)
            {
                FilterExtensions.Add(filters[i * 2 + 1]);
                comboFilter.Add($"{filters[i * 2]} ({filters[i * 2 + 1]})");
            }

            FileExtensionCombo.ItemsSource = comboFilter;
            FileExtensionCombo.SelectedIndex = FilterIndex;
        }

        private void DisplayDirectory()
        {
            SetupPathLine();
            FilesEntries.Items.Clear();

            foreach (var dir in Directory.EnumerateDirectories(CurrentPath))
            {
                string changeDate = Directory.GetLastWriteTime(dir).ToString("g");

                BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(GetFileIcon(dir, true, true, false).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                FilesEntries.Items.Add(new Proxy { Icon = source, Path = dir, ChangeDate = changeDate, CurrentColumnsSize = CurrentColumnsSize });
            }

            foreach (var file in Directory.EnumerateFiles(CurrentPath))
            {
                if (IsAvailabeFile(file))
                {
                    FileInfo fileInfo = new FileInfo(file);

                    string changeDate = fileInfo.LastWriteTime.ToString("g");
                    string fileSize = ConvertToReadableFileSize(fileInfo.Length);

                    BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(GetFileIcon(file, false, true, false).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    FilesEntries.Items.Add(new Proxy { Icon = source, Path = file, ChangeDate = changeDate, FileSize = fileSize, CurrentColumnsSize = CurrentColumnsSize });
                }
            }
        }

        private static string ConvertToReadableFileSize(long fileSize)
        {
            for (int i = 0; i < SizePrefixs.Length; i++)
            {
                if (fileSize <= (Math.Pow(1024, i + 1)))
                    return (fileSize / Math.Pow(1024, i)) + " " + SizePrefixs[i];
            }

            return (fileSize / Math.Pow(1024, SizePrefixs.Length - 1)) + " " + SizePrefixs[^1];
        }

        private bool IsAvailabeFile(string path)
        {
            if (FilterExtensions.Count == 0)
                return true;

            int index = FileExtensionCombo.SelectedIndex;
            if (index == -1)
                index = 0;

            if (FilterExtensions[index] == "*.*")
                return true;

            string[] fileParts = path.Split('.');

            if (fileParts.Length <= 1)
                return false;

            string[] filters = FilterExtensions[index].Split(';');

            foreach (string filter in filters)
            {
                if (fileParts[^1] == filter.Substring(filter.IndexOf('.') + 1))
                    return true;
            }

            return false;
        }
        
        private void FileExtensionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayDirectory();

            if (FilterExtensions.Count != 0)
            {
                int length = FileNameField.Text.LastIndexOf('.');
                if (length == -1)
                    length = FileNameField.Text.Length;

                string fileName = FileNameField.Text.Substring(0, length);

                int index = FileExtensionCombo.SelectedIndex;
                if (index == -1)
                    index = 0;

                if (FilterExtensions[index] == "*.*")
                    return;

                FileNameField.Text = fileName + "." + FilterExtensions[index].Substring(FilterExtensions[index].IndexOf('.') + 1);
            }
        }

        private void SetupPathLine()
        {
            BarGrid.Visibility = Visibility.Visible;
            BarGrid.Children.Clear();

            string mainPath = string.Empty;

            foreach (string folder in GetCurrentPath().Split(Path.DirectorySeparatorChar))
            {
                if (string.IsNullOrWhiteSpace(folder))
                    continue;

                string currentPath = mainPath + folder + Path.DirectorySeparatorChar;

                Button pathButton = new Button();
                pathButton.Content = folder;
                pathButton.Background = new SolidColorBrush();
                pathButton.BorderBrush = new SolidColorBrush();
                pathButton.Click += (object sender, RoutedEventArgs e) => {
                    if (IsAccessPath(currentPath))
                    {
                        CurrentPath = currentPath.Replace(SecureRootName, RootPath + "\\");
                        DisplayDirectory();
                    }
                };

                BarGrid.Children.Add(pathButton);

                TextBlock arrow = new TextBlock();
                arrow.TextWrapping = TextWrapping.NoWrap;
                arrow.VerticalAlignment = VerticalAlignment.Center;
                arrow.Text = ">";
                arrow.Margin = new Thickness(1, 0, 1, 0);

                BarGrid.Children.Add(arrow);

                mainPath = currentPath;
            }

            PathScroll.ScrollToRightEnd();
        }

        private string GetCurrentPath()
        {
            string securePath;

            if (DiscardRoot == false)
                securePath = CurrentPath;
            else
                securePath = CurrentPath.Replace(RootPath, SecureRootName);

            return securePath;
        }

        private void CurrentPathLabel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string trimmedPath = CurrentPathLabel.Text.Trim();
                if (trimmedPath != GetCurrentPath() && IsAccessPath(trimmedPath))
                {
                    CurrentPath = trimmedPath.Replace(SecureRootName, RootPath);
                    DisplayDirectory();
                }
                else
                    MessageBox.Show("Не удаётся найти путь, проверьте правильность ввода", "Ошибка пути", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsAccessPath(string path)
        {
            if (!DiscardRoot && path.IndexOf(RootPath) != 0 ||
                DiscardRoot && path.IndexOf(SecureRootName) != 0)
                return false;

            if (!DiscardRoot && !Directory.Exists(path) ||
                DiscardRoot && !Directory.Exists(path.Replace(SecureRootName, RootPath + "\\")))
                return false;

            return true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }

        private void CurrentPathLabel_GotFocus(object sender, RoutedEventArgs e)
        {
            BarGrid.Visibility = Visibility.Collapsed;
            CurrentPathLabel.Text = GetCurrentPath();
            Panel.SetZIndex(CurrentPathLabel, 1);
            CurrentPathLabel.Select(0, CurrentPathLabel.Text.Length);
        }

        private void CurrentPathLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(CurrentPathLabel, 0);
            CurrentPathLabel.Text = string.Empty;
            SetupPathLine();
        }

        private void PathScroll_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CurrentPathLabel.Focus();
        }

        private void FilesEntries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesEntries.SelectedItem is Proxy proxy)
            {
                if (Directory.Exists(proxy.Path))
                {
                    CurrentPath = proxy.Path;
                    DisplayDirectory();
                }
                else
                    SelectButton_Click(null, null);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == SecureFileDialogType.Open)
            {
                if (FilesEntries.SelectedItem == null)
                {
                    MessageBox.Show("Необходимо выбрать файл!", "Файл", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Proxy proxy = FilesEntries.SelectedItem as Proxy;

                if (Directory.Exists(proxy.Path))
                {
                    CurrentPath = proxy.Path;
                    DisplayDirectory();
                    return;
                }

                FileName = proxy.Path;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(FileNameField.Text))
                {
                    MessageBox.Show("Необходимо ввести имя файла!", "Файл", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                FileName = Path.Combine(CurrentPath, FileNameField.Text);

                if (File.Exists(FileName))
                {
                    MessageBoxResult result = MessageBox.Show("Файл с таким именем уже существет, вы хотите его перезаписать?", "Файл", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                        return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            OneFieldDialog dlg = new OneFieldDialog();
            dlg.InputName = "Введите название папки";
            //TODO: add validator callback
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(CurrentPath, dlg.Result));
                    DisplayDirectory();
                }
                catch
                {
                    MessageBox.Show("При создании папки произошла ошибка, попробуйте другое имя", "Ошибка создания", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            if (FilesEntries.SelectedItem != null &&
                MessageBox.Show("Вы уверены что хотите удалить элемент?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Proxy proxy = FilesEntries.SelectedItem as Proxy;
                    if (Directory.Exists(proxy.Path))
                        Directory.Delete(proxy.Path);
                    else
                    {
                        DBContext.Command($"INSERT INTO fileHistory (filePath, userId, changeTime) VALUES ('{proxy.Path}', {UserSingleton.Get().LoginUser.UserId}, CURRENT_TIMESTAMP)");
                        File.Delete(proxy.Path);
                    }
                }
                catch
                {
                    MessageBox.Show("При удалении элемента произошла ошибка", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DialogResult.HasValue)
                DialogResult = false;
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            OneFieldDialog dlg = new OneFieldDialog();
            dlg.InputName = "Введите название файла";
            //TODO: add validator callback
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                try
                {
                    string filePath = Path.Combine(CurrentPath, dlg.Result);
                    DBContext.Command($"INSERT INTO fileHistory (filePath, userId, changeTime) VALUES ('{filePath}', {UserSingleton.Get().LoginUser.UserId}, CURRENT_TIMESTAMP)");
                    File.CreateText(filePath).Close();
                    DisplayDirectory();
                }
                catch
                {
                    MessageBox.Show("При создании файла произошла ошибка, попробуйте другое имя", "Ошибка создания", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            for (int i = 0; i < DragableGrid.ColumnDefinitions.Count; i++)
            {
                GridLength actualWidth = DragableGrid.ColumnDefinitions[i].Width;

                CurrentColumnsSize.Set(i, actualWidth.ToString());
            }
        }

        private static System.Drawing.Icon GetFileIcon(string name, bool isDir, bool isSmall, bool linkOverlay)
        {
            Shell32.Shfileinfo shfi = new Shell32.Shfileinfo();
            uint flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;

            if (linkOverlay == true) flags += Shell32.ShgfiLinkoverlay;

            if (isSmall == true)
                flags += Shell32.ShgfiSmallicon;
            else
                flags += Shell32.ShgfiLargeicon;

            uint attributeFlag;

            if (isDir == false)
                attributeFlag = Shell32.FileAttributeNormal;
            else
                attributeFlag = Shell32.FileAttributeDirectory;

            Shell32.SHGetFileInfo(name, attributeFlag, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

            System.Drawing.Icon icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();
            User32.DestroyIcon(shfi.hIcon);
            return icon;
        }

        static class Shell32
        {
            private const int MaxPath = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                private const int Namesize = 80;
                public readonly IntPtr hIcon;
                private readonly int iIcon;
                private readonly uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                private readonly string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                private readonly string szTypeName;
            };
            public const uint ShgfiIcon = 0x000000100;
            public const uint ShgfiLinkoverlay = 0x000008000;
            public const uint ShgfiLargeicon = 0x000000000;
            public const uint ShgfiSmallicon = 0x000000001;
            public const uint ShgfiUsefileattributes = 0x000000010;
            public const uint FileAttributeNormal = 0x00000080;
            public const uint FileAttributeDirectory = 0x00000010;
            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
                );
        }
        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }
}
