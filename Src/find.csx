#load "util.csx"
#load "tinyvim.csx"
#load "findresultswindow.csx"

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

IVimBuffer vimBuffer;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}
var DTE = Util.GetDTE2();


/* Input Mode */
Action messageAction = null;
TinyVimMode inputMode = new TinyVimMode();
inputMode.OnKeyInputStart = inputMode.OnKeyInputStartCommandPromptMode;

//Input Mode:Escape Command
var inputEscapeCp = new CommandParser();
inputMode.CommandParsers.Add(inputEscapeCp);

inputEscapeCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Escape);
};
inputEscapeCp.CommandAction = (x) =>
{
    InterceptEnd();
};

//Input Mode:Enter Command
var inputEnterCp = new CommandParser();
inputMode.CommandParsers.Add(inputEnterCp);

inputEnterCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Enter);
};
inputEnterCp.CommandAction = (x) =>
{
    FindStart(inputMode.Buffer);
    InterceptEnd();
    var frw = new FindResultsWindow(Vim);
    frw.Display();
};

messageAction = () => Vim.DisplayStatus($"keyword?:{inputMode.Buffer}");
messageAction.Invoke();

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

public void FindStart(string searchKeyword)
{
    var find = DTE.Find;

    find.Action = vsFindAction.vsFindActionFindAll;
    find.Backwards = false;
    find.FilesOfType = "*.*";
    find.FindWhat = searchKeyword;
    //find.KeepModifiedDocumentsOpen = True
    find.MatchCase = false;
    find.MatchInHiddenText = false;
    find.MatchWholeWord = false;
    find.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
    //find.ReplaceWith = "NEW THING";
    find.ResultsLocation = vsFindResultsLocation.vsFindResults1;
    //find.SearchPath = "c:    emp";
    find.SearchSubfolders = true;
    find.Target = vsFindTarget.vsFindTargetSolution;

    //////Add Event
    //var m_findEvents = DTE.Events.FindEvents;
    //var fe = new FindEvent(DTE);
    //m_findEvents.FindDone += new EnvDTE._dispFindEvents_FindDoneEventHandler(fe.FindEvents_FindDone);
    ////Vim.DisplayStatus("FindDone");

    find.Execute();
}
//public class FindEvent
//{
//    private DTE2 dte;
//    public FindEvent(DTE2 dte)
//    {
//        this.dte = dte;
//    }
//    public void FindEvents_FindDone(EnvDTE.vsFindResult Result, bool Cancelled)
//    {
//        //Vim.DisplayStatus("Events");
//        ////DTE.Events.FindEvents.FindDone -= FindEvents_FindDone;
//        //var frw = new FindResultsWindow(Vim);
//        //frw.Display();
//        dte.ActiveDocument.Activate();
//    }
//}
public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    inputMode?.OnKeyInputStart(sender, e);
    messageAction?.Invoke();
}
public void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}
public void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    messageAction = null;
    Vim.DisplayStatus(string.Empty);
}
