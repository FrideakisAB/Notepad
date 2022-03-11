using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;

namespace DZNotepad
{
    public class TabsEnumerator : IEnumerator
    {
        private TabControl TabControl;
        private int Position = -1;

        public TabsEnumerator(TabControl tabControl)
        {
            TabControl = tabControl;
        }

        public object Current
        {
            get
            {
                if (Position == -1 || Position >= TabControl.Items.Count)
                    throw new ArgumentException();

                return (EditableFile)(TabControl.Items[Position] as CloseableTab)?.Content;
            }
        }

        public bool MoveNext()
        {
            if (Position < TabControl.Items.Count - 1)
            {
                Position++;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            Position = -1;
        }

        public void Dispose() { }
    }
}
