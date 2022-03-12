using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DZNotepad
{
    public class LastFiles : IDisposable
    {
        public const int LastFilesCount = 5;
        public event EventHandler OnAddFile;

        private List<string> LastFilesList = new List<string>(LastFilesCount);

        public LastFiles()
        {
            SqliteDataReader reader = DBContext.CommandReader("SELECT * FROM lastFiles");
            if (reader.HasRows)
            {
                while (reader.Read() && LastFilesList.Count <= LastFilesCount)
                    LastFilesList.Add(reader.GetValue(0) as string);
            }
        }

        public void Dispose()
        {
            if (LastFilesList.Count != 0)
            {
                DBContext.Command("DELETE FROM lastFiles;");
                foreach (string file in LastFilesList)
                    DBContext.Command(string.Format("INSERT INTO lastFiles VALUES ('{0}');", file));
            }
        }

        public void RegisterNewFile(string path)
        {
            if (!LastFilesList.Contains(path))
            {
                if (LastFilesList.Count == LastFilesCount)
                    LastFilesList.RemoveAt(0);
            }
            else
                LastFilesList.Remove(path);

            LastFilesList.Add(path);
            OnAddFile?.Invoke(this, new EventArgs());
        }

        public string[] GetLastFiles() => LastFilesList.ToArray();
    }
}
