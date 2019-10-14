namespace Filters

open Brahma.OpenCL

module Log2Filter = 
    let log2Command stride ratio = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
            let i = range.GlobalID0
            let j = range.GlobalID1
            let input = buf.[i + stride * j]
            let r = float((input >>> 0) &&& 255u);
            let g = float((input >>> 8) &&& 255u);
            let b = float((input >>> 16) &&& 255u);
            let color = ((uint32(255) <<< 24) + (uint32(log(1.0 + b)/ratio) <<< 16) + (uint32(log(1.0 + g)/ratio) <<< 8) + uint32(log(1.0 + r)/ratio))
            dst.[i + stride * j] <- color @>