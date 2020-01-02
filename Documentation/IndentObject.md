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

`count` is not supported.  
The mapping is different from the original.  
Therefore, it not be versatile when compared to the original.  
