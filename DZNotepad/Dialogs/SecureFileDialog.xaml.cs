using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для SecureFileDialog.xaml
    /// </summary>

    public enum SecureFileDialogType
    {
        Open,
        Save
    }

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
        }

        /// <summary>
        /// В случае если DialogResult = true, содержит путь к выбранному файлу
        /// </summary>
        public string FileName { get; private set; }

        private string RootPath;
        private string CurrentPath;
        private SecureFileDialogType Mode;
        private bool DiscardRoot;

        private static string SecureRootName = "RDC:\\";

        /// <summary>
        /// Конструктор диалога открытия/сохранения файла
        /// </summary>
        /// <param name="rootPath">Корневой путь, верхний предел иерархии, доступ выше него будет ограничен</param>
        /// <param name="name">Название окна</param>
        /// <param name="dialogType">Тип диалога, открытие или сохранение</param>
        /// <param name="discardRoot">Если true - скрывает корневой путь в строке пути</param>
        public SecureFileDialog(string rootPath, string name, SecureFileDialogType dialogType, bool discardRoot=true)
        {
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

            DisplayDirectory();
        }

        private void DisplayDirectory()
        {
            SetupPathLine();
            FilesEntries.Items.Clear();

            foreach (var dir in Directory.EnumerateDirectories(CurrentPath))
            {
                BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(GetFileIcon(dir, true, true, false).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                FilesEntries.Items.Add(new Proxy { Icon = source, Path = dir });
            }

            foreach (var file in Directory.EnumerateFiles(CurrentPath))
            {
                BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(GetFileIcon(file, false, true, false).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                FilesEntries.Items.Add(new Proxy { Icon = source, Path = file });
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
                        CurrentPath = currentPath.Replace(SecureRootName, RootPath);
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
                DiscardRoot && !Directory.Exists(path.Replace(SecureRootName, RootPath)))
                return false;

            return true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }

        private void CurrentPathLabel_GotFocus(object sender, RoutedEventArgs e)
        {
            BarGrid.Visibility = Visibility.Hidden;
            CurrentPathLabel.Text = GetCurrentPath();
            CurrentPathLabel.Select(0, CurrentPathLabel.Text.Length);
        }

        private void CurrentPathLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            CurrentPathLabel.Text = string.Empty;
            SetupPathLine();
        }

        private void FilesEntries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Proxy proxy = FilesEntries.SelectedItem as Proxy;
            if (proxy != null)
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
            CreateFolder dlg = new CreateFolder();
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(CurrentPath, dlg.Name));
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
            CreateFile dlg = new CreateFile();
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                try
                {
                    string filePath = Path.Combine(CurrentPath, dlg.Name);
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
