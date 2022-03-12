using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace DZNotepad
{
    public class ConfigParser
    {
        public List<string> Languages { get; private set; } = new List<string>();

        public void ParseFile(string path)
        {
            if (File.Exists(path))
            {
                CreateConfigFile(path);
                return;
            }

            Languages.Clear();

            string[] configLines = File.ReadAllLines(path);

            for (int i = 0; i < configLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(configLines[i]) || configLines[i][0] == ';')
                    continue;

                string language = configLines[i].Trim();

                if (language.IndexOf(';') != -1)
                    language = language.Remove(language.IndexOf(';'));

                if (Translator.IsValidLanguage(language))
                    Languages.Add(language);
                else
                    MessageBox.Show($"Во время разбора файла конфигурации произошла ошибка! В строке {i + 1} указан не поддерживаемый язык, либо неправильно его описание", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CreateConfigFile(string path)
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

            foreach (var pair in Translator.LanguagesWithISO)
                startupMessage += "; " + pair.Key + "\n";

            File.WriteAllText(path, startupMessage);

            Languages.Clear();

            Languages.Add("english");
            Languages.Add("german");
            Languages.Add("french");
            Languages.Add("bulgarian");
            Languages.Add("polish");
        }

        public void CreateLanguageFolders()
        {
            foreach (string language in Languages)
                Directory.CreateDirectory(Path.Combine(UserSingleton.RootPath, language));
        }
    }
}
