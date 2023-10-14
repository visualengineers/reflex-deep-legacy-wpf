using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace FlexiWallWPF.Behaviours
{
    //TODO: merge into UtilityLibrary
    /// <summary>
    /// Behaviour causing the ScrollViewer to scroll to the last line when elements are added (via attaching to <see cref="System.Windows.UIElement.LayoutUpdated">UIElement.LayoutUpdated</see>-event).
    /// </summary>
    public class AutoScrollBehavior : Behavior<ScrollViewer>
    {
        /// <summary>
        /// tolerance: if height of the element has changed less than this value, there is no automatic scrolling.
        /// </summary>
        private const double Tolerance = 0.1;

        /// <summary>
        /// The ScrollViewer the Bahaviour is attached to
        /// </summary>
        private ScrollViewer _scrollViewer;

        /// <summary>
        /// Auxiliary variable storing the current height of the ScrollViewer.
        /// </summary>
        private double _height;
        
        /// <summary>
        /// Method is called when the behaviour is attached to a UI-Element (of type ScrollViewer). 
        /// Stores the associated scrollviewer for later use and attaches the<see cref="System.Windows.UIElement.LayoutUpdated">UIElement.LayoutUpdated</see> event handler 
        /// to the ScrollViewers <see cref="UIElement.LayoutUpdated">UIElement.LayoutUpdated</see>-event.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            _scrollViewer = AssociatedObject;
            _scrollViewer.LayoutUpdated += ScrollViewerLayoutUpdated;
        }

        /// <summary>
        /// Method handles the <see cref="System.Windows.UIElement.LayoutUpdated">UIElement.LayoutUpdated</see>-event from the ScrollViewer.
        /// Checks whether the height has changed significantly. if this is the case, the current height is 
        /// updated and the ScrollViewer scrolls to the new offset.
        /// </summary>
        /// <param name="sender">The object firing the event. Not used.</param>
        /// <param name="e">Event-parameters. Not used.</param>
        private void ScrollViewerLayoutUpdated(object sender, EventArgs e)
        {           
            if (Math.Abs(_scrollViewer.ExtentHeight - _height) < Tolerance) 
                return;
            _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ExtentHeight);
            _height = _scrollViewer.ExtentHeight;
        }

        /// <summary>
        /// When behaviour is detached from ScrollViewer the EventHandler is detached.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_scrollViewer != null)
                _scrollViewer.LayoutUpdated -= ScrollViewerLayoutUpdated;
        }
    }
}
