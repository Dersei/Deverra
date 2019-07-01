namespace VM

open System.Drawing
open Brahma.OpenCL
open Brahma.FSharp.OpenCL.Core
open Brahma.FSharp.OpenCL.Extensions
open OpenCL.Net
open FSharp.Core
open System.Windows.Input
open System.Diagnostics
open Filters
open System.Windows.Media.Imaging

type Filters = Sepia = 0 | Negative  = 1 | Sobel = 2 | UltraSobel = 3 | Mean = 4 | Contrast = 5 
type WBeX = WriteableBitmapExtensions


type public ViewModel() =
    inherit ViewModelBase()

    let mutable originalImage : WriteableBitmap = null
    let mutable runCommand : ICommand = null
    let mutable filters : struct (Filters * int)[] = Array.empty
    let mutable resultImage : WriteableBitmap = null

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
    member this.ResultImage 
        with get() = resultImage
        and set(value) = 
            resultImage <- value
            this.OnPropertyChanged(<@ this.ResultImage @>)
    member __.Filters
        with get() = filters
        and set(value) = filters <- value

    member this.RunCommand
        with get() = if runCommand  = null then 
                        runCommand <- this.createCommand (fun _ -> this.Run()) (fun _ -> true)
                        runCommand
                        else runCommand
        and set(value) =
            runCommand <- value

    member this.Run() = 
        let provider = ComputeProvider.Create("*", DeviceType.Gpu)
        let mutable commandQueue = new Brahma.OpenCL.CommandQueue(provider, provider.Devices |> Seq.head)
        let stride = originalImage.PixelHeight;
        let maxSamplers = (OpenCL.Net.Cl.GetDeviceInfo(provider.Devices |> Seq.head, DeviceInfo.MaxSamplers) |> fst).CastTo<int>()
        let mutable kernels : Kernel<_2D> list = List.empty
        let mutable kernelprepares : (_2D -> uint32 array -> uint32 array -> unit) list = List.empty
        let mutable kernelruns : (unit -> Commands.Run<_2D>) list = List.empty
        this.Filters <- if this.Filters |> Array.isEmpty then [|(Filters.Sepia,0)|] else this.Filters
        for filter in this.Filters do
            let kernel, kernelprepare, kernelrun = match filter with 
                                                    | (Filters.Sepia, _) -> provider.Compile(SepiaFilter.sepiaCommand stride)
                                                    | (Filters.Negative, _) -> provider.Compile(NegativeFilter.negativeCommand stride)
                                                    | (Filters.Sobel, _) -> provider.Compile(SobelFilter.sobelCommand stride)
                                                    | (Filters.UltraSobel, _) -> provider.Compile(UltraSobelFilter.ultraSobelCommand stride)
                                                    | (Filters.Mean, _) -> provider.Compile(MeanFilter.meanCommand stride)
                                                    | (Filters.Contrast, ratio) -> provider.Compile(ContrastFilter.contrastCommand stride ratio)
                                                    | _ -> failwith "Wrong filter" 
            kernels <- kernels @ [kernel]
            kernelprepares <- kernelprepares @ [kernelprepare]
            kernelruns <- kernelruns @ [kernelrun]
        

        let gcdSize = safeGcd originalImage.PixelHeight originalImage.PixelWidth maxSamplers
        let d = _2D(originalImage.PixelHeight, originalImage.PixelWidth, gcdSize, gcdSize)
        let timerSrc = new Stopwatch()
        timerSrc.Start()
        let src = (originalImage.Rotate(90).ToByteArray() |> ColorExt.packBytes)
        timerSrc.Stop()
        printfn "Finished reading %A" timerSrc.Elapsed
        let dst = Array.zeroCreate (originalImage.PixelWidth * originalImage.PixelHeight)
        kernelprepares.Head d src dst
        kernelprepares |> List.skip 1 |> List.iter (fun item -> item d dst dst)

        let timer = new Stopwatch()
        timer.Start()
        kernelruns |> List.iter (fun item -> commandQueue.Add(item()).Finish |> ignore) 
        commandQueue.Add(dst.ToHost provider).Finish() |> ignore
        timer.Stop()
        printfn "Finished processsing %A" timer.Elapsed
        commandQueue.Dispose()
        provider.CloseAllBuffers()
        provider.Dispose()
        printfn "Finished writing image"
        let wbm = WriteableBitmap(originalImage.PixelHeight, originalImage.PixelWidth, 32.0, 32.0, System.Windows.Media.PixelFormats.Bgra32, null)
        this.ResultImage <- wbm.FromByteArray(dst |> ColorExt.createByteArray).Rotate(270)