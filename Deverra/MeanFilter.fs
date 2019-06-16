module MeanFilter

open Brahma.OpenCL

let meanCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
    let i = range.GlobalID0
    let j = range.GlobalID1
    let mutable hR = 0u
    let mutable hG = 0u
    let mutable hB = 0u
    if (i > 0 && i < stride) then
        let leftR = (buf.[i - 1 + stride * j] &&& 255u)
        let leftG = ((buf.[i - 1 + stride * j] >>> 8) &&& 255u)
        let leftB = ((buf.[i - 1 + stride * j] >>> 16) &&& 255u)
        let rightR = (buf.[i + 1 + stride * j] &&& 255u)
        let rightG = ((buf.[i + 1 + stride * j] >>> 8) &&& 255u)
        let rightB = ((buf.[i + 1 + stride * j] >>> 16) &&& 255u)
        hR <- leftR + rightR
        hG <- leftG + rightG
        hB <- leftB + rightB
    if (j > 0) then
        let topR = (buf.[i + stride * (j - 1)] &&& 255u)
        let topG = ((buf.[i + stride * (j - 1)] >>> 8) &&& 255u)
        let topB = ((buf.[i + stride * (j - 1)] >>> 16) &&& 255u)
        let bottomR = (buf.[i + stride * (j + 1)] &&& 255u)
        let bottomG = ((buf.[i + stride * (j + 1)] >>> 8) &&& 255u)
        let bottomB = ((buf.[i + stride * (j + 1)] >>> 16) &&& 255u)
        hR <- topR + bottomR
        hG <- topG + bottomG
        hB <- topB + bottomB
    let result = ((uint32(255) <<< 24) + (uint32(hB / 4u) <<< 16) + (uint32(hG / 4u) <<< 8) + uint32(hR / 4u));
    dst.[i + stride * j] <- result @>