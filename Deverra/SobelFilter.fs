module SobelFilter

open Brahma.OpenCL

let sobelCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) (check:int) ->
    let i = range.GlobalID0
    let j = range.GlobalID1
    let mutable h = float32 0.
    if (i > 0 && i < stride) then
        let left = float32 (buf.[i - 1 + stride * j] &&& 255u)
        let right = float32 (buf.[i + 1 + stride * j] &&& 255u)
        h <- abs (left - right)
    let mutable v = float32 0.
    if (j > 0) then
        let top = float32 (buf.[i + stride * (j - 1)] &&& 255u)
        let bottom = float32 (buf.[i + stride * (j + 1)] &&& 255u)
        v <- abs (top - bottom)
    let result = byte((sqrt (h * h + v * v)))
    let mutable color = 0u
    if(check=0) then color <- ((uint32(255) <<< 24) + (uint32(result) <<< 16) + (uint32(result) <<< 8) + uint32(result))
    else color <- ((uint32(255) <<< 24) + (uint32(0) <<< 16) + (uint32(result) <<< 8) + uint32(0))
    dst.[i + stride * j] <- color @>