module Deverra.Main

open System.Drawing
open System.Windows.Forms
open FSharp.Core
open ImageForm
open VM
open System
open System.IO

[<EntryPoint>]
[<STAThread>]
let main args =
    let path = match args.Length with
               | 0 -> failwith "Path not given"  
               | _ -> 
                    match File.Exists(args.[0]) with 
                    | true -> args.[0]
                    | _ -> failwith "Wrong path"  
               
    let filterValues = Filters.GetNames(typeof<Filters>) |> Array.map (fun item -> item.ToLowerInvariant());
    if (args |> Array.skip 1 |> Array.exists (fun item ->  filterValues |> Array.contains item && not (Int32.TryParse(item) |> fst))) then failwith "Wrong filter"
    let filters = args |> Array.skip 1 |> Array.map (fun item -> Enum.TryParse(item, true) |> snd)  

    let img = try 
                new Bitmap(path) 
              with 
                | :? FileNotFoundException -> failwith "File is not a image";
    let vm = ViewModel(OriginalImage = img, Filters = filters, Ratio = 100)
    let form = new ImageForm(Visible=true, Height = img.Height, Width = img.Width, StartPosition = FormStartPosition.CenterScreen)
    vm.Run()
    form.Start img vm.FilteredImage
    System.Windows.Forms.Application.Run(form)
    img.Dispose()
    0