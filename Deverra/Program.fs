module Deverra.Main

open System.Drawing
open System.Windows.Forms
open FSharp.Core
open ImageForm
open VM
open System

let rec gcd x y =
    if y = 0 then x
    else gcd y (x % y)

let safeGcd x y max = 
    let result = gcd x y
    gcd result max

[<EntryPoint>]
let main args =
    let path = match args.Length with
               | 0 -> failwith "Path not given"
               | _ -> args.[0]

    let filterValues = Filters.GetNames(typeof<Filters>) |> Array.map (fun item -> item.ToLowerInvariant());
    if (args |> Array.skip 1 |> Array.exists (fun item ->  filterValues |> Array.contains item)) then failwith "Wrong filter"
    let filters = args |> Array.skip 1 |> Array.map (fun item -> Enum.TryParse(item, true) |> snd)  

    let img = new Bitmap(path)
    let vm = ViewModel(OriginalImage = img, Filters = filters)
    let form = new ImageForm(Visible=true, Height = img.Height, Width = img.Width, StartPosition = FormStartPosition.CenterScreen)
    vm.Run()
    form.Start img vm.FilteredImage
    System.Windows.Forms.Application.Run(form)
    0