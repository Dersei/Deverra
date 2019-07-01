namespace Filters

open Brahma.OpenCL

module SaturationFilter = 
    let saturationCommand stride ratio = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
           let i = range.GlobalID0
           let j = range.GlobalID1
           let Pr = 0.299
           let Pg = 0.587
           let Pb = 0.114
           let input = buf.[i + stride * j]
           let r = float((input >>> 0) &&& 255u);
           let g = float((input >>> 8) &&& 255u);
           let b = float((input >>> 16) &&& 255u);
           let P = sqrt(r * r * Pr + g * g * Pg + b * b *Pb)
           let tr = P + (r - P) * ratio;
           let tg = P + (g - P) * ratio;
           let tb = P + (b - P) * ratio;
           let mutable tbC = 0.0;
           let mutable tgC = 0.0;
           let mutable trC = 0.0;
           if tb > 255.0 then tbC <- 255.0 else tbC <- tb
           if tg > 255.0 then tgC <- 255.0 else tgC <- tg
           if tr > 255.0 then trC <- 255.0 else trC <- tr
           let color = ((uint32(255) <<< 24) + (uint32(tbC) <<< 16) + (uint32(tgC) <<< 8) + uint32(trC))
           dst.[i + stride * j] <- color @>