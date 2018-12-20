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
- [FindAllReferences.csx](FindAllReferences.ja.md)
- [FindAllReferencesResults.csx](FindAllReferencesResults.ja.md)
- FindAllReferencesWindow.csx  
  このスクリプトは単独では動作しません。
- [FindResults.csx](FindResults.ja.md)
- FindResultsWindow.csx  
  このスクリプトは単独では動作しません。
- [Methods.csx](Methods.ja.md)
- Options.csx  
  このスクリプトは単独では動作しません。
- [Scroll.csx](Scroll.ja.md)
- [SelectionMove.csx](SelectionMove.ja.md)
- SimpleSurround.csx
- [Solution.csx](Solution.ja.md)
- [Tasklist.csx](TaskList.ja.md)
- TinyVim.csx  
  このスクリプトは単独では動作しません。
- Util.csx  
  このスクリプトは単独では動作しません。
