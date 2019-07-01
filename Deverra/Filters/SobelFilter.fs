namespace Filters

open Brahma.OpenCL

module SobelFilter =
    let sobelCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
        let i = range.GlobalID0
        let j = range.GlobalID1
        let mutable h = 0.
        if (i > 0 && i < stride) then
            let left = float(buf.[i - 1 + stride * j] &&& 255u)
            let right = float(buf.[i + 1 + stride * j] &&& 255u)
            h <- abs ((left - right))
        let mutable v = 0.
        if (j > 0) then
            let top = float(buf.[i + stride * (j - 1)] &&& 255u)
            let bottom = float(buf.[i + stride * (j + 1)] &&& 255u)
            v <- abs ((top - bottom))
        let result =  byte((sqrt (h * h + v * v)))
        let color = ((uint32(255) <<< 24) + (uint32(result) <<< 16) + (uint32(result) <<< 8) + uint32(result))
        dst.[i + stride * j] <- color @>