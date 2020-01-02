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

I use a lot of reflection in scripts.
This is a unrecommended method.
If VsVim or Visual Studio update, scripts may not work.

## Description of each script file

- [CalledCount.csx](CalledCount.md)
- [EasyMotion.csx](EasyMotion.md)
- [FindAll.csx](FindAll.md)
- [FindAllReferences.csx](FindAllReferences.md)
- [FindAllReferencesResults.csx](FindAllReferencesResults.md)
- FindAllReferencesWindow.csx  
  This script does not work on its own.
- [FindResults.csx](FindResults.md)
- FindResultsWindow.csx  
  This script does not work on its own.
- [IndentObject.csx](IndentObject.md)
- [JDash.csx](JDash.md)
- [Marks.csx](Marks.md)
- [MethodTextObject.csx](MethodTextObject.md)
- [Methods.csx](Methods.md)
- Options.csx  
  This script does not work on its own.
- [Scroll.csx](Scroll.md)
- [SelectionMove.csx](SelectionMove.md)
- [ShowCommandText.csx](ShowCommandText.md)
- [SimpleSurround.csx](SimpleSurround.md)
- [Solution.csx](Solution.md)
- [Tabs.csx](Tabs.md)
- [Tasklist.csx](TaskList.md)
- TinyVim.csx  
  This script does not work on its own.
- Util.csx  
  This script does not work on its own.
- VsVimFindResultsWindow.csx  
  This script does not work on its own.
- VsVimFindResultsWindow.xaml  
  This is xaml for use with VsVimFindResultsWindow.csx.
- WindowUtil.csx  
  This script does not work on its own.
