namespace Filters

open Brahma.OpenCL

module UltraSobelFilter =
    let ultraSobelCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
        let i = range.GlobalID0
        let j = range.GlobalID1
        let truncate (color : float) = 
            let mutable result = 0.0
            if color > 0.0 then result <- color else result <- 0.0;
            if result < 255.0 then result <- result else result <- 255.0;
            result
        let mutable h = 0.
        if (i > 0 && i < stride) then
            let left = (buf.[i - 1 + stride * j] &&& 255u)
            let right = (buf.[i + 1 + stride * j] &&& 255u)
            h <- abs (float(left - right))
        let mutable v = 0.
        if (j > 0) then
            let top = (buf.[i + stride * (j - 1)] &&& 255u)
            let bottom = (buf.[i + stride * (j + 1)] &&& 255u)
            v <- abs (float(top - bottom))
        let result = truncate ((sqrt (h * h + v * v)))
        let color = ((uint32(255) <<< 24) + (uint32(result) <<< 16) + (uint32(result) <<< 8) + uint32(result))
        dst.[i + stride * j] <- color @>