module Deverra.Main

open System.Drawing
open System.Windows.Forms
open Brahma.OpenCL
open Brahma.FSharp.OpenCL.Core
open Brahma.FSharp.OpenCL.Extensions
open OpenCL.Net
open FSharp.Core
open ImageForm
open Filters
open VM
open System

let rec gcd x y =
    if y = 0 then x
    else gcd y (x % y)

let safeGcd x y max = 
    let result = gcd x y
    gcd result max

let run (img : Bitmap, filter : Filters) = 
    let resultImg = new Bitmap(img.Width, img.Height)
    let provider = ComputeProvider.Create("*", DeviceType.Gpu)
    let mutable commandQueue = new Brahma.OpenCL.CommandQueue(provider, provider.Devices |> Seq.head)
    let stride = img.Height;
    let maxSamplers = (OpenCL.Net.Cl.GetDeviceInfo(provider.Devices |> Seq.head, DeviceInfo.MaxSamplers) |> fst).CastTo<int>()
    
    let kernel, kernelprepare, kernelrun = match filter with 
                                            | Filters.Sepia -> provider.Compile(SepiaFilter.sepiaCommand stride)
                                            | Filters.Negative -> provider.Compile(NegativeFilter.negativeCommand stride)
                                            | Filters.Sobel -> provider.Compile(SobelFilter.sobelCommand stride)
                                            | Filters.Mean -> provider.Compile(MeanFilter.meanCommand stride)
                                            | _ -> failwith "Wrong filter"

    let gcdSize = safeGcd img.Height img.Width maxSamplers
    let d = _2D(img.Height, img.Width, gcdSize, gcdSize)
    let src = Array.init (img.Width * img.Height) (function i -> ColorExt.packColor(img.GetPixel(i / stride, i % stride)))
    let dst = Array.zeroCreate (img.Width * img.Height)
    kernelprepare d src dst
    commandQueue.Add(kernelrun()) |> ignore
    commandQueue.Add(dst.ToHost provider).Finish() |> ignore
    Array.iteri (fun i (v:uint32) -> resultImg.SetPixel(i / stride, i % stride, ColorExt.unpackColor(v))) dst
    resultImg
    

[<EntryPoint>]
let main args =
    let (path, filter)  = match args.Length with
                                | 1 -> (args.[0], Filters.Sepia)
                                | 2 -> (args.[0], Enum.TryParse(args.[1]) |> snd)
                                | _ -> failwith "Wrong args"

    let img = new Bitmap(path)
    let vm = ViewModel(OriginalImage = img, Filter = filter)
    let form = new ImageForm(Visible=true, Height = img.Height, Width = img.Width, StartPosition = FormStartPosition.CenterScreen)
    vm.Run()
    form.Start img vm.FilteredImage
    System.Windows.Forms.Application.Run(form)
    0