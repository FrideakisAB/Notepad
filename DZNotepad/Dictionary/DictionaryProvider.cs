using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace DZNotepad
{
    public class DictionaryProvider
    {
        private static Regex regex = new Regex(@"\w*Val$", RegexOptions.Compiled);

        public static void ApplyDictionary(ResourceDictionary target, ResourceDictionary from)
        {
            foreach (string key in from.Keys)
            {
                if (regex.IsMatch(key))
                    target[key] = from[key];
            }
        }

        public static void SaveStyleInDB(ResourceDictionary style, string name)
        {
            long id = (long)DBContext.CommandScalar(string.Format("SELECT styleNameId FROM stylesNames WHERE styleName = '{0}';", name));

            string saveScript =
                "INSERT INTO styles VALUES({0}, 'anyBackgroundVal', '{1}');" +
                "INSERT INTO styles VALUES({0}, 'anyForegroundVal', '{2}');" +
                "INSERT INTO styles VALUES({0}, 'anyBorderBrushVal', '{3}');" +
                "INSERT INTO styles VALUES({0}, 'anyFontFamilyVal', '{4}');" +
                "INSERT INTO styles VALUES({0}, 'anyFontSizeVal', '{5}');" +
                "INSERT INTO styles VALUES({0}, 'anyFontStyleVal', '{6}');" +
                "INSERT INTO styles VALUES({0}, 'anyFontWeightVal', '{7}');" +

                "INSERT INTO styles VALUES({0}, 'anyTBBackgroundVal', '{8}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBForegroundVal', '{9}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBBorderBrushVal', '{10}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBSelectionBrushVal', '{11}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBCaretBrushVal', '{12}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBCornerVal', '{13}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBFontFamilyVal', '{14}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBFontSizeVal', '{15}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBFontStyleVal', '{16}');" +
                "INSERT INTO styles VALUES({0}, 'anyTBFontWeightVal', '{17}');" +

                "INSERT INTO styles VALUES({0}, 'anyButtonBackgroundVal', '{18}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonForegroundVal', '{19}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonBorderBrushVal', '{20}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonMouseOverVal', '{21}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonPressedVal', '{22}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonCornerVal', '{23}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonFontFamilyVal', '{24}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonFontSizeVal', '{25}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonFontStyleVal', '{26}');" +
                "INSERT INTO styles VALUES({0}, 'anyButtonFontWeightVal', '{27}');" +

                "INSERT INTO styles VALUES({0}, 'anyTabItemBackgroundVal', '{28}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemForegroundVal', '{29}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemBorderBrushVal', '{30}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemCornerVal', '{31}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemFontFamilyVal', '{32}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemFontSizeVal', '{33}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemFontStyleVal', '{34}');" +
                "INSERT INTO styles VALUES({0}, 'anyTabItemFontWeightVal', '{35}');" +

                "INSERT INTO styles VALUES({0}, 'anyComboBackgroundVal', '{36}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboForegroundVal', '{37}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboBorderBrushVal', '{38}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboArrowVal', '{39}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboMouseOverVal', '{40}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboPressedVal', '{41}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboUnpressedVal', '{42}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboPopupBackVal', '{43}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboPopupBorderVal', '{44}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboCornerVal', '{45}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboCornerArrowVal', '{46}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboFontFamilyVal', '{47}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboFontSizeVal', '{48}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboFontStyleVal', '{49}');" +
                "INSERT INTO styles VALUES({0}, 'anyComboFontWeightVal', '{50}');";

            DBContext.Command(string.Format(saveScript,
                id,
                (style["anyBackgroundVal"] as SolidColorBrush).ToString(),
                (style["anyForegroundVal"] as SolidColorBrush).ToString(),
                (style["anyBorderBrushVal"] as SolidColorBrush).ToString(),
                (style["anyFontFamilyVal"] as FontFamily).ToString(),
                (double)style["anyFontSizeVal"],
                ((FontStyle)style["anyFontStyleVal"]).ToString(),
                ((FontWeight)style["anyFontWeightVal"]).ToString(),

                (style["anyTBBackgroundVal"] as SolidColorBrush).ToString(),
                (style["anyTBForegroundVal"] as SolidColorBrush).ToString(),
                (style["anyTBBorderBrushVal"] as SolidColorBrush).ToString(),
                (style["anyTBSelectionBrushVal"] as SolidColorBrush).ToString(),
                (style["anyTBCaretBrushVal"] as SolidColorBrush).ToString(),
                ((CornerRadius)style["anyTBCornerVal"]).ToString(),
                (style["anyTBFontFamilyVal"] as FontFamily).ToString(),
                (double)style["anyTBFontSizeVal"],
                ((FontStyle)style["anyTBFontStyleVal"]).ToString(),
                ((FontWeight)style["anyTBFontWeightVal"]).ToString(),

                (style["anyButtonBackgroundVal"] as SolidColorBrush).ToString(),
                (style["anyButtonForegroundVal"] as SolidColorBrush).ToString(),
                (style["anyButtonBorderBrushVal"] as SolidColorBrush).ToString(),
                (style["anyButtonMouseOverVal"] as SolidColorBrush).ToString(),
                (style["anyButtonPressedVal"] as SolidColorBrush).ToString(),
                ((CornerRadius)style["anyButtonCornerVal"]).ToString(),
                (style["anyButtonFontFamilyVal"] as FontFamily).ToString(),
                (double)style["anyButtonFontSizeVal"],
                ((FontStyle)style["anyButtonFontStyleVal"]).ToString(),
                ((FontWeight)style["anyButtonFontWeightVal"]).ToString(),

                (style["anyTabItemBackgroundVal"] as SolidColorBrush).ToString(),
                (style["anyTabItemForegroundVal"] as SolidColorBrush).ToString(),
                (style["anyTabItemBorderBrushVal"] as SolidColorBrush).ToString(),
                ((CornerRadius)style["anyTabItemCornerVal"]).ToString(),
                (style["anyTabItemFontFamilyVal"] as FontFamily).ToString(),
                (double)style["anyTabItemFontSizeVal"],
                ((FontStyle)style["anyTabItemFontStyleVal"]).ToString(),
                ((FontWeight)style["anyTabItemFontWeightVal"]).ToString(),

                (style["anyComboBackgroundVal"] as SolidColorBrush).ToString(),
                (style["anyComboForegroundVal"] as SolidColorBrush).ToString(),
                (style["anyComboBorderBrushVal"] as SolidColorBrush).ToString(),
                (style["anyComboArrowVal"] as SolidColorBrush).ToString(),
                (style["anyComboMouseOverVal"] as SolidColorBrush).ToString(),
                (style["anyComboPressedVal"] as SolidColorBrush).ToString(),
                (style["anyComboUnpressedVal"] as SolidColorBrush).ToString(),
                (style["anyComboPopupBackVal"] as SolidColorBrush).ToString(),
                (style["anyComboPopupBorderVal"] as SolidColorBrush).ToString(),
                ((CornerRadius)style["anyComboCornerVal"]).ToString(),
                ((CornerRadius)style["anyComboCornerArrowVal"]).ToString(),
                (style["anyComboFontFamilyVal"] as FontFamily).ToString(),
                (double)style["anyComboFontSizeVal"],
                ((FontStyle)style["anyComboFontStyleVal"]).ToString(),
                ((FontWeight)style["anyComboFontWeightVal"]).ToString()
            ));
        }

        public static void LoadStyleFromDB(ResourceDictionary style, string name)
        {
            //TODO
        }
    }
}
