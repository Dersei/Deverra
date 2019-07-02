# Deverra

This simple project is written in F# and C# using Brahma.Fsharp and OpenCL. Its goal is simple image processing with predefined filters.

First project "Deverra"- written in F# - is console app which takes care of compiling and running OpenCL kernels. It can be used directly with adequate arguments:

```powershell
.\Deverra.exe  [filters]
.\Deverra.exe "Resources/warsaw2.jpg" Contrast=100 Sepia
```

-   path - relative or absolute path to image
    
-   filters - one or more filters (when none is given, the program uses default sepia)
    
    -   filter with value - *name=value*
        
    -   filter without value - *name*
        

The second project "Deverra.GUI" is a WPF app, written in C# and it allows to use all filters with GUI.