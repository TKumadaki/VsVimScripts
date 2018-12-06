About the script files
===

## Known Issues

Solution.csx and FindResultsWindow.csx are manipulating the tool window of Visual Studio.  
However, it becomes impossible to operate if it becomes the following state.  

- The editor that executed the csx command is no longer active.  
- The editor that executed the csx command is closed.  

The reason for these is that the script gets the key event of the editor that executed the csx command and manipulates the tool window.  

When using SimpleSurround.csx etc., the user needs to enter a value in the command margin.  
The command margin at this time is a poor function that the cursor is not displayed and the arrow keys can not be used.
This is because methods that manipulate command margins are not exposed.  

## Description of each script file

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
- [Solution.csx](Solution.md)
- Tasklist.csx
- TinyVim.csx
- Util.csx
