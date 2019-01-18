#load "util.csx"
#load "tinyvim.csx"
#load "findresultswindow.csx"
#load "options.csx"

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
static Options findOptions;
FindResultsWindow frw = null;
InitilizeFindOptions(ref findOptions);

/* Input Mode */
Action messageAction = null;
TinyVimMode currentMode = null;
TinyVimMode inputMode = new TinyVimMode();
TinyVimMode optionsMode = new TinyVimMode();
currentMode = inputMode;

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
    messageAction = () =>
    {
        var lst = findOptions.DescriptionsAndValues().ToList<string>();
        lst.Insert(0, $"Options:{optionsMode.Buffer}");
        Vim.DisplayStatusLong(lst);
    };
    currentMode = optionsMode;
};

/* Options Mode */
optionsMode.OnKeyInputStart = optionsMode.OnKeyInputStartCommandPromptMode;

//Options Mode:Escape Command
var optionsEscapeCp = new CommandParser();
optionsMode.CommandParsers.Add(optionsEscapeCp);

optionsEscapeCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Escape);
};
optionsEscapeCp.CommandAction = (x) =>
{
    EndIntercept();
};

//Options Mode:Enter Command
var optionsEnterCp = new CommandParser();
optionsMode.CommandParsers.Add(optionsEnterCp);

optionsEnterCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Enter);
};
optionsEnterCp.CommandAction = (x) =>
{
    findOptions.Parse(optionsMode.Buffer);
    EndIntercept();
    FindExecute(inputMode.Buffer);
};


messageAction = () => Vim.DisplayStatusLong(new string[] { $"FindWhat:{inputMode.Buffer}" });
messageAction.Invoke();

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

public void FindExecute(string searchKeyword)
{
    var find = DTE.Find;

    find.Action = vsFindAction.vsFindActionFindAll;
    find.Backwards = (bool)findOptions["-Backwards"];
    find.FilesOfType = (string)findOptions["-FilesOfType"];
    find.FindWhat = searchKeyword;
    //find.KeepModifiedDocumentsOpen = True
    find.MatchCase = (bool)findOptions["-MatchCase"];
    find.MatchInHiddenText = (bool)findOptions["-MatchInHiddenText"];
    find.MatchWholeWord = (bool)findOptions["-MatchWholeWord"];
    find.PatternSyntax = (vsFindPatternSyntax)findOptions["-PatternSyntax"];
    //find.ReplaceWith = "hoge";
    find.ResultsLocation = vsFindResultsLocation.vsFindResults1;
    //find.SearchPath = @"c:\hoge";
    find.SearchSubfolders = (bool)findOptions["-SearchSubfolders"];
    find.Target = (vsFindTarget)findOptions["-Target"];

    frw = new FindResultsWindow(Vim);
    frw.Display();

    findEvents.FindDone += FindDone;
    find.Execute();
}
public void FindDone(EnvDTE.vsFindResult result, bool cancelled)
{
    findEvents.FindDone -= FindDone;
    findEvents = null;

    frw.Select();

    DTE.ActiveDocument.Activate();
}
public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    currentMode?.OnKeyInputStart(sender, e);
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
public void InitilizeFindOptions(ref Options options)
{
    if (options != null)
        return;

    options = new Options();
    Action<Option, string> setBool = (x, y) =>
    {
        bool val = false;
        if (!bool.TryParse(y, out val))
        {
            val = false;
        }
        x.OptionValue = val;
    };
    Func<Option, string> getDisplayBool = (x) =>
    {
        return ((bool)x.OptionValue).ToString();
    };

    Action<Option, string> setString = (x, y) =>
    {
        x.OptionValue = y;
    };
    Func<Option, string> getDisplayString = (x) =>
    {
        return x.OptionValue.ToString();
    };

    var bw = new Option();
    bw.Name = "-Backwards";
    bw.Alias = "-bw";
    bw.NameOnly = false;
    bw.Description = "[-Backwards or -bw <bool>]";
    bw.SetValue = setBool;
    bw.GetDisplayValue = getDisplayBool;
    bw.OptionValue = false;
    options.Add(bw);

    var ft = new Option();
    ft.Name = "-FilesOfType";
    ft.Alias = "-ft";
    ft.NameOnly = false;
    ft.Description = "[-FilesOfType or -ft <string>]";
    ft.SetValue = setString;
    ft.GetDisplayValue = getDisplayString;
    ft.OptionValue = "*.*";
    options.Add(ft);

    var mc = new Option();
    mc.Name = "-MatchCase";
    mc.Alias = "-mc";
    mc.NameOnly = false;
    mc.Description = "[-MatchCase or -mc <bool>]";
    mc.SetValue = setBool;
    mc.GetDisplayValue = getDisplayBool;
    mc.OptionValue = false;
    options.Add(mc);

    var mw = new Option();
    mw.Name = "-MatchWholeWord";
    mw.Alias = "-mw";
    mw.NameOnly = false;
    mw.Description = "[-MatchWholeWord or -mw <bool>]";
    mw.SetValue = setBool;
    mw.GetDisplayValue = getDisplayBool;
    mw.OptionValue = false;
    options.Add(mw);

    var mh = new Option();
    mh.Name = "-MatchInHiddenText";
    mh.Alias = "-mh";
    mh.NameOnly = false;
    mh.Description = "[-MatchInHiddenText or -mh <bool>]";
    mh.SetValue = setBool;
    mh.GetDisplayValue = getDisplayBool;
    mh.OptionValue = false;
    options.Add(mh);

    var ps = new Option();
    ps.Name = "-PatternSyntax";
    ps.Alias = "-ps";
    ps.NameOnly = false;
    ps.Description = "[-PatternSyntax or -ps <lt or rx or wc>]";
    ps.SetValue = (x, y) =>
    {
        vsFindPatternSyntax syntax = vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
        switch (y.ToLower())
        {
            case "lt":
                syntax = vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
                break;
            case "rx":
                syntax = vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
                break;
            case "wc":
                syntax = vsFindPatternSyntax.vsFindPatternSyntaxWildcards;
                break;
        }
        x.OptionValue = syntax;
    };
    ps.GetDisplayValue = (x) =>
    {
        vsFindPatternSyntax syntax = (vsFindPatternSyntax)x.OptionValue;
        switch (syntax)
        {
            case vsFindPatternSyntax.vsFindPatternSyntaxLiteral:
                return "lt";
            case vsFindPatternSyntax.vsFindPatternSyntaxRegExpr:
                return "rx";
            case vsFindPatternSyntax.vsFindPatternSyntaxWildcards:
                return "wc";
        }
        return string.Empty;
    };
    ps.OptionValue = vsFindPatternSyntax.vsFindPatternSyntaxRegExpr;
    options.Add(ps);

    var ss = new Option();
    ss.Name = "-SearchSubfolders";
    ss.Alias = "-ss";
    ss.NameOnly = false;
    ss.Description = "[-SearchSubfolders or -ss <bool>]";
    ss.SetValue = setBool;
    ss.GetDisplayValue = getDisplayBool;
    ss.OptionValue = true;
    options.Add(ss);

    var t = new Option();
    t.Name = "-Target";
    t.Alias = "-t";
    t.Description = "[-Target or -t <doc or func or sel or proj or files or open or sln>]";
    t.NameOnly = false;
    t.SetValue = (x, y) =>
    {
        vsFindTarget target = vsFindTarget.vsFindTargetSolution;
        switch (y.ToLower())
        {
            case "doc":
                target = vsFindTarget.vsFindTargetCurrentDocument;
                break;
            case "func":
                target = vsFindTarget.vsFindTargetCurrentDocumentFunction;
                break;
            case "sel":
                target = vsFindTarget.vsFindTargetCurrentDocumentSelection;
                break;
            case "proj":
                target = vsFindTarget.vsFindTargetCurrentProject;
                break;
            case "files":
                target = vsFindTarget.vsFindTargetFiles;
                break;
            case "open":
                target = vsFindTarget.vsFindTargetOpenDocuments;
                break;
            case "sln":
                target = vsFindTarget.vsFindTargetSolution;
                break;
        }
        x.OptionValue = target;
    };
    t.GetDisplayValue = (x) =>
    {
        vsFindTarget target = (vsFindTarget)x.OptionValue;
        switch (target)
        {
            case vsFindTarget.vsFindTargetCurrentDocument:
                return "doc";
            case vsFindTarget.vsFindTargetCurrentDocumentFunction:
                return "func";
            case vsFindTarget.vsFindTargetCurrentDocumentSelection:
                return "sel";
            case vsFindTarget.vsFindTargetCurrentProject:
                return "proj";
            case vsFindTarget.vsFindTargetFiles:
                return "files";
            case vsFindTarget.vsFindTargetOpenDocuments:
                return "open";
            case vsFindTarget.vsFindTargetSolution:
                return "sln";
        }
        return string.Empty;
    };
    t.OptionValue = vsFindTarget.vsFindTargetSolution;
    options.Add(t);
}
