open System.Drawing
open System.Windows.Forms
open Brahma.OpenCL
open Brahma.FSharp.OpenCL.Core
open Brahma.FSharp.OpenCL.Extensions
open OpenCL.Net
open FSharp.Core

let rec gcd x y =
    if y = 0 then x
    else gcd y (x % y)

let safeGcd x y = 
    let result = gcd x y
    gcd result 32

[<EntryPoint>]
let main _ =
    let img = new Bitmap(@"Resources/warsaw2.jpg")
    let resultImg = new Bitmap(img.Width, img.Height)
    let form = new Form(Visible=true, Height = img.Height, Width = img.Width, StartPosition = FormStartPosition.CenterScreen)
    form.Paint.Add(function e-> e.Graphics.DrawImage(resultImg, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel))
    let provider = ComputeProvider.Create("*", DeviceType.Gpu)
    let mutable commandQueue = new Brahma.OpenCL.CommandQueue(provider, provider.Devices |> Seq.head)
    let stride = img.Height;
    
    let kernel, kernelprepare, kernelrun = provider.Compile(NegativeFilter.negativeCommand stride)
    let gcdSize = safeGcd img.Height img.Width
    let d = _2D(img.Height, img.Width, gcdSize, gcdSize)
    let src = Array.init (img.Width * img.Height) (function i -> ColorExt.packColor(img.GetPixel(i / stride, i % stride)))
    let dst = Array.zeroCreate (img.Width * img.Height)
    kernelprepare d src dst
    commandQueue.Add(kernelrun()) |> ignore
    commandQueue.Add(dst.ToHost provider).Finish() |> ignore
    Array.iteri (fun i (v:uint32) -> resultImg.SetPixel(i / stride, i % stride, ColorExt.unpackColor(v))) dst
    let (value, _) = (OpenCL.Net.Cl.GetDeviceInfo(provider.Devices |> Seq.head, DeviceInfo.MaxWorkItemSizes))
    printfn "%A" value
    System.Windows.Forms.Application.Run(form)
    0