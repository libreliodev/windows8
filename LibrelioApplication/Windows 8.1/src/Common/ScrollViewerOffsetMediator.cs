using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace LibrelioApplication.Common
{
    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            try
            {
                dynamic _Item = item;
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, _Item.ColSpan);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, _Item.RowSpan);
            }
            catch
            {
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            finally
            {
                base.PrepareContainerForItemOverride(element, item);
            }
        }
    }



    /// <summary>
    /// Mediator that forwards Offset property changes on to a ScrollViewer
    /// instance to enable the animation of Horizontal/VerticalOffset.
    /// </summary>
    public class ScrollViewerOffsetMediator : FrameworkElement
    {
        /// <summary>
        /// ScrollViewer instance to forward Offset changes on to.
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register(
                "ScrollViewer",
                typeof(ScrollViewer),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(default(ScrollViewer), new PropertyChangedCallback(OnScrollViewerChanged)));
        private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var mediator = (ScrollViewerOffsetMediator)o;
            var scrollViewer = (ScrollViewer)(e.NewValue);
            if (null != scrollViewer)
            {
                scrollViewer.ChangeView(mediator.HorizontalOffset, null, null);
            }
        }

        /// <summary>
        /// VerticalOffset property to forward to the ScrollViewer.
        /// </summary>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(
                "HorizontalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(default(double), new PropertyChangedCallback(OnHorizontalOffsetChanged)));
        public static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var mediator = (ScrollViewerOffsetMediator)o;
            if (null != mediator.ScrollViewer)
            {
                mediator.ScrollViewer.ChangeView((double)(e.NewValue), null, null);
            }
        }

        /// <summary>
        /// Multiplier for ScrollableHeight property to forward to the ScrollViewer.
        /// </summary>
        /// <remarks>
        /// 0.0 means "scrolled to top"; 1.0 means "scrolled to bottom".
        /// </remarks>
        public double ScrollableWidthMultiplier
        {
            get { return (double)GetValue(ScrollableWidthMultiplierProperty); }
            set { SetValue(ScrollableWidthMultiplierProperty, value); }
        }
        public static readonly DependencyProperty ScrollableWidthMultiplierProperty =
            DependencyProperty.Register(
                "ScrollableWidthMultiplier",
                typeof(double),
                typeof(ScrollViewerOffsetMediator),
                new PropertyMetadata(default(double), new PropertyChangedCallback(OnScrollableWidthMultiplierChanged)));
        public static void OnScrollableWidthMultiplierChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var mediator = (ScrollViewerOffsetMediator)o;
            var scrollViewer = mediator.ScrollViewer;
            if (null != scrollViewer)
            {
                scrollViewer.ChangeView((double)(e.NewValue) * scrollViewer.ScrollableWidth, null, null);
            }
        }
    }
}
