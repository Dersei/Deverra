module Deverra.Main

open FSharp.Core
open ImageForm
open VM
open System
open System.IO
open System.Windows.Media.Imaging
open System.Windows

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

    let filters = args |> Array.skip 1 |> Array.map (fun item -> if item.Contains "=" then parseValueWithRatio(item) else struct ((Enum.TryParse(checkIfEnumCorrect item, true) |> snd), 0))
    
    let img = try 
                new WriteableBitmap(new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)))
              with 
                | :? FileNotFoundException -> failwith "File is not an image";
    let vm = ViewModel(OriginalImage = img, Filters = filters)
    let window = new ImageForm(SizeToContent = SizeToContent.WidthAndHeight, WindowStartupLocation = WindowStartupLocation.CenterScreen)
    vm.Run() |> ignore
    window.Start(img, vm.ResultImage)
    window.ShowDialog() |> ignore
    0