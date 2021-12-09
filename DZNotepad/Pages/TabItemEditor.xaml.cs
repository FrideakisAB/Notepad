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
    /// Логика взаимодействия для TabItemEditor.xaml
    /// </summary>
    public partial class TabItemEditor : Page, IEditorPage
    {
        PreviewPage Preview { get; }

        public TabItemEditor(PreviewPage preview)
        {
            InitializeComponent();

            Preview = preview;

            ChangePreview();

            DictionaryProvider.ApplyDictionary(this.Resources, SelectStyle.CurrentDictionary);

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;
        }

        ~TabItemEditor()
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
                backgroundColor.SelectedColor = Preview.Resources["anyTabItemBackgroundVal"] as SolidColorBrush;
                foregroundColor.SelectedColor = Preview.Resources["anyTabItemForegroundVal"] as SolidColorBrush;
                borderBrushColor.SelectedColor = Preview.Resources["anyTabItemBorderBrushVal"] as SolidColorBrush;

                fontFamilyCombo.SelectedItem = fontFamilyCombo.Items.Cast<FontFamily>().Where(i => i.Equals(Preview.Resources["anyTabItemFontFamilyVal"])).First();

                string fontSize = ((int)((double)Preview.Resources["anyTabItemFontSizeVal"])).ToString();
                fontSizeCombo.SelectedItem = fontSizeCombo.Items.Cast<ComboBoxItem>().Where(i => (i.Content as string) == fontSize).First();
                fontStyleCombo.SelectedItem = fontStyleCombo.Items.Cast<FontStyle>().Where(i => i.Equals(Preview.Resources["anyTabItemFontStyleVal"])).First();
                fontWeightCombo.SelectedItem = fontWeightCombo.Items.Cast<FontWeight>().Where(i => i.Equals(Preview.Resources["anyTabItemFontWeightVal"])).First();
                cornerSlider.Value = ((CornerRadius)Preview.Resources["anyTabItemCornerVal"]).TopLeft;
            }
        }

        private void backgroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemBackgroundVal"] = backgroundColor.SelectedColor as SolidColorBrush;
        }

        private void foregroundColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemForegroundVal"] = foregroundColor.SelectedColor as SolidColorBrush;
        }

        private void borderBrushColor_SelectedColorChanged(object sender, EventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemBorderBrushVal"] = borderBrushColor.SelectedColor as SolidColorBrush;
        }

        private void fontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemFontFamilyVal"] = fontFamilyCombo.SelectedItem;
        }

        private void fontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemFontSizeVal"] = double.Parse((fontSizeCombo.SelectedItem as ComboBoxItem).Content as string);
        }

        private void fontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemFontStyleVal"] = fontStyleCombo.SelectedItem;
        }

        private void fontWeights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemFontWeightVal"] = fontWeightCombo.SelectedItem;
        }

        private void cornerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Preview != null)
                Preview.Resources["anyTabItemCornerVal"] = new CornerRadius(cornerSlider.Value);
        }
    }
}
