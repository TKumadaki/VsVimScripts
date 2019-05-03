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

私はこのスクリプトでリフレクションを多用しています。
これは推奨されない方法です。
VsVimやVisual Studioが更新されると、このスクリプトは動かなくなるかもしれません。

## 各スクリプトファイルの説明

- [CalledCount.csx](CalledCount.ja.md)
- [FindAll.csx](FindAll.ja.md)
- [FindAllReferences.csx](FindAllReferences.ja.md)
- [FindAllReferencesResults.csx](FindAllReferencesResults.ja.md)
- FindAllReferencesWindow.csx  
  このスクリプトは単独では動作しません。
- [FindResults.csx](FindResults.ja.md)
- FindResultsWindow.csx  
  このスクリプトは単独では動作しません。
- [JDash.csx](JDash.ja.md)
- [Marks.csx](Marks.ja.md)
- [Methods.csx](Methods.ja.md)
- Options.csx  
  このスクリプトは単独では動作しません。
- [Scroll.csx](Scroll.ja.md)
- [SelectionMove.csx](SelectionMove.ja.md)
- [ShowCommandText.csx](ShowCommandText.ja.md)
- [SimpleSurround.csx](SimpleSurround.ja.md)
- [Solution.csx](Solution.ja.md)
- [Tabs.csx](Tabs.ja.md)
- [Tasklist.csx](TaskList.ja.md)
- TinyVim.csx  
  このスクリプトは単独では動作しません。
- Util.csx  
  このスクリプトは単独では動作しません。
- VsVimFindResultsWindow.csx  
  このスクリプトは単独では動作しません。
- VsVimFindResultsWindow.xaml  
  これはVsVimFindResultsWindow.csxで使用するxamlです。
- WindowUtil.csx  
  このスクリプトは単独では動作しません。
