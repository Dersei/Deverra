module ImageForm

open System.Windows.Forms

type ImageForm() =
    inherit Form()
    let mutable isFirstShown = true
    let mutable OriginalImage : PictureBox = new PictureBox(Dock = DockStyle.Fill)
    let mutable FilteredImage : PictureBox = new PictureBox(Dock = DockStyle.Fill)

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