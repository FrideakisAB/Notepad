using System.Collections;
using System.Windows.Controls;

namespace DZNotepad
{
    public class FileTabManager : IEnumerable
    {
        private TabControl TabControl;

        private LastFiles LastFilesObject;
        private FileInfoBlock FileInfoBlockObject;
        private TranslateInfoBlock TranslateInfoBlockObject;

        public EditableFile SelectedFile { get => (EditableFile)(TabControl.SelectedItem as CloseableTab)?.Content; }

        public FileTabManager(TabControl tabControl, LastFiles lastFilesObject, FileInfoBlock fileInfoBlockObject, TranslateInfoBlock translateInfoBlockObject)
        {
            TabControl = tabControl;
            LastFilesObject = lastFilesObject;
            FileInfoBlockObject = fileInfoBlockObject;
            TranslateInfoBlockObject = translateInfoBlockObject;

            TabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.Items.IsEmpty)
            {
                CreateNewTab();
                TabControl.SelectedItem = TabControl.Items.GetItemAt(0);
            }
        }

        public void CreateNewTab(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                CloseableTab item = new CloseableTab();
                TabControl.Items.Add(item);
                EditableFile editableFile = new EditableFile(LastFilesObject, item, FileInfoBlockObject, TranslateInfoBlockObject);
                item.Content = editableFile;
                TabControl.SelectedItem = item;
            }
            else
            {
                CloseableTab targetTab = null;
                foreach (var tab in TabControl.Items)
                {
                    var closeableTab = tab as CloseableTab;
                    if (closeableTab != null && ((EditableFile)closeableTab.Content).FileName == path)
                        targetTab = closeableTab;
                }

                if (targetTab == null)
                {
                    CloseableTab item = new CloseableTab();
                    TabControl.Items.Add(item);
                    EditableFile editableFile = new EditableFile(LastFilesObject, item, FileInfoBlockObject, TranslateInfoBlockObject, path);
                    item.Content = editableFile;
                    TabControl.SelectedItem = item;

                    LastFilesObject.RegisterNewFile(path);
                }
                else
                {
                    ((EditableFile)targetTab.Content).OnReOpen();
                    TabControl.SelectedItem = targetTab;
                }
            }
        }

        public void RemoveFileTab(EditableFile file)
        {
            TabControl.Items.Remove(file);
        }

        public void Clear()
        {
            TabControl.Items.Clear();
        }


        public IEnumerator GetEnumerator()
        {
            return new TabsEnumerator(TabControl);
        }
    }
}
