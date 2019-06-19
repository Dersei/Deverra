namespace Filters

open Brahma.OpenCL

module ContrastFilter = 
    let contrastCommand stride (contrast:int) = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
           let i = range.GlobalID0
           let j = range.GlobalID1
           let cc = float32(contrast)
           let factor = ((259.0f * (cc + 255.0f)) / (255.0f * (259.0f - cc)))
           let truncate (color : float32) = 
                                            let mutable result = 0.0f
                                            if color > 0.0f then result <- color else result <- 0.0f;
                                            if result < 255.0f then result <- result else result <- 255.0f;
                                            result
           let input = buf.[i + stride * j]
           let r = float32((input >>> 0) &&& 255u);
           let g = float32((input >>> 8) &&& 255u);
           let b = float32((input >>> 16) &&& 255u);
           let tr = truncate (factor * (r - 128.0f) + 128.0f)
           let tg = truncate (factor * (g - 128.0f) + 128.0f)
           let tb = truncate (factor * (b - 128.0f) + 128.0f)
           let color = ((uint32(255) <<< 24) + (uint32(tb) <<< 16) + (uint32(tg) <<< 8) + uint32(tr))
           dst.[i + stride * j] <- color @>