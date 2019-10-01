module ImageForm

open System.Windows
open System.Windows.Controls
open System.Windows.Forms
open System
open System.Windows.Media.Imaging
open System.IO
open System.Windows.Input

type ImageForm() =
    inherit Window()
    let mutable isFirstShown = true
    let mutable filteredResult : WriteableBitmap = null
    let OriginalImage : Image = new Image(HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch)
    let FilteredImage : Image = new Image(HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch)
    let saveImage (image:BitmapSource, filename:string) = 
        let fileStream = new FileStream(filename, FileMode.Create)
        let pngBitmapEncoder = new PngBitmapEncoder();
        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(image));
        pngBitmapEncoder.Save(fileStream);

    let boolToVisibility visibility = 
        if visibility then Visibility.Visible else Visibility.Hidden
    

    member _.SaveImage = 
        let sfd = new SaveFileDialog(AddExtension = true,Filter = "Image files | *.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff", DefaultExt = ".png", RestoreDirectory = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
        if sfd.ShowDialog() = DialogResult.OK then saveImage(filteredResult, sfd.FileName)

    member this.Start (originalImage : WriteableBitmap, filteredImage : WriteableBitmap) = 
        let container = new Grid()
        OriginalImage.Source <- originalImage
        FilteredImage.Source <- filteredImage
        filteredResult <- filteredImage
        FilteredImage.Visibility <- Visibility.Hidden
        container.Children.Add OriginalImage |> ignore
        container.Children.Add FilteredImage |> ignore
        this.Content <- container
        OriginalImage.MouseDown.Add(function _ ->  OriginalImage.Visibility <- boolToVisibility(not isFirstShown); FilteredImage.Visibility <- boolToVisibility isFirstShown; isFirstShown <- not isFirstShown;)
        FilteredImage.MouseDown.Add(function _ ->  OriginalImage.Visibility <- boolToVisibility(not isFirstShown); FilteredImage.Visibility <- boolToVisibility isFirstShown; isFirstShown <- not isFirstShown;)
        this.KeyDown.Add(function e -> if (e.KeyboardDevice.Modifiers &&& ModifierKeys.Control) <> ModifierKeys.None && e.Key = Key.S then this.SaveImage else ())