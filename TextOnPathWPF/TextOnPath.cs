using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using PhysicsSimulation.Utilities;

namespace TextOnPathWPF
{
    [ContentProperty("Text")]
    public class TextOnPath : Control
    {
        static TextOnPath()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextOnPath), new FrameworkPropertyMetadata(typeof(TextOnPath)));

            FontSizeProperty.OverrideMetadata(typeof(TextOnPath),
                new FrameworkPropertyMetadata(
                    OnFontPropertyChanged));

            FontFamilyProperty.OverrideMetadata(typeof(TextOnPath),
                new FrameworkPropertyMetadata(
                    OnFontPropertyChanged));

            FontStretchProperty.OverrideMetadata(typeof(TextOnPath),
                new FrameworkPropertyMetadata(
                    OnFontPropertyChanged));

            FontStyleProperty.OverrideMetadata(typeof(TextOnPath),
                new FrameworkPropertyMetadata(
                    OnFontPropertyChanged));

            FontWeightProperty.OverrideMetadata(typeof(TextOnPath),
                new FrameworkPropertyMetadata(
                    OnFontPropertyChanged));
        }

        static void OnFontPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == null || e.NewValue == e.OldValue)
                return;

            textOnPath.UpdateText();
            textOnPath.Update();
        }

        double[] _segmentLengths;
        TextBlock[] _textBlocks;

        Panel _layoutPanel;
        bool _layoutHasValidSize;

        #region Text DP
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(TextOnPath),
            new PropertyMetadata(null, OnStringPropertyChanged,
                CoerceTextValue));

        static void OnStringPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                if (textOnPath._layoutPanel != null)
                    textOnPath._layoutPanel.Children.Clear();
                return;
            }

            textOnPath.UpdateText();
            textOnPath.Update();
        }

        static object CoerceTextValue(DependencyObject d, object baseValue)
        {
            return (String)baseValue == "" ? null : baseValue;
        }

        #endregion

        #region TextPath DP
        public Geometry TextPath
        {
            get { return (Geometry)GetValue(TextPathProperty); }
            set { SetValue(TextPathProperty, value); }
        }

        public static readonly DependencyProperty TextPathProperty =
            DependencyProperty.Register("TextPath", typeof(Geometry), typeof(TextOnPath),
            new FrameworkPropertyMetadata(null,

                                          OnTextPathPropertyChanged));

        static void OnTextPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            textOnPath.TextPath.Transform = null;

            textOnPath.UpdateSize();
            textOnPath.Update();
        }

        #endregion

        #region DrawPath DP

        /// <summary>
        /// Set this property to True to display the TextPath geometry in the control
        /// </summary>
        public bool DrawPath
        {
            get { return (bool)GetValue(DrawPathProperty); }
            set { SetValue(DrawPathProperty, value); }
        }

        public static readonly DependencyProperty DrawPathProperty =
            DependencyProperty.Register("DrawPath", typeof(bool), typeof(TextOnPath),
            new PropertyMetadata(false, OnDrawPathPropertyChanged));

        static void OnDrawPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            textOnPath.Update();
        }

        #endregion

        #region DrawLinePath DP
        /// <summary>
        /// Set this property to True to display the line segments under the text (flattened path)
        /// </summary>
        public bool DrawLinePath
        {
            get { return (bool)GetValue(DrawLinePathProperty); }
            set { SetValue(DrawLinePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawFlattendPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawLinePathProperty =
            DependencyProperty.Register("DrawLinePath", typeof(bool), typeof(TextOnPath),
            new PropertyMetadata(false, OnDrawLinePathPropertyChanged));

        static void OnDrawLinePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            textOnPath.Update();
        }

        #endregion

        #region ScaleTextPath DP
        /// <summary>
        /// If set to True (default) then the geometry defined by TextPath automatically gets scaled to fit the width/height of the control
        /// </summary>
        public bool ScaleTextPath
        {
            get { return (bool)GetValue(ScaleTextPathProperty); }
            set { SetValue(ScaleTextPathProperty, value); }
        }

        public static readonly DependencyProperty ScaleTextPathProperty =
            DependencyProperty.Register("ScaleTextPath", typeof(bool), typeof(TextOnPath),
                    new PropertyMetadata(false, OnScaleTextPathPropertyChanged));

        static void OnScaleTextPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnPath = d as TextOnPath;

            if (textOnPath == null)
                return;

            if (e.NewValue == e.OldValue)
                return;

            var value = (Boolean)e.NewValue;

            if (value == false && textOnPath.TextPath != null)
                textOnPath.TextPath.Transform = null;

            textOnPath.UpdateSize();
            textOnPath.Update();

        }

        #endregion

        void UpdateText()
        {
            if (Text == null || FontFamily == null)
                return;

            _textBlocks = new TextBlock[Text.Length];
            _segmentLengths = new double[Text.Length];

            for (var i = 0; i < Text.Length; i++)
            {
                var t = new TextBlock
                {
                    FontSize = FontSize,
                    FontFamily = FontFamily,
                    FontStretch = FontStretch,
                    FontWeight = FontWeight,
                    FontStyle = FontStyle,
                    Text = new String(Text[i], 1),
                    RenderTransformOrigin = new Point(0.0, 1.0)
                };

                t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                _textBlocks[i] = t;
                _segmentLengths[i] = t.DesiredSize.Width;


            }
        }

        void Update()
        {
            if (Text == null || TextPath == null || _layoutPanel == null || !_layoutHasValidSize)
                return;

            var intersectionPoints = GeometryHelper.GetIntersectionPoints(TextPath.GetFlattenedPathGeometry(), _segmentLengths);

            _layoutPanel.Children.Clear();

            _layoutPanel.Margin = new Thickness(FontSize);

            for (var i = 0; i < intersectionPoints.Count - 1; i++)
            {
                var oppositeLen = FastSquareRoot.Sqrt(Convert.ToSingle(Math.Pow(intersectionPoints[i].X + _segmentLengths[i] - intersectionPoints[i + 1].X, 2.0) + Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0))) / 2.0;
                var hypLen = FastSquareRoot.Sqrt(Convert.ToSingle(Math.Pow(intersectionPoints[i].X - intersectionPoints[i + 1].X, 2.0) + Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0)));

                var ratio = oppositeLen / hypLen;

                if (ratio > 1.0)
                    ratio = 1.0;
                else if (ratio < -1.0)
                    ratio = -1.0;

                //double angle = 0.0;

                var angle = 2.0 * Math.Asin(ratio) * 180.0 / Math.PI;

                // adjust sign on angle
                if ((intersectionPoints[i].X + _segmentLengths[i]) > intersectionPoints[i].X)
                {
                    if (intersectionPoints[i + 1].Y < intersectionPoints[i].Y)
                        angle = -angle;
                }
                else
                {
                    if (intersectionPoints[i + 1].Y > intersectionPoints[i].Y)
                        angle = -angle;
                }

                var currTextBlock = _textBlocks[i];

                var rotate = new RotateTransform(angle);
                var translate = new TranslateTransform(intersectionPoints[i].X, intersectionPoints[i].Y - currTextBlock.DesiredSize.Height);
                var transformGrp = new TransformGroup();
                transformGrp.Children.Add(rotate);
                transformGrp.Children.Add(translate);
                currTextBlock.RenderTransform = transformGrp;

                _layoutPanel.Children.Add(currTextBlock);

                if (DrawLinePath)
                {
                    var line = new Line
                    {
                        X1 = intersectionPoints[i].X,
                        Y1 = intersectionPoints[i].Y,
                        X2 = intersectionPoints[i + 1].X,
                        Y2 = intersectionPoints[i + 1].Y,
                        Stroke = Brushes.Black
                    };
                    _layoutPanel.Children.Add(line);
                }
            }

            // don't draw path if already drawing line path
            if (DrawPath && DrawLinePath == false)
            {
                var path = new Path
                {
                    Data = TextPath,
                    Stroke = Brushes.Black
                };
                _layoutPanel.Children.Add(path);
            }
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _layoutPanel = GetTemplateChild("LayoutPanel") as Panel;
            if (_layoutPanel == null)
                throw new Exception("Could not find template part: LayoutPanel");

            _layoutPanel.SizeChanged += LayoutPanelSizeChanged;
        }

        Size _newSize;

        void LayoutPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _newSize = e.NewSize;

            UpdateSize();
            Update();
        }

        void UpdateSize()
        {
            if (TextPath == null)
                return;

            _layoutHasValidSize = true;

            var xScale = _newSize.Width / TextPath.Bounds.Width;
            var yScale = _newSize.Height / TextPath.Bounds.Height;

            if (TextPath.Bounds.Width <= 0)
                xScale = 1.0;

            if (TextPath.Bounds.Height <= 0)
                xScale = 1.0;

            if (xScale <= 0 || yScale <= 0)
                return;

            var grp = TextPath.Transform as TransformGroup;
            if (grp != null)
            {
                if (!(grp.Children[0] is ScaleTransform) || !(grp.Children[1] is TranslateTransform))
                    return;

                if (ScaleTextPath)
                {
                    var scale = (ScaleTransform)grp.Children[0];
                    scale.ScaleX *= xScale;
                    scale.ScaleY *= yScale;
                }

                var translate = (TranslateTransform)grp.Children[1];
                translate.X += -TextPath.Bounds.X;
                translate.Y += -TextPath.Bounds.Y;
            }
            else
            {
                ScaleTransform scale;
                TranslateTransform translate;

                if (ScaleTextPath)
                {
                    scale = new ScaleTransform(xScale, yScale);
                    translate = new TranslateTransform(-TextPath.Bounds.X * xScale, -TextPath.Bounds.Y * yScale);
                }
                else
                {
                    scale = new ScaleTransform(1.0, 1.0);
                    translate = new TranslateTransform(-TextPath.Bounds.X, -TextPath.Bounds.Y);
                }

                grp = new TransformGroup();
                grp.Children.Add(scale);
                grp.Children.Add(translate);
                TextPath.Transform = grp;
            }
        }
    }
}
