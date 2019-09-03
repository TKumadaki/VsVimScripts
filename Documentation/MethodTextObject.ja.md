MethodTextObject.csx
===

メソッドをテキストオブジェクトとしてあつかえます。  
以下のように設定します。  

```
nnoremap vam          :csx MethodTextObject vam<CR>
nnoremap dam          :csx MethodTextObject dam<CR>
nnoremap yam          :csx MethodTextObject yam<CR>

nnoremap vim          :csx MethodTextObject vim<CR>
nnoremap dim          :csx MethodTextObject dim<CR>
nnoremap yim          :csx MethodTextObject yim<CR>
```

スクリプトの以下の部分を環境に合わせて変更してください。

```
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.CSharp.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\Microsoft\Microsoft.NET.Build.Extensions\net461\lib\System.Threading.Tasks.dll"
```

## サポートするコマンド

- dam : メソッド全体を削除します。
- dim : メソッドの処理を削除します。
- yam : メソッド全体をコピーします。
- yim : メソッドの処理をコピーします。
- vam : メソッド全体を選択します。
- vim : メソッドの処理を選択します。

## サポートする言語
C#
