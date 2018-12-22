FindAll.csx
===

このスクリプトはVsVim上でVisual Studioの"全てを検索する"の機能を使用できます。  
`csx findall`を実行すると以下のように表示されます。  

`FindWhat:`  

検索するキーワードを入力し、Enterキーを押します。  

`Options:`  

検索オプションを入力します。Enterキーを押すと、検索結果が表示されます。  
指定できるオプションは以下の通りです。  

- `-Backwards or -bw` 現在位置から逆方向に検索を実行するかどうかを設定します。  
- `-FilesOfType or -ft` 検索するファイルのファイル拡張子を設定します。  
- `-MatchCase or -mc` 検索で大文字と小文字が区別されるかどうかを示す値を設定します。  
- `-MatchWholeWord or -mw` 検索が単語全体にのみ一致するかどうかを示す値を設定します。  
- `-MatchInHiddenText or -mh` 非表示のテキストが検索に含まれるかどうかを示す値を設定します。  
- `-PatternSyntax or -ps` 検索パターンを指定するために使用される構文を設定します。  
- `-SearchSubfolders or -ss` サブフォルダーが検索操作に含まれるかどうかを示す値を設定します。  
- `-Target or -t` 開いているすべてのドキュメント、ファイル、アクティブなドキュメントなどの検索操作のターゲットを設定します。  

