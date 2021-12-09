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

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для BackgroundEditor.xaml
    /// </summary>
    public partial class BackgroundEditor : Page, IEditorPage
    {
        PreviewPage Preview { get; }

        public BackgroundEditor(PreviewPage preview)
        {
            InitializeComponent();

            Preview = preview;

            ChangePreview();

            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
        }

        ~BackgroundEditor()
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
                backgroundColor.SelectedColor = Preview.Resources["anyBackgroundVal"] as SolidColorBrush;
                foregroundColor.SelectedColor = Preview.Resources["anyForegroundVal"] as SolidColorBrush;
                borderBrushColor.SelectedColor = Preview.Resources["anyBorderBrushVal"] as SolidColorBrush;

                fontFamilyCombo.SelectedItem = fontFamilyCombo.Items.Cast<FontFamily>().Where(i => i.Equals(Preview.Resources["anyFontFamilyVal"])).First();

                string fontSize = ((int)((double)Preview.Resources["anyFontSizeVal"])).ToString();
                fontSizeCombo.SelectedItem = fontSizeCombo.Items.Cast<ComboBoxItem>().Where(i => (i.Content as string) == fontSize).First();
                fontStyleCombo.SelectedItem = fontStyleCombo.Items.Cast<FontStyle>().Where(i => i.Equals(Preview.Resources["anyFontStyleVal"])).First();
                fontWeightCombo.SelectedItem = fontWeightCombo.Items.Cast<FontWeight>().Where(i => i.Equals(Preview.Resources["anyFontWeightVal"])).First();
            }
        }

        private void backgroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyBackgroundVal"] = backgroundColor.SelectedColor as SolidColorBrush;
        }

        private void foregroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyForegroundVal"] = foregroundColor.SelectedColor as SolidColorBrush;
        }

        private void borderBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyBorderBrushVal"] = borderBrushColor.SelectedColor as SolidColorBrush;
        }

        private void fontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyFontFamilyVal"] = fontFamilyCombo.SelectedItem;
        }

        private void fontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyFontSizeVal"] = double.Parse((fontSizeCombo.SelectedItem as ComboBoxItem).Content as string);
        }

        private void fontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyFontStyleVal"] = fontStyleCombo.SelectedItem;
        }

        private void fontWeights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyFontWeightVal"] = fontWeightCombo.SelectedItem;
        }
    }
}
