module ImageForm

open System.Windows.Forms
open System

type ImageForm() =
    inherit Form()
    let mutable isFirstShown = true
    let OriginalImage : PictureBox = new PictureBox(Dock = DockStyle.Fill)
    let FilteredImage : PictureBox = new PictureBox(Dock = DockStyle.Fill)

    member __.SaveImage = 
        let sfd = new SaveFileDialog(AddExtension = true,Filter = "Image files | *.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff", DefaultExt = ".png", RestoreDirectory = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
        if sfd.ShowDialog() = DialogResult.OK then FilteredImage.Image.Save(sfd.FileName)

    member this.Start originalImage filteredImage = 
        let container = new Panel(Dock = DockStyle.Fill)
        OriginalImage.Image <- originalImage
        FilteredImage.Image <- filteredImage
        FilteredImage.Visible <- false
        container.Controls.Add OriginalImage
        container.Controls.Add FilteredImage
        this.Controls.Add container
        OriginalImage.Click.Add(function _ ->  OriginalImage.Visible <- not isFirstShown; FilteredImage.Visible <- isFirstShown; isFirstShown <- not isFirstShown;)
        FilteredImage.Click.Add(function _ ->  OriginalImage.Visible <- not isFirstShown; FilteredImage.Visible <- isFirstShown; isFirstShown <- not isFirstShown;)
        this.KeyDown.Add(function e -> if e.Control && e.KeyCode = Keys.S then this.SaveImage else ())