EasyMotion.csx
===

[EasyMotion](https://github.com/jaredpar/EasyMotion)をC#スクリプトで移植したものです。
以下のように設定します。  

```

nnoremap s            :csx EasyMotion<CR>

```
スクリプトを以下のように変更すると 2文字での検索が可能です。
1文字で検索したい場合は、検索文字から続けてEnterキーを押してください。


```
private int defaultSearchKeywordLength = 2;

```


