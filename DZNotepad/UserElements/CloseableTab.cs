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
        }

        public void SetHeader(string header)
        {
            headerElement.Text = header;
        }

        public void SetStyle(Style style)
        {
            this.Style = style;
        }
    }
}
