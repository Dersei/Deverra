using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
            ToApplyList.ItemsSource = _toApply;

            var toApplyContainerStyle = new Style(typeof(ListViewItem));
            toApplyContainerStyle.Setters.Add(new EventSetter(PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMoveEvent)));
            toApplyContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(ToApplyListItem_Drop)));
            toApplyContainerStyle.Setters.Add(new EventSetter(PreviewMouseRightButtonUpEvent, new MouseButtonEventHandler(ToApplyListItem_OnMouseRightButtonUpPreview)));
            var filtersContainerStyle = new Style(typeof(ListViewItem));
            filtersContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
            filtersContainerStyle.Setters.Add(new EventSetter(PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMoveEvent)));
            FilterList.ItemContainerStyle = filtersContainerStyle;
            ToApplyList.ItemContainerStyle = toApplyContainerStyle;
            _filteredGeometry = new RectangleGeometry(new Rect(new Size(FilteredImage.ActualWidth, FilteredImage.ActualHeight)));
            FilteredImage.Clip = _filteredGeometry;
        }

        private readonly RectangleGeometry _filteredGeometry;
        private void FilteredImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isFrozen) return;
            var position = e.GetPosition(FilteredImage);

            if (!_isReversed && !_isVertical)
            {
                _filteredGeometry.Rect = new Rect(0, 0, position.X, FilteredImage.ActualHeight);
            }
            else if (!_isVertical && FilteredImage.ActualWidth - position.X > 0)
            {
                _filteredGeometry.Rect = new Rect(position.X, 0, FilteredImage.ActualWidth - position.X, FilteredImage.ActualHeight);
            }
            else if (!_isReversed && _isVertical && FilteredImage.ActualHeight - position.Y > 0)
            {
                _filteredGeometry.Rect = new Rect(0, position.Y, FilteredImage.ActualWidth, FilteredImage.ActualHeight - position.Y);
            }
            else if (_isVertical && FilteredImage.ActualHeight - position.Y > 0)
            {
                _filteredGeometry.Rect = new Rect(0, 0, FilteredImage.ActualWidth, position.Y);
            }
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bitmap = ((ViewModel)DataContext).OriginalImage = new WriteableBitmap(new BitmapImage(new Uri(openFileDialog.FileName)));
                Width = bitmap.PixelWidth / (double)bitmap.PixelHeight * (Height - OpenButton.ActualHeight - 50);
            }
        }

        private bool _isInside;
        private void ListBoxItem_PreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is TextBox)
            {
                e.Handled = true;
                return;
            }

            if (sender is ListViewItem draggedItem && e.LeftButton == MouseButtonState.Pressed)
            {
                ArrowLabel.Visibility = Visibility.Collapsed;
                _isInside = draggedItem.GetParentObject().GetParentObject().GetParentObject().GetParentObject().GetParentObject().GetParentObject().GetParentObject().GetParentObject() == ToApplyList;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        private readonly ObservableCollection<VM.Filters> _filters = new ObservableCollection<VM.Filters>
        {VM.Filters.Sepia, VM.Filters.Negative, VM.Filters.Sobel,VM.Filters.UltraSobel, VM.Filters.Mean, VM.Filters.Contrast};

        private readonly ObservableCollection<IdFilter> _toApply = new ObservableCollection<IdFilter>();

        private void ToApplyListItem_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(IdFilter));
            var droppedData = data is null ? new IdFilter((VM.Filters)e.Data.GetData(typeof(VM.Filters))) : (IdFilter)e.Data.GetData(typeof(IdFilter));
            var target = (IdFilter)((ListViewItem)sender).DataContext;
            e.Handled = true;
            if (data is null)
            {
                _toApply.Add(droppedData);
                ToApplyList.UpdateLayout();
                return;
            }

            var removedIdx = ToApplyList.Items.IndexOf(droppedData);
            var targetIdx = ToApplyList.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                _toApply.Insert(targetIdx + 1, droppedData);
                _toApply.RemoveAt(removedIdx);
            }
            else
            {
                var remIdx = removedIdx + 1;
                if (_toApply.Count + 1 > remIdx)
                {
                    _toApply.Insert(targetIdx, droppedData);
                    _toApply.RemoveAt(remIdx);
                }
            }
            ToApplyList.UpdateLayout();
        }

        private void ToApplyList_Drop(object sender, DragEventArgs e)
        {
            if (_isInside)
            {
                _isInside = false;
                var droppedDataInside = (IdFilter)e.Data.GetData(typeof(IdFilter));
                _toApply.Remove(droppedDataInside);
                _toApply.Add(droppedDataInside);
                return;
            }
            var droppedData = (VM.Filters)e.Data.GetData(typeof(VM.Filters));
            _toApply.Add(new IdFilter(droppedData));
            ToApplyList.UpdateLayout();
            ((ListViewItem)ToApplyList.ItemContainerGenerator.ContainerFromIndex(_toApply.Count - 1)).IsSelected = true;

        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((ViewModel)DataContext);
            if (vm.ResultImage is null) return;
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
                if (_isFrozen)
                {
                    var writeable = vm.OriginalImage.Clone();
                    var width = GetFilteredWidth(writeable);
                    writeable.Blit(new Rect(0, 0, GetFilteredWidth(writeable), writeable.PixelHeight), vm.ResultImage, new Rect(new Size(GetFilteredWidth(writeable), vm.ResultImage.PixelHeight)), WriteableBitmapExtensions.BlendMode.None);
                    writeable.Save(saveFileDialog.FileName);
                    return;
                }
                vm.ResultImage.Save(saveFileDialog.FileName);
            }

        }

        private double GetFilteredWidth(WriteableBitmap bitmap)
        {
            return FilteredImage.Width * bitmap.PixelHeight / FilteredImage.ActualHeight;
        }

        private void ToApplyListItem_OnMouseRightButtonUpPreview(object sender, MouseButtonEventArgs e)
        {
            var target = (IdFilter)((ListViewItem)(sender)).DataContext;
            _toApply.Remove(target);
        }

        private void RatioBox_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var timer = new Stopwatch();
            timer.Start();
            ((ViewModel)DataContext).Filters = ToApplyList.Items.Cast<IdFilter>().Select(filter => ((VM.Filters)filter, filter.Ratio)).ToArray();
            var controller = await this.ShowProgressAsync("Processing...", "Processing...");
            ((ViewModel)DataContext).Run();
            await controller.CloseAsync();
            timer.Stop();
            Console.WriteLine(timer.Elapsed);
        }

        private class IdFilter : IEquatable<IdFilter>
        {
            private readonly Guid Id;
            public VM.Filters Filter { get; }
            public Visibility Visibility { get; }
            public int Ratio { get; set; }

            public IdFilter(VM.Filters filter)
            {
                Id = Guid.NewGuid();
                Filter = filter;
                Visibility = filter == VM.Filters.Contrast ? Visibility.Visible : Visibility.Collapsed;
                Ratio = 0;
            }

            public static implicit operator VM.Filters(IdFilter idFilter)
            {
                return idFilter.Filter;
            }

            public override string ToString()
            {
                return Filter.ToString();
            }

            public bool Equals(IdFilter other)
            {
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                return obj is IdFilter other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }


        private bool _isFrozen;
        private bool _isReversed;
        private bool _isVertical;
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                _isFrozen = !_isFrozen;
            }
            if (e.Key == Key.R)
            {
                _isReversed = !_isReversed;
                FilteredImage_OnMouseMove(FilteredImage, new MouseEventArgs(Mouse.PrimaryDevice, 0));
            }
            if (e.Key == Key.T)
            {
                _isVertical = !_isVertical;
                FilteredImage_OnMouseMove(FilteredImage, new MouseEventArgs(Mouse.PrimaryDevice, 0));
            }
        }
    }
}
