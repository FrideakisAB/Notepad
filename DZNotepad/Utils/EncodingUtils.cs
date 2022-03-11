using System;
using System.Collections.Generic;
using System.Text;

namespace DZNotepad
{
    public class EncodingUtils
    {
        public static Encoding UTF32BE { get; } = new UTF32Encoding(true, true);

        public static Encoding StringToSystemEncoding(string encoding)
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

        public static string SystemEncodingToString(Encoding encoding)
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
    }
}
