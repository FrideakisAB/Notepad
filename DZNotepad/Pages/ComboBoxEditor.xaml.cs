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

namespace DZNotepad
{
    /// <summary>
    /// Логика взаимодействия для ComboBoxEditor.xaml
    /// </summary>
    public partial class ComboBoxEditor : Page, IEditorPage
    {
        PreviewPage Preview { get; }

        public ComboBoxEditor(PreviewPage preview)
        {
            InitializeComponent();

            Preview = preview;

            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);

            ChangePreview();

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
        }

        ~ComboBoxEditor()
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
                backgroundColor.SelectedColor = Preview.Resources["anyComboBackgroundVal"] as SolidColorBrush;
                foregroundColor.SelectedColor = Preview.Resources["anyComboForegroundVal"] as SolidColorBrush;
                borderBrushColor.SelectedColor = Preview.Resources["anyComboBorderBrushVal"] as SolidColorBrush;
                arrowColor.SelectedColor = Preview.Resources["anyComboArrowVal"] as SolidColorBrush;
                mouseOverColor.SelectedColor = Preview.Resources["anyComboMouseOverVal"] as SolidColorBrush;
                pressedColor.SelectedColor = Preview.Resources["anyComboPressedVal"] as SolidColorBrush;
                arrowColor.SelectedColor = Preview.Resources["anyComboUnpressedVal"] as SolidColorBrush;
                popupBackColor.SelectedColor = Preview.Resources["anyComboPopupBackVal"] as SolidColorBrush;
                popupBorderColor.SelectedColor = Preview.Resources["anyComboPopupBorderVal"] as SolidColorBrush;

                fontFamilyCombo.SelectedItem = fontFamilyCombo.Items.Cast<FontFamily>().Where(i => i.Equals(Preview.Resources["anyComboFontFamilyVal"])).First();

                string fontSize = ((int)((double)Preview.Resources["anyComboFontSizeVal"])).ToString();
                fontSizeCombo.SelectedItem = fontSizeCombo.Items.Cast<ComboBoxItem>().Where(i => (i.Content as string) == fontSize).First();
                fontStyleCombo.SelectedItem = fontStyleCombo.Items.Cast<FontStyle>().Where(i => i.Equals(Preview.Resources["anyComboFontStyleVal"])).First();
                fontWeightCombo.SelectedItem = fontWeightCombo.Items.Cast<FontWeight>().Where(i => i.Equals(Preview.Resources["anyComboFontWeightVal"])).First();
                cornerSlider.Value = ((CornerRadius)Preview.Resources["anyComboCornerVal"]).TopLeft;
            }
        }

        private void backgroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboBackgroundVal"] = backgroundColor.SelectedColor as SolidColorBrush;
        }

        private void foregroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboForegroundVal"] = foregroundColor.SelectedColor as SolidColorBrush;
        }

        private void borderBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboBorderBrushVal"] = borderBrushColor.SelectedColor as SolidColorBrush;
        }

        private void fontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboFontFamilyVal"] = fontFamilyCombo.SelectedItem;
        }

        private void fontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboFontSizeVal"] = double.Parse((fontSizeCombo.SelectedItem as ComboBoxItem).Content as string);
        }

        private void fontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboFontStyleVal"] = fontStyleCombo.SelectedItem;
        }

        private void fontWeights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboFontWeightVal"] = fontWeightCombo.SelectedItem;
        }

        private void cornerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Preview != null)
            {
                Preview.Resources["anyComboCornerVal"] = new CornerRadius(cornerSlider.Value);
                Preview.Resources["anyComboCornerArrowVal"] = new CornerRadius(cornerSlider.Value, 0, 0, cornerSlider.Value);
            }
        }

        private void arrowColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboArrowVal"] = arrowColor.SelectedColor as SolidColorBrush;
        }

        private void mouseOverColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboMouseOverVal"] = mouseOverColor.SelectedColor as SolidColorBrush;
        }

        private void pressedColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboPressedVal"] = pressedColor.SelectedColor as SolidColorBrush;
        }

        private void popupBackColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboPopupBackVal"] = popupBackColor.SelectedColor as SolidColorBrush;
        }

        private void popupBorderColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboPopupBorderVal"] = popupBorderColor.SelectedColor as SolidColorBrush;
        }

        private void unpressedColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyComboUnpressedVal"] = unpressedColor.SelectedColor as SolidColorBrush;
        }
    }
}
