module Deverra.Main

open System.Drawing
open System.Windows.Forms
open FSharp.Core
open ImageForm
open VM
open System
open System.IO
open System.Windows.Media.Imaging

let filterValues = Filters.GetNames(typeof<Filters>) |> Array.map (fun item -> item.ToLowerInvariant());

let checkIfEnumCorrect (value : string) = if filterValues |> Array.contains(value.ToLower()) then value else failwith "Incorrect filter type"

let checkIfValueCorrect (value : string) = let result = Int32.TryParse(value)
                                           if result |> fst then result |> snd else failwith "Incorrect ratio value"

let parseValueWithRatio (value : string) = 
                                        let split = value.Split '=' 
                                        let filter = Enum.TryParse(checkIfEnumCorrect split.[0], true) |> snd
                                        let ratio = checkIfValueCorrect split.[1]
                                        struct (filter, ratio)

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
    let filters = args |> Array.skip 1 |> Array.map (fun item -> if item.Contains "=" then parseValueWithRatio(item) else struct ((Enum.TryParse(checkIfEnumCorrect item, true) |> snd), 0))
    
    let img = try 
                new WriteableBitmap(new BitmapImage(new Uri(path)))
              with 
                | :? FileNotFoundException -> failwith "File is not a image";
    let vm = ViewModel(OriginalImage = img, Filters = filters)
    //let form = new ImageForm(Visible=true, Height = img.Height, Width = img.Width, StartPosition = FormStartPosition.CenterScreen)
    vm.Run()
    //form.Start img vm.FilteredImage
    //System.Windows.Forms.Application.Run(form)
    //img.Dispose()
    0