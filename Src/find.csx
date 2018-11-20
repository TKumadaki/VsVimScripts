#load "util.csx"
#load "tinyvim.csx"
//#load "findresults.csx"

using EnvDTE;
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
    Find(inputMode.Buffer);
    InterceptEnd();
    //TODO:検索窓を開く
};

messageAction = () => Vim.DisplayStatus($"keyword?:{inputMode.Buffer}");
messageAction.Invoke();

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

public void Find(string searchKeyword)
{
    var objFind = DTE.Find;
    objFind.Action = vsFindAction.vsFindActionFindAll;
    objFind.Backwards = false;
    objFind.FilesOfType = "*.*";
    objFind.FindWhat = searchKeyword;
    //objFind.KeepModifiedDocumentsOpen = True
    objFind.MatchCase = false;
    objFind.MatchInHiddenText = false;
    objFind.MatchWholeWord = false;
    objFind.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
    //objFind.ReplaceWith = "NEW THING";
    objFind.ResultsLocation = vsFindResultsLocation.vsFindResults1;
    //objFind.SearchPath = "c:    emp";
    objFind.SearchSubfolders = true;
    objFind.Target = vsFindTarget.vsFindTargetSolution;
    objFind.Execute();
}
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
