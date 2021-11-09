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
            dockPanel.Margin = new Thickness(-7, -3, -7, -4);
            headerElement = new TextBlock { Text = "" };
            headerElement.Margin = new Thickness(3, 2, 2, 2);
            dockPanel.Children.Add(headerElement);

            var closeButton = new TabCloseButton();
            closeButton.Margin = new Thickness(2, 2, 2, 2);
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
        }

        public void SetHeader(string header)
        {
            headerElement.Text = header;
        }

        public void SetStyle(Style style)
        {
            (Header as DockPanel).Style = style;
        }
    }
}
