module ColorExt

open System.Drawing

let packColor (color:Color)= 
    ((uint32(color.A) <<< 24) + (uint32(color.B) <<< 16) + (uint32(color.G) <<< 8) + uint32(color.R));

let unpackColor (color:uint32) =
    let r = byte((color >>> 0) &&& 255u);
    let g = byte((color >>> 8) &&& 255u);
    let b = byte((color >>> 16) &&& 255u);
    let a = byte((color >>> 24) &&& 255u);
    Color.FromArgb(int(a), int(r), int(g), int(b))

