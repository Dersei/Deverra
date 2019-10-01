using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Deverra.GUI
{
/// <summary>
/// Provides extended support for drag drop operation
/// Created by Robert Köpferl https://www.codeproject.com/Tips/635510/WPF-Drag-Drop-Auto-scroll
/// </summary>
public static class DragDropExtension
{
	/*
	 *	ScrollOnDragDrop
	 * 
	 * This property can be added to Controls that contain at least one ScrollViewer.
	 * The code will then determine the first ScrollViewer available and do its task on there.
	 *   
	 * If a Drag operation is ongoing and the mouse cursor reaches the vertical top or bottom
	 * of a list box or any other scrollable control, it will scroll to the top/bottom of it.
	 * The sensitive area is determined to about 40 units or 25% whatever is less.
	 * In case of a item based control like ListBox (ScrollViewer.CanContentScroll==true)
	 * the offset accounts to a max of 3, while it accounts on pixel based controls to 30.
	 * The closer the mouse to the top/bottom the more it accelerates.
	 * The operation interval switches between 300ms (ListBox) to 20ms (other).
	 * 
	 * Set property to true to activate on a control containing scrollable content.
	 * 
	 * Horizontal scrolling is not implemented!
	 * 
	*/


	#region ScrollOnDragDropProperty

    public static readonly DependencyProperty ScrollOnDragDropProperty =
        DependencyProperty.RegisterAttached("ScrollOnDragDrop",
            typeof(bool),
            typeof(DragDropExtension),
            new PropertyMetadata(false, HandleScrollOnDragDropChanged));

    public static bool GetScrollOnDragDrop(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (bool)element.GetValue(ScrollOnDragDropProperty);
    }

    public static void SetScrollOnDragDrop(DependencyObject element, bool value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(ScrollOnDragDropProperty, value);
    }

    private static void HandleScrollOnDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = d as FrameworkElement;

        if (d == null)
        {
            Debug.Fail("Invalid type!");
            return;
        }

        Unsubscribe(container);

        if (true.Equals(e.NewValue))
        {
            Subscribe(container);
        }
    }

    private static void Subscribe(FrameworkElement container)
    {
        container.PreviewDragOver += OnContainerPreviewDragOver;
    }

	private static DateTime _lastTime = DateTime.MinValue;

    private static void OnContainerPreviewDragOver(object sender, DragEventArgs e)
    {
        if (!(sender is FrameworkElement container))
            return;

		// determine Item-wise or content scrolling (by pixel)
	    var itemWise = (bool) container.GetValue(ScrollViewer.CanContentScrollProperty); 


		// record time and execute only so often - store time singular static
		// this does not restrict to scroll at two places at a time (how would that go anyways)
		// but only syncs them in time.... that's fair enough; (300ms for ListBox, 20ms for Content)
	    var span = DateTime.UtcNow - _lastTime;
		if( span.Milliseconds < (itemWise ? 300 : 20))
			return;
		_lastTime = DateTime.UtcNow;

		// digg out the scrollviewer in question
		var scrollViewer = GetFirstVisualChild<ScrollViewer>(container);
		if (scrollViewer == null)
            return;

		//==============//////////// actual begin ================
		// base Tolerance on ActualHeight and make sensitive area relative but at max a constant size
	    const double kMaxTolerance = 40;
		var actualHeight = scrollViewer.ActualHeight;
		// try max 25% of height (4 sml ctrl) and limit to max so the regions don't become too 
		// big but also the sensitive regions never overlap
        var tolerance = Math.Min( kMaxTolerance, actualHeight * 0.25);
		var verticalPos = e.GetPosition(scrollViewer).Y;
        // for list box go as fast as maximum 3 (leave some room to hit->0.35 more) for content jump 30;
        var offset = itemWise ? 3.35 : 30d;

        if (verticalPos < tolerance) // Top of visible list? 
        {
			// accelerate offset * 0..1
	        offset *= ( (tolerance - verticalPos)/tolerance);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up. 
        }
        else if (verticalPos > actualHeight - tolerance) //Bottom of visible list? 
	    {
			// accelerate offset * 0..1
			offset *= ((tolerance - (actualHeight-verticalPos)) / tolerance);
		    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.     
	    }
        
    }

    private static void Unsubscribe(FrameworkElement container)
    {
        container.PreviewDragOver -= OnContainerPreviewDragOver;
    }

    public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T dependencyObject)
                {
                    return dependencyObject;
                }

                var childItem = GetFirstVisualChild<T>(child);
                if (childItem != null)
                {
                    return childItem;
                }
            }
        }

        return null;
    }

    #endregion
}}
