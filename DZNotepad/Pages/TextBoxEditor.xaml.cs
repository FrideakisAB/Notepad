using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DZNotepad.Pages
{
    /// <summary>
    /// Логика взаимодействия для TextBoxEditor.xaml
    /// </summary>
    public partial class TextBoxEditor : Page, IEditorPage
    {

        PreviewPage Preview { get; }
        public TextBoxEditor(PreviewPage preview)
        {
            InitializeComponent();

            Preview = preview;

            ChangePreview();

            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
        }

        ~TextBoxEditor()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(this.Resources, dictionary);
        }

        public void ChangePreview()
        {
            if (Preview != null)
            {
                backgroundColor.SelectedColor = Preview.Resources["anyTBBackgroundVal"] as SolidColorBrush;
                foregroundColor.SelectedColor = Preview.Resources["anyTBForegroundVal"] as SolidColorBrush;
                borderBrushColor.SelectedColor = Preview.Resources["anyTBBorderBrushVal"] as SolidColorBrush;

                fontFamilyCombo.SelectedItem = fontFamilyCombo.Items.Cast<FontFamily>().Where(i => i.Equals(Preview.Resources["anyTBFontFamilyVal"])).First();

                string fontSize = ((int)((double)Preview.Resources["anyTBFontSizeVal"])).ToString();
                fontSizeCombo.SelectedItem = fontSizeCombo.Items.Cast<ComboBoxItem>().Where(i => (i.Content as string) == fontSize).First();
                fontStyleCombo.SelectedItem = fontStyleCombo.Items.Cast<FontStyle>().Where(i => i.Equals(Preview.Resources["anyTBFontStyleVal"])).First();
                fontWeightCombo.SelectedItem = fontWeightCombo.Items.Cast<FontWeight>().Where(i => i.Equals(Preview.Resources["anyTBFontWeightVal"])).First();
            }
        }

        private void backgroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBBackgroundVal"] = backgroundColor.SelectedColor as SolidColorBrush;
        }

        private void foregroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBForegroundVal"] = foregroundColor.SelectedColor as SolidColorBrush;
        }

        private void borderBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBBorderBrushVal"] = borderBrushColor.SelectedColor as SolidColorBrush;
        }

        private void selectionBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBSelectionBrushVal"] = selectionBrushColor.SelectedColor as SolidColorBrush;
        }

        private void caretBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBCaretBrushVal"] = caretBrushColor.SelectedColor as SolidColorBrush;
        }

        private void fontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBFontFamilyVal"] = fontFamilyCombo.SelectedItem;
        }

        private void fontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBFontSizeVal"] = double.Parse((fontSizeCombo.SelectedItem as ComboBoxItem).Content as string);
        }

        private void fontStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBFontStyleVal"] = fontStyleCombo.SelectedItem;
        }

        private void fontWeightCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTBFontWeightVal"] = fontWeightCombo.SelectedItem;
        }

        private void cornerRadiusEditor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Preview != null)
                Preview.Resources["anyTBCornerVal"] = new CornerRadius(cornerRadiusEditor.Value);
        }
    }
}
