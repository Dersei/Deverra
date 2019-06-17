﻿namespace VM

open System.Drawing
open Brahma.OpenCL
open Brahma.FSharp.OpenCL.Core
open Brahma.FSharp.OpenCL.Extensions
open OpenCL.Net
open FSharp.Core
open System.Windows.Input
open System.Diagnostics
open Filters

type Filters = Sepia = 0 | Negative  = 1 | Sobel = 2 | Mean = 3


type public ViewModel() =
    inherit ViewModelBase()

    let mutable originalImage : Bitmap = null
    let mutable filteredImage : Bitmap = null
    let mutable filter : Filters = Filters.Sepia
    let mutable runCommand : ICommand = null

    let rec gcd x y =
        if y = 0 then x
        else gcd y (x % y)
    
    let safeGcd x y max = 
        let result = gcd x y
        gcd result max
    
    member this.OriginalImage 
        with get() = originalImage
        and set(value) = 
            originalImage <- value 
            this.OnPropertyChanged(<@ this.OriginalImage @>)
    member this.FilteredImage 
        with get() = filteredImage
        and set(value) = 
            filteredImage <- value
            this.OnPropertyChanged(<@ this.FilteredImage @>)
    member this.Filter 
        with get() = filter
        and set(value) = 
            filter <- value
            this.OnPropertyChanged(<@ this.Filter @>)
    

    member this.RunCommand
        with get() = if runCommand  = null then 
                        runCommand <- this.createCommand (fun _ -> this.Run()) (fun _ -> true)
                        runCommand
                        else runCommand
        and set(value) =
            runCommand <- value

    member this.Run() = 
        let resultImg = new Bitmap(originalImage.Width, originalImage.Height)
        let provider = ComputeProvider.Create("*", DeviceType.Gpu)
        let mutable commandQueue = new Brahma.OpenCL.CommandQueue(provider, provider.Devices |> Seq.head)
        let stride = originalImage.Height;
        let maxSamplers = (OpenCL.Net.Cl.GetDeviceInfo(provider.Devices |> Seq.head, DeviceInfo.MaxSamplers) |> fst).CastTo<int>()
    
        let kernel, kernelprepare, kernelrun = match filter with 
                                                | Filters.Sepia -> provider.Compile(SepiaFilter.sepiaCommand stride)
                                                | Filters.Negative -> provider.Compile(NegativeFilter.negativeCommand stride)
                                                | Filters.Sobel -> provider.Compile(SobelFilter.sobelCommand stride)
                                                | Filters.Mean -> provider.Compile(MeanFilter.meanCommand stride)
                                                | _ -> failwith "Wrong filter"

        let gcdSize = safeGcd originalImage.Height originalImage.Width maxSamplers
        let d = _2D(originalImage.Height, originalImage.Width, gcdSize, gcdSize)
        let src = Array.init (originalImage.Width * originalImage.Height) (function i -> ColorExt.packColor(originalImage.GetPixel(i / stride, i % stride)))
        let dst = Array.zeroCreate (originalImage.Width * originalImage.Height)
        kernelprepare d src dst
        let timer = new Stopwatch()
        timer.Start()
        commandQueue.Add(kernelrun()).Finish() |> ignore
        commandQueue.Add(dst.ToHost provider).Finish() |> ignore
        timer.Stop()
        printfn "%A" timer.Elapsed
        Array.iteri (fun i (v:uint32) -> resultImg.SetPixel(i / stride, i % stride, ColorExt.unpackColor(v))) dst
        commandQueue.Dispose()
        provider.CloseAllBuffers()
        provider.Dispose()
        this.FilteredImage <- resultImg