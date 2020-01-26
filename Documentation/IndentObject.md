IndentObject.csx
===

This script is for using functions like [vim-indent-object](https://github.com/michaeljsmith/vim-indent-object) in VsVim.  
Set as follows.  

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

When using count, perform the following mapping.

```
vnoremap 2ai           :csx IndentObject vai2<CR>
vnoremap 2aI           :csx IndentObject vaI2<CR>
vnoremap 2ii           :csx IndentObject vii2<CR>

vnoremap 3ai           :csx IndentObject vai3<CR>
vnoremap 3aI           :csx IndentObject vaI3<CR>
vnoremap 3ii           :csx IndentObject vii3<CR>

```
