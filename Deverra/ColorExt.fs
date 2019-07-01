module ColorExt

open System.Windows.Media

let packColor (color:Color)= 
    ((uint32(color.A) <<< 24) + (uint32(color.B) <<< 16) + (uint32(color.G) <<< 8) + uint32(color.R));

let unpackColor (color:uint32) =
    let r = byte((color >>> 0) &&& 255u);
    let g = byte((color >>> 8) &&& 255u);
    let b = byte((color >>> 16) &&& 255u);
    let a = byte((color >>> 24) &&& 255u);
    Color.FromArgb(a, r, g, b)

let packBytes (bytes : array<byte>) = 
    let result = Array.create (bytes.Length/4) 0u
    for i in 0..result.Length do
        result.[i] <- ((uint32(bytes.[4 * i + 3]) <<< 24) + (uint32(bytes.[4 * i]) <<< 16) + (uint32(bytes.[4 * i + 1]) <<< 8) + uint32(bytes.[4 * i + 2]));
    result

let private rotateGridBy90DegreesToTheRight grid =
    let height, width = Array2D.length1 grid, Array2D.length2 grid
    Array2D.init width height (fun row column -> Array2D.get grid (height - column - 1) row)

let private toArray (arr: 'T [,]) = arr |> Seq.cast<'T> |> Seq.toArray

let rotateArray (stride : int)(colors : array<uint32>) = 
    colors |> Array.chunkBySize stride |> array2D |> rotateGridBy90DegreesToTheRight |> toArray

let createByteArray (colors : array<uint32>) = 
    let result = Array.create (colors.Length * 4) 0uy
    for i in 0..4..result.Length - 1 do
        result.[i] <-  byte((colors.[i/4] >>> 16) &&& 255u); 
        result.[i + 1] <- byte((colors.[i/4] >>> 8) &&& 255u);  
        result.[i + 2] <- byte((colors.[i/4] >>> 0) &&& 255u); 
        result.[i + 3] <- byte((colors.[i/4] >>> 24) &&& 255u);
    result