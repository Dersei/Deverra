namespace Filters

open Brahma.OpenCL

module MeanFilter = 
    let meanCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
        let i = range.GlobalID0
        let j = range.GlobalID1
        let mutable hR = (buf.[i + stride * j] &&& 255u)
        let mutable hG =((buf.[i + stride * j] >>> 8) &&& 255u)
        let mutable hB = ((buf.[i + stride * j] >>> 16) &&& 255u)
        if (i > 0 && i < stride) then
            let leftR = (buf.[i - 1 + stride * j] &&& 255u)
            let leftG = ((buf.[i - 1 + stride * j] >>> 8) &&& 255u)
            let leftB = ((buf.[i - 1 + stride * j] >>> 16) &&& 255u)
            let rightR = (buf.[i + 1 + stride * j] &&& 255u)
            let rightG = ((buf.[i + 1 + stride * j] >>> 8) &&& 255u)
            let rightB = ((buf.[i + 1 + stride * j] >>> 16) &&& 255u)
            hR <- hR + leftR + rightR
            hG <- hG + leftG + rightG
            hB <- hB + leftB + rightB
        if (i > 0 && i < stride && j > 0) then
            let leftTopR = (buf.[i - 1 + stride * (j - 1)] &&& 255u)
            let leftTopG = ((buf.[i - 1 + stride* (j - 1)] >>> 8) &&& 255u)
            let leftTopB = ((buf.[i - 1 + stride * (j - 1)] >>> 16) &&& 255u)
            let rightTopR = (buf.[i + 1 + stride * (j - 1)] &&& 255u)
            let rightTopG = ((buf.[i + 1 + stride * (j - 1)] >>> 8) &&& 255u)
            let rightTopB = ((buf.[i + 1 + stride * (j - 1)] >>> 16) &&& 255u)
            hR <- hR + leftTopR + rightTopR
            hG <- hG + leftTopG + rightTopG
            hB <- hB + leftTopB + rightTopB
        if (i > 0 && i < stride && j > 0) then
            let leftBottomR = (buf.[i - 1 + stride * (j + 1)] &&& 255u)
            let leftBottomG = ((buf.[i - 1 + stride* (j + 1)] >>> 8) &&& 255u)
            let leftBottomB = ((buf.[i - 1 + stride * (j + 1)] >>> 16) &&& 255u)
            let rightBottomR = (buf.[i + 1 + stride * (j + 1)] &&& 255u)
            let rightBottomG = ((buf.[i + 1 + stride * (j + 1)] >>> 8) &&& 255u)
            let rightBottomB = ((buf.[i + 1 + stride * (j + 1)] >>> 16) &&& 255u)
            hR <- hR + leftBottomR + rightBottomR
            hG <- hG + leftBottomG + rightBottomG
            hB <- hB + leftBottomB + rightBottomB
        if (j > 0) then
            let topR = (buf.[i + stride * (j - 1)] &&& 255u)
            let topG = ((buf.[i + stride * (j - 1)] >>> 8) &&& 255u)
            let topB = ((buf.[i + stride * (j - 1)] >>> 16) &&& 255u)
            let bottomR = (buf.[i + stride * (j + 1)] &&& 255u)
            let bottomG = ((buf.[i + stride * (j + 1)] >>> 8) &&& 255u)
            let bottomB = ((buf.[i + stride * (j + 1)] >>> 16) &&& 255u)
            hR <- hR + topR + bottomR
            hG <- hG + topG + bottomG
            hB <- hB + topB + bottomB
        let result = ((uint32(255) <<< 24) + (uint32(hB / 9u) <<< 16) + (uint32(hG / 9u) <<< 8) + uint32(hR / 9u));
        dst.[i + stride * j] <- result @>