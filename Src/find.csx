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
var findEvents = DTE.Events.FindEvents;

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
    EndIntercept();
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
    EndIntercept();
    FindExecute(inputMode.Buffer);
};

messageAction = () => Vim.DisplayStatus($"FindWhat?:{inputMode.Buffer}");
messageAction.Invoke();

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

public void FindExecute(string searchKeyword)
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

    var frw = new FindResultsWindow(Vim);
    frw.Display();

    findEvents.FindDone += FindDone;
    find.Execute();
}
public void FindDone(EnvDTE.vsFindResult result, bool cancelled)
{
    findEvents.FindDone -= FindDone;
    findEvents = null;
    DTE.ActiveDocument.Activate();
}
public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    inputMode?.OnKeyInputStart(sender, e);
    messageAction?.Invoke();
}
public void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
public void EndIntercept()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    messageAction = null;
    Vim.DisplayStatus(string.Empty);
}
