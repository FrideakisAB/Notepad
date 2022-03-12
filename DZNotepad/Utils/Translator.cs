using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace DZNotepad
{
    public class Translator
    {
        private const int maxCharCount = 2500;
        // Google support all languages in this dictionary

        public static Dictionary<string, string> LanguagesWithISO { get => nameLanguageToISO; }

        static Dictionary<string, string> nameLanguageToISO = new Dictionary<string, string>()
        {
            { "afrikaans", "af" },
            { "albanian", "sq" },
            { "amharic", "am" },
            { "arabic", "ar" },
            { "armenian", "hy" },
            { "azerbaijani", "az" },
            { "basque", "eu" },
            { "belarusian", "be" },
            { "bengali", "bn" },
            { "bosnian", "bs" },
            { "bulgarian", "bg" },
            { "catalan", "ca" },
            { "chichewa", "ny" },
            { "corsican", "co" },
            { "croatian", "hr" },
            { "czech", "cs" },
            { "danish", "da" },
            { "dutch", "nl" },
            { "english", "en" },
            { "esperanto", "eo" },
            { "estonian", "et" },
            { "filipino", "tl" },
            { "finnish", "fi" },
            { "french", "fr" },
            { "frisian", "fy" },
            { "galician", "gl" },
            { "georgian", "ka" },
            { "german", "de" },
            { "greek", "el" },
            { "gujarati", "gu" },
            { "haitian", "ht" },
            { "hausa", "ha" },
            { "hebrew", "he" },
            { "hindi", "hi" },
            { "chinese (simplified)", "zh-cn" },
            { "chinese (traditional)", "zh-tw" },
            { "cebuano", "ceb" },
            { "hawaiian", "haw" },
            { "hmong", "hmn" },
            { "hungarian", "hu" },
            { "icelandic", "is" },
            { "igbo", "ig" },
            { "indonesian", "id" },
            { "irish", "ga" },
            { "italian", "it" },
            { "japanese", "ja" },
            { "javanese", "jw" },
            { "kannada", "kn" },
            { "kazakh", "kk" },
            { "khmer", "km" },
            { "korean", "ko" },
            { "kurdish", "ku" },
            { "kyrgyz", "ky" },
            { "lao", "lo" },
            { "latin", "la" },
            { "latvian", "lv" },
            { "lithuanian", "lt" },
            { "luxembourgish", "lb" },
            { "macedonian", "mk" },
            { "malagasy", "mg" },
            { "malay", "ms" },
            { "malayalam", "ml" },
            { "maltese", "mt" },
            { "maori", "mi" },
            { "marathi", "mr" },
            { "mongolian", "mn" },
            { "myanmar", "my" },
            { "nepali", "ne" },
            { "norwegian", "no" },
            { "odia", "or" },
            { "pashto", "ps" },
            { "persian", "fa" },
            { "polish", "pl" },
            { "portuguese", "pt" },
            { "punjabi", "pa" },
            { "romanian", "ro" },
            { "russian", "ru" },
            { "samoan", "sm" },
            { "scots gaelic", "gd" },
            { "serbian", "sr" },
            { "sesotho", "st" },
            { "shona", "sn" },
            { "sindhi", "sd" },
            { "sinhala", "si" },
            { "slovak", "sk" },
            { "slovenian", "sl" },
            { "somali", "so" },
            { "spanish", "es" },
            { "sundanese", "su" },
            { "swahili", "sw" },
            { "swedish", "sv" },
            { "tajik", "tg" },
            { "tamil", "ta" },
            { "telugu", "te" },
            { "thai", "th" },
            { "turkish", "tr" },
            { "ukrainian", "uk" },
            { "urdu", "ur" },
            { "uyghur", "ug" },
            { "uzbek", "uz" },
            { "vietnamese", "vi" },
            { "welsh", "cy" },
            { "xhosa", "xh" },
            { "yiddish", "yi" },
            { "yoruba", "yo" },
            { "zulu", "zu" }
        };

        HttpClient client = new HttpClient();

        public static bool IsValidLanguage(string language) => nameLanguageToISO.ContainsKey(language.ToLower());

        public string Translate(string sourceLanguage, string destinationLanguage, string text)
        {
            if (!IsValidLanguage(sourceLanguage) || !IsValidLanguage(destinationLanguage))
                throw new ArgumentException("Language names incorrect");

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (text.Length > maxCharCount)
            {
                string srcISO = nameLanguageToISO[sourceLanguage.ToLower()];
                string dstISO = nameLanguageToISO[destinationLanguage.ToLower()];

                List<string> splitText = splitTextBySuggestions(text);

                StringBuilder result = new StringBuilder(text.Length / 3);

                foreach (string suggestion in splitText)
                    result.Append(translateBlock(srcISO, dstISO, suggestion));

                return result.ToString();
            }
            else
                return translateBlock(nameLanguageToISO[sourceLanguage.ToLower()], nameLanguageToISO[destinationLanguage.ToLower()], text);
        }

        private string translateBlock(string srcLangISO, string dstLangISO, string text)
        {
            string url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", srcLangISO, dstLangISO, Uri.EscapeUriString(text));

            try
            {
                var jsonData = JsonSerializer.Deserialize<List<JsonElement>>(client.GetStringAsync(url).Result);

                JsonElement translationItems = jsonData[0];

                string translation = string.Empty;

                foreach (var item in translationItems.EnumerateArray())
                    translation += item[0].GetString();

                if (translation.Length > 1)
                    translation = translation.Replace("¡", string.Empty);

                return translation;
            }
            catch
            {
                return text;
            }
        }

        private List<string> splitTextBySuggestions(string text)
        {
            List<string> suggestions = new List<string>();

            while (text.Length > maxCharCount)
            {
                int indexDelim = text.LastIndexOf(".", maxCharCount);
                int indexExclamation = text.LastIndexOf('!', maxCharCount);
                int indexQuestion = text.LastIndexOf('?', maxCharCount);
                if (indexExclamation != -1 && indexExclamation > indexDelim) indexDelim = indexExclamation;
                if (indexQuestion != -1 && indexQuestion > indexDelim) indexDelim = indexQuestion;

                if (indexDelim == -1) indexDelim = text.LastIndexOf(",", maxCharCount);
                if (indexDelim == -1) indexDelim = text.LastIndexOf(" ", maxCharCount);
                if (indexDelim == -1) indexDelim = maxCharCount;

                suggestions.Add(text.Substring(0, indexDelim));
                text = text.Remove(0, indexDelim);
            }

            if (string.IsNullOrWhiteSpace(text))
                suggestions.Add(text);

            return suggestions;
        }
    }
}
