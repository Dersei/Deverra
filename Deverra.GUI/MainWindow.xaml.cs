using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VM;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Deverra.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            FilterList.ItemsSource = _filters;

            var itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMoveEvent)));
            itemContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(FilterList_Drop)));
            FilterList.ItemContainerStyle = itemContainerStyle;
        }

        private void FilteredImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            FilteredImage.Width = position.X;
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bitmap = ((ViewModel)DataContext).OriginalImage = new Bitmap(openFileDialog.FileName);
                Width = bitmap.Width / (double)bitmap.Height * (Height - OpenButton.ActualHeight - 50);
            }
        }

        private void FilterList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var filterList = new List<VM.Filters>();

            for (var i = 0; i < FilterList.Items.Count; i++)
            {
                FilterList.UpdateLayout();
                var item = (ListBoxItem)FilterList.ItemContainerGenerator.ContainerFromIndex(i);
                if (item.IsSelected)
                {
                    filterList.Add((VM.Filters)FilterList.Items[i]);
                }
            }

            ((ViewModel)DataContext).Filters = filterList.ToArray();
        }

        private static void ListBoxItem_PreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {

            if (sender is ListBoxItem draggedItem && e.RightButton == MouseButtonState.Pressed)
            {
                draggedItem.IsSelected = true;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        readonly ObservableCollection<VM.Filters> _filters = new ObservableCollection<VM.Filters>
        {VM.Filters.Sepia, VM.Filters.Negative, VM.Filters.Sobel, VM.Filters.Mean};

        private void FilterList_Drop(object sender, DragEventArgs e)
        {
            var droppedData = (VM.Filters)e.Data.GetData(typeof(VM.Filters));
            var target = (VM.Filters)((ListBoxItem)(sender)).DataContext;

            var removedIdx = FilterList.Items.IndexOf(droppedData);
            var targetIdx = FilterList.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                _filters.Insert(targetIdx + 1, droppedData);
                _filters.RemoveAt(removedIdx);
            }
            else
            {
                var remIdx = removedIdx + 1;
                if (_filters.Count + 1 > remIdx)
                {
                    _filters.Insert(targetIdx, droppedData);
                    _filters.RemoveAt(remIdx);
                }
            }
            FilterList.UpdateLayout();
            ((ListBoxItem)FilterList.ItemContainerGenerator.ContainerFromIndex(targetIdx)).IsSelected = true;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((ViewModel) DataContext);
            if(vm.FilteredImage is null) return;
            var saveFileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "Image files | *.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff",
                DefaultExt = ".png",
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.FilteredImage.Save(saveFileDialog.FileName);
            }

        }
    }
}
