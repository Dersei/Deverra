namespace Filters

open Brahma.OpenCL

module HueFilter = 
    let hueCommand stride ratio = <@ fun (range:_2D) (buf:array<uint32>) (dst:array<uint32>) ->
            //let toHSV r g b =
            //    let _r = float(r) / 255.0
            //    let _g = float(g) / 255.0
            //    let _b = float(b) / 255.0
            //    let mutable max = _r
            //    if max < _g then max <- _g;
            //    if max < _b then max <- _b;
            //    let mutable min = _r
            //    if min < _g then min <- _g;
            //    if min < _b then min <- _b;
            //    let diff = max - min;
            //    let l = (max + min) / 2.0;
            //    let mutable s = 0.0;
            //    let mutable h = 0.0;
            //    if(abs(diff) < 0.00001) then s <- 0.0; h <- 0.0;
            //    else 
            //        if l <= 0.5 then s <- diff / (max + min);
            //        else s <- diff / (2.0 - max - min)

            //        let r_dist = (max - _r) / diff;
            //        let g_dist = (max - _g) / diff;
            //        let b_dist = (max - _b) / diff;

            //        if _r = max then h <- b_dist - g_dist
            //        else if _g = max then h <- 2.0 + r_dist - b_dist;
            //            else h <- 4.0 + g_dist - r_dist

            //        h <- h * 60.0
            //        if h < 0.0 then h <- h + 360.0
            //    (h,s,l)
            let U = cos(ratio * 3.14 / 180.0);
            let W = sin(ratio * 3.14 / 180.0);
            let i = range.GlobalID0
            let j = range.GlobalID1
            let input = buf.[i + stride * j]
            let r = float((input >>> 0) &&& 255u);
            let g = float((input >>> 8) &&& 255u);
            let b = float((input >>> 16) &&& 255u);
            let tr = (0.299 + 0.701 * U + 0.168 * W) * r + (0.587 - 0.587 * U + 0.330 * W) * g + (0.114 - 0.114 * U - 0.497 * W) * b
            let tg = (0.299 - 0.299 * U - 0.328 * W) * r + (0.587 + 0.413 * U + 0.035 * W) * g + (0.114 - 0.114 * U + 0.292 * W) * b
            let tb = (0.299 - 0.3 * U + 10.25 * W) * r + (0.587 - 0.588 * U - 10.05 * W) * g + (0.114 + 0.886 * U - 0.203 * W) * b
            let mutable tbC = 0.0;
            let mutable tgC = 0.0;
            let mutable trC = 0.0;
            if tb > 255.0 then tbC <- 255.0 else tbC <- tb
            if tb < 0.0 then tbC <- 0.0 else tbC <- tbC
            if tg > 255.0 then tgC <- 255.0 else tgC <- tg
            if tg < 0.0 then tgC <- 0.0 else tgC <- tgC
            if tr > 255.0 then trC <- 255.0 else trC <- tr
            if tr < 0.0 then trC <- 0.0 else trC <- trC
            let color = ((uint32(255) <<< 24) + (uint32(tbC) <<< 16) + (uint32(tgC) <<< 8) + uint32(trC))
            dst.[i + stride * j] <- color @>