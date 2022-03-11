using System;
using System.Windows;
using System.Windows.Controls;
using DZNotepad.UserElements;
using System.Windows.Media;

namespace DZNotepad
{
    public class CloseableTab : TabItem
    {
        public delegate void EventTabClosedHandler(object sender, EventTabClosedArgs e);
        public event EventTabClosedHandler Closed;
        TextBlock headerElement;

        public CloseableTab()
        {
            var dockPanel = new DockPanel();
            headerElement = new TextBlock { Text = "" };
            dockPanel.Children.Add(headerElement);

            var closeButton = new TabCloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    EventTabClosedArgs args = new EventTabClosedArgs();
                    Closed?.Invoke(sender, args);

                    if (!args.PreventDefault)
                    {
                        var tabControl = Parent as ItemsControl;
                        tabControl.Items.Remove(this);
                    }
                };
            dockPanel.Children.Add(closeButton);

            Header = dockPanel;

            SelectStyle.UpdateStyleObservers += UpdateStyleObservers;

            ResourceDictionary themeDictionary = new ResourceDictionary();
            themeDictionary.Source = new Uri("/DZNotepad;component/Dictionary/Theme.xaml", UriKind.RelativeOrAbsolute);
            Resources.MergedDictionaries.Add(themeDictionary);

            ResourceDictionary anyStyleDictionary = new ResourceDictionary();
            anyStyleDictionary.Source = new Uri("/DZNotepad;component/Dictionary/AnyStyle.xaml", UriKind.RelativeOrAbsolute);
            Resources.MergedDictionaries.Add(anyStyleDictionary);

            DictionaryProvider.ApplyDictionary(Resources, SelectStyle.CurrentDictionary);

            Style = (Resources["AnyStyleTabItem"] as Style);
        }

        ~CloseableTab()
        {
            SelectStyle.UpdateStyleObservers -= UpdateStyleObservers;
        }

        private void UpdateStyleObservers(ResourceDictionary dictionary)
        {
            DictionaryProvider.ApplyDictionary(Resources, dictionary);
        }

        public void SetHeader(string header)
        {
            headerElement.Text = header;
        }
    }
}
