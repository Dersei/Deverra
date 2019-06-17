namespace Filters

open Brahma.OpenCL

module SepiaFilter = 
    let sepiaCommand stride = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
           let i = range.GlobalID0
           let j = range.GlobalID1
           let input = buf.[i + stride * j]
           let r = float32((input >>> 0) &&& 255u);
           let g = float32((input >>> 8) &&& 255u);
           let b = float32((input >>> 16) &&& 255u);
           let tr = (0.393f * r + 0.769f * g + 0.189f * b)
           let tg = (0.349f * r + 0.686f * g + 0.168f * b)
           let tb = (0.272f * r + 0.534f * g + 0.131f * b)
           let mutable tbC = 0.0f;
           let mutable tgC = 0.0f;
           let mutable trC = 0.0f;
           if tb > 255.0f then tbC <- 255.0f else tbC <- tb
           if tg > 255.0f then tgC <- 255.0f else tgC <- tg
           if tr > 255.0f then trC <- 255.0f else trC <- tr
           let color = ((uint32(255) <<< 24) + (uint32(tbC) <<< 16) + (uint32(tgC) <<< 8) + uint32(trC))
           dst.[i + stride * j] <- color @>