FindAll.csx
===

This script can use "search all" function of Visual Studio on VsVim.  
When you run `csx findall` you will see something like this:  

`FindWhat:`  

Type the keyword you want to find and press Enter.  

`Options:`  

Enter search options. Press the Enter key to display the search results.  
The options that can be specified are as follows.  

- `-Backwards or -bw` Sets a value indicating whether the search is performed backwards from the current position.  
- `-FilesOfType or -ft` Sets the file extension for the files to be searched.  
- `-MatchCase or -mc` Sets a value indicating whether the search is case-sensitive.  
- `-MatchWholeWord or -mw` Sets a value indicating whether the search matches whole words only.  
- `-MatchInHiddenText or -mh` Sets a value indicating whether hidden text is included in the search.  
- `-PatternSyntax or -ps` Sets the syntax used to specify the search pattern.  
- `-SearchSubfolders or -ss` Sets a value indicating whether subfolders are included in a search operation.  
- `-Target or -t` Sets the target of the search operation, such as all open docs, files, the active document, and so forth.  

