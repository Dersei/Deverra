module NegativeFilter

open Brahma.OpenCL


let negativeCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
    let i = range.GlobalID0
    let j = range.GlobalID1
    let input = buf.[i + stride * j]
    let r = float32((input >>> 0) &&& 255u);
    let g = float32((input >>> 8) &&& 255u);
    let b = float32((input >>> 16) &&& 255u);
    let tr = 255.0f - r
    let tg = 255.0f - g
    let tb = 255.0f - b
    let color = ((uint32(255) <<< 24) + (uint32(tb) <<< 16) + (uint32(tg) <<< 8) + uint32(tr))
    dst.[i + stride * j] <- color @>