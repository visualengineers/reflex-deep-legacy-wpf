using System;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;

namespace FastDrawingUtilityLibrary
{
    public class VectorData : BindableBase
    {
        Color _color;
        Point _start, _end;
        string _id;

        public Color Color
        {
            set
            {
                if (_color == value) 
                    return;
                _color = value;
                RaisePropertyChanged();
            }
            get { return _color; }
        }

        public Point Start
        {
            set
            {
                if (Math.Abs(_start.X - value.X) < Double.Epsilon && Math.Abs(_start.Y - value.Y) < Double.Epsilon) 
                    return;
                _start = value;
                RaisePropertyChanged();
            }
            get { return _start; }
        }

        public Point End
        {
            set
            {
                if (Math.Abs(_end.X - value.X) < Double.Epsilon && Math.Abs(_end.Y - value.Y) < Double.Epsilon) 
                    return;
                _end = value;
                RaisePropertyChanged();
            }
            get { return _end; }
        }

        public string Id
        {
            set
            {
                if (_id == value) 
                    return;
                _id = value;
                RaisePropertyChanged();
            }
            get { return _id; }
        }
    }
}