SimpleSurround.csx
===

This script mimics part of the functionality of [tpope / vim-surround] (https://github.com/tpope/vim-surround).  
Map it as follows.  

`nnoremap yss :csx SimpleSurround<Enter>`

Open the text file and enter the following.  

`Hello !!`

In normal mode, enter as follows.  

`yss`

Then enter the following:  

`t`

Then the following is displayed in the command margin.  

`<`

Continue typing as follows, then press the Enter key.  

`p`

The text file will look like the following.  

`<p>Hello !!</p>`

