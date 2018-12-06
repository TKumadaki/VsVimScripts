スクリプトファイルについて
===

## 既知の問題

Solution.csxやFindResultsWindow.csxはVisual Studioのツールウィンドウを操作しています。  
しかし以下の状態になると操作することができなくなります。

- csxコマンドを実行したエディタがアクティブではなくなった。  
- csxコマンドを実行したエディタが閉じられた。

理由は、csxコマンドを実行したエディタのキーイベントを取得してツールウィンドウを操作しているからです。  

SimpleSurround.csxなどを使用する時、ユーザーはコマンドマージンに値を入力する必要があります。  
この時のコマンドマージンはカーソルが表示されず、矢印キーも使えない貧弱な機能になっています。  
これは、コマンドマージンを操作するメソッドが公開されていないためです。  

## 各スクリプトファイルの説明

- CalledCount.csx
- FindAll.csx
- FindAllReferences.csx
- FindAllReferencesResults.csx
- FindAllReferencesWindow.csx
- FindResults.csx
- FindResultsWindow.csx
- Methods.csx
- Options.csx
- Scroll.csx
- SelectionMove.csx
- SimpleSurround.csx
- [Solution.csx](Solution.ja.md)
- Tasklist.csx
- TinyVim.csx
- Util.csx
