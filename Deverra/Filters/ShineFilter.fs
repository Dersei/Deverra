namespace Filters

open Brahma.OpenCL

module ShineFilter = 
    let shineCommand stride (ratio : int) = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
            let i = range.GlobalID0
            let j = range.GlobalID1
            let input = buf.[i + stride * j]
            let mutable r = float((input >>> 0) &&& 255u) * 255.0;
            let mutable g = float((input >>> 8) &&& 255u) * 255.0;
            let mutable b = float((input >>> 16) &&& 255u) * 255.0;
            let a = float((input >>> 24) &&& 255u) * 255.0;
            let R = ratio;
            let gray  = (r + g + b) / 3.0;
            let every = float((int(gray) / R) * R);
            r <- every / 255.0
            g <- every / 255.0
            b <- every / 255.0
            let color = ((uint32(a) <<< 24) + (uint32(b) <<< 16) + (uint32(g) <<< 8) + uint32(r))
            dst.[i + stride * j] <- color @>