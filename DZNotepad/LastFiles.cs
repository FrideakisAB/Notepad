using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DZNotepad
{
    public class LastFiles : IDisposable
    {
        public const int LastFilesCount = 5;
        public event EventHandler OnAddFile;

        List<string> lastFiles = new List<string>(LastFilesCount);

        public LastFiles()
        {
            SqliteDataReader reader = DBContext.CommandReader("SELECT * FROM lastFiles");
            if (reader.HasRows)
            {
                while (reader.Read() && lastFiles.Count <= LastFilesCount)
                    lastFiles.Add(reader.GetValue(0) as string);
            }
        }

        public void Dispose()
        {
            if (lastFiles.Count != 0)
            {
                DBContext.Command("DELETE FROM lastFiles;");
                foreach (string file in lastFiles)
                    DBContext.Command(string.Format("INSERT INTO lastFiles VALUES ('{0}');", file));
            }
        }

        public void RegisterNewFile(string path)
        {
            if (!lastFiles.Contains(path))
            {
                if (lastFiles.Count == LastFilesCount)
                    lastFiles.RemoveAt(0);
            }
            else
                lastFiles.Remove(path);

            lastFiles.Add(path);
            OnAddFile?.Invoke(this, new EventArgs());
        }

        public string[] GetLastFiles() => lastFiles.ToArray();
    }
}
