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
countを使用する場合は、以下のようなマッピングを行ってください。

```
vnoremap 2ai           :csx IndentObject vai2<CR>
vnoremap 2aI           :csx IndentObject vaI2<CR>
vnoremap 2ii           :csx IndentObject vii2<CR>

vnoremap 3ai           :csx IndentObject vai3<CR>
vnoremap 3aI           :csx IndentObject vaI3<CR>
vnoremap 3ii           :csx IndentObject vii3<CR>

```

