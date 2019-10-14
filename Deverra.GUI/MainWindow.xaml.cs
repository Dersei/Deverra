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

            RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Flyouts;
            WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Flyouts;
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
            using var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bitmap = ((ViewModel)DataContext).OriginalImage = new WriteableBitmap(new BitmapImage(new Uri(openFileDialog.FileName)));
                Width = bitmap.PixelWidth / (double)bitmap.PixelHeight * (Height - OpenButton.ActualHeight - 50);
                ((ViewModel)DataContext).ResultImage = null;
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
        {VM.Filters.Sepia, VM.Filters.Negative, VM.Filters.Sobel,VM.Filters.UltraSobel, VM.Filters.Mean, VM.Filters.Contrast, VM.Filters.Saturation, VM.Filters.Hue, VM.Filters.Log2, VM.Filters.Wave, VM.Filters.Shine};

        private readonly ObservableCollection<IdFilter> _toApply = new ObservableCollection<IdFilter>();

        private void ToApplyListItem_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(IdFilter));
            var droppedData = data is null ? new IdFilter((VM.Filters)(e.Data.GetData(typeof(VM.Filters)) ?? VM.Filters.Sepia)) : (IdFilter)e.Data.GetData(typeof(IdFilter));
            if (droppedData is null) return;
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

            var droppedData = (VM.Filters)(e.Data.GetData(typeof(VM.Filters)) ?? VM.Filters.Sepia);
            _toApply.Add(new IdFilter(droppedData));
            ToApplyList.UpdateLayout();
            ((ListViewItem)ToApplyList.ItemContainerGenerator.ContainerFromIndex(_toApply.Count - 1)).IsSelected = true;

        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((ViewModel)DataContext);
            if (vm.ResultImage is null) return;
            using var saveFileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "Image files | *.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff",
                DefaultExt = ".png",
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (_isFrozen)
                {
                    var writeable = vm.OriginalImage.Clone();
                    writeable.Blit(GetFilteredRect(writeable), vm.ResultImage, GetFilteredRect(writeable), WriteableBitmapExtensions.BlendMode.None);
                    writeable.Save(saveFileDialog.FileName);
                    return;
                }
                vm.ResultImage.Save(saveFileDialog.FileName);
            }

        }

        private Rect GetFilteredRect(WriteableBitmap bitmap)
        {
            return new Rect(GetFilteredX(bitmap), GetFilteredY(bitmap), GetFilteredWidth(bitmap), GetFilteredHeight(bitmap));
        }

        private double GetFilteredX(WriteableBitmap bitmap)
        {
            return _filteredGeometry.Rect.X * bitmap.PixelHeight / FilteredImage.ActualHeight;
        }

        private double GetFilteredY(WriteableBitmap bitmap)
        {
            return _filteredGeometry.Rect.Y * bitmap.PixelWidth / FilteredImage.ActualWidth;
        }

        private double GetFilteredWidth(WriteableBitmap bitmap)
        {
            return _filteredGeometry.Rect.Width * bitmap.PixelHeight / FilteredImage.ActualHeight;
        }

        private double GetFilteredHeight(WriteableBitmap bitmap)
        {
            return _filteredGeometry.Rect.Height * bitmap.PixelWidth / FilteredImage.ActualWidth;
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
            var (result, exception) = ((ViewModel)DataContext).Run();
            if (!result)
            {
                await this.ShowMessageAsync("Error", exception.Message);
                await controller.CloseAsync();
            }
            else
            {
                await controller.CloseAsync();
            }
            timer.Stop();
            Console.WriteLine(timer.Elapsed);
        }
        private void SwitchSides()
        {
            if (_isFrozen)
            {
                _isFrozen = false;
                FilteredImage_OnMouseMove(FilteredImage, new MouseEventArgs(Mouse.PrimaryDevice, 0));
                _isFrozen = true;
            }
            else
            {
                FilteredImage_OnMouseMove(FilteredImage, new MouseEventArgs(Mouse.PrimaryDevice, 0));
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
                SwitchSides();
            }
            if (e.Key == Key.T)
            {
                _isVertical = !_isVertical;
                SwitchSides();
            }
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            HelpFlyout.IsOpen = true;
        }

        private void ReplaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (ViewModel)DataContext;
            if (vm.ResultImage is null) return;
            vm.OriginalImage = vm.ResultImage;
            vm.ResultImage = null;
        }
    }
}
