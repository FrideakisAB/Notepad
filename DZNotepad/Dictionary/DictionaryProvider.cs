using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DZNotepad
{
    public class DictionaryProvider
    {
        public static void ApplyDictionary(ResourceDictionary target, ResourceDictionary from)
        {
            foreach (string key in from.Keys)
            {
                target[key] = from[key];
            }
        }
    }
}
