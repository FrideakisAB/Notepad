using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для ButtonEditor.xaml
    /// </summary>
    public partial class ButtonEditor : Page, IEditorPage
    {

        PreviewPage Preview { get; }

        public ButtonEditor(PreviewPage preview)
        {
            InitializeComponent();

            Preview = preview;

            ChangePreview();

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
        }

        ~ButtonEditor()
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
                Preview.Resources["anyButtonBackgroundVal"] = backgroundColor.SelectedColor as SolidColorBrush;
        }

        private void foregroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonForegroundVal"] = foregroundColor.SelectedColor as SolidColorBrush;
        }

        private void borderBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonForegroundVal"] = borderBrushColor.SelectedColor as SolidColorBrush;
        }

        private void buttonMouseOverColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonMouseOverVal"] = buttonMouseOverColor.SelectedColor as SolidColorBrush;
        }

        private void buttonPressedEditor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonPressedVal"] = buttonPressedEditor.SelectedColor as SolidColorBrush;
        }

        private void fontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonFontFamilyVal"] = fontFamilyCombo.SelectedItem;
        }

        private void fontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonFontSizeVal"] = double.Parse((fontSizeCombo.SelectedItem as ComboBoxItem).Content as string);
        }

        private void fontStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonFontStyleVal"] = fontStyleCombo.SelectedItem;
        }

        private void fontWeightCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonFontWeightVal"] = fontWeightCombo.SelectedItem;
        }

        private void cornerRadiusEditor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Preview != null)
                Preview.Resources["anyButtonCornerVal"] = new CornerRadius(cornerRadiusEditor.Value);
        }
    }
}
