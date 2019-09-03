MethodTextObject.csx
===

Treat methods as text objects.  
Set as follows.  

```
nnoremap vam          :csx MethodTextObject vam<CR>
nnoremap dam          :csx MethodTextObject dam<CR>
nnoremap yam          :csx MethodTextObject yam<CR>

nnoremap vim          :csx MethodTextObject vim<CR>
nnoremap dim          :csx MethodTextObject dim<CR>
nnoremap yim          :csx MethodTextObject yim<CR>
```

Change the following part of the script according to your environment.  

```
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.CSharp.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\Microsoft\Microsoft.NET.Build.Extensions\net461\lib\System.Threading.Tasks.dll"
```

## Supported commands

- dam : Delete the entire method.
- dim : Delete the inside of the method.
- yam : Copy the entire method.
- yim : Copy the inside of the method.
- vam : Select the entire method.
- vim : Select the inside of the method.

## Supported languages

- C#
