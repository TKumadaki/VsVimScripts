IndentObject.csx
===

[vim-indent-object](https://github.com/michaeljsmith/vim-indent-object)をVsVimに移植したスクリプトです。  
以下のように設定します。  

```
nnoremap dai          :csx IndentObject dai<CR>
nnoremap daI          :csx IndentObject daI<CR>
nnoremap dii          :csx IndentObject dii<CR>

nnoremap yai          :csx IndentObject yai<CR>
nnoremap yaI          :csx IndentObject yaI<CR>
nnoremap yii          :csx IndentObject yii<CR>

nnoremap cai          :csx IndentObject cai<CR>
nnoremap caI          :csx IndentObject caI<CR>
nnoremap cii          :csx IndentObject cii<CR>

vnoremap ai           :csx IndentObject vai<CR>
vnoremap aI           :csx IndentObject vaI<CR>
vnoremap ii           :csx IndentObject vii<CR>

```

`count`はサポートされません。  
マッピングがオリジナルと異なります。  
そのため、オリジナルと比較すると汎用性はありません。
