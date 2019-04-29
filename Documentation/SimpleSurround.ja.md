SimpleSurround.csx
===

このスクリプトは[tpope/vim-surround](https://github.com/tpope/vim-surround)の機能の一部分を真似したものです。  
以下のようにマッピングします。  

`nnoremap yss :csx SimpleSurround<CR>`

テキストファイルを開き、以下のように入力します。  

`Hello !!`

ノーマルモードで、以下のように入力します。  

`yss`

次に以下のように入力します。  

`t`

するとコマンドマージンに以下が入力されます。  

`<`

続けて以下のように入力したあと、Enterキーを押します。  

`p`

するとテキストファイルが以下のようになります。  

`<p>Hello !!</p>`

