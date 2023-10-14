using System.ComponentModel;

namespace FastDrawingUtilityLibrary.Events
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(object item,
                                            string propertyName)
            : base(propertyName)
        {
            Item = item;
        }

        public object Item { get; private set; }
    }

    public delegate void ItemPropertyChangedEventHandler(object sender,
                                        ItemPropertyChangedEventArgs args);
}
