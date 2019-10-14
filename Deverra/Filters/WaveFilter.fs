namespace Filters

open Brahma.OpenCL

module WaveFilter = 
    let waveCommand stride ratio = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
            let i = range.GlobalID0
            let j = range.GlobalID1
            let timeAcceleration = 15.0;
            let utime = 1000.0;
            let waveRadius = ratio;
            let stepVal = (utime * timeAcceleration) + float(i) * 61.8;
            let offset = cos(stepVal) * waveRadius;
            let input = buf.[i + stride * j + int(offset)]
            let r = float((input >>> 0) &&& 255u);
            let g = float((input >>> 8) &&& 255u);
            let b = float((input >>> 16) &&& 255u);
            let a = float((input >>> 24) &&& 255u);
            let color = ((uint32(a) <<< 24) + (uint32(b) <<< 16) + (uint32(g) <<< 8) + uint32(r))
            dst.[i + stride * j] <- color @>