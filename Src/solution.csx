#load "util.csx"
#load "tinyvim.csx"

using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

const string PhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";   //GUID_ItemType_PhysicalFile
const string SolutionItem = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";   //VsProjectItemKindSolutionItem
const string VirtualFolder = "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}";  //GUID_ItemType_VirtualFolder

var DTE = Util.GetDTE2();

UIHierarchy solutionExplorer = DTE.ToolWindows.SolutionExplorer;

string lastSearchKeyword = string.Empty;
bool lastSearchForward = true;
bool currentSearchForward = true;
bool autoHides = true;

Action messageAction = null;
TinyVimMode currentVimMode = null;
TinyVimMode normalMode = new TinyVimMode();
TinyVimMode searchMode = new TinyVimMode();
TinyVimMode incrementalSearchMode = new TinyVimMode();
TinyVimMode deleteMode = new TinyVimMode();
TinyVimMode exitMode = new TinyVimMode();

/* Normal Mode */
normalMode.OnKeyInputStart = normalMode.OnKeyInputStartNormalMode;

//Normal Mode:a Command
var aCp = new CommandParser();
normalMode.CommandParsers.Add(aCp);

aCp.CommandEquals = (x) =>
{
    return (x.Char == 'a');
};
aCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    EndIntercept();
    DTE.ExecuteCommand("View.SolutionExplorer");
    DTE.ExecuteCommand("Project.AddNewItem");
};

//Normal Mode:c Command
var cCp = new CommandParser();
normalMode.CommandParsers.Add(cCp);

cCp.CommandEquals = (x) =>
{
    return (x.Char == 'c');
};
cCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem item = GetSelectedItem();
    if (0 < item.UIHierarchyItems.CountEx())
    {
        CollapseOrExpand(item, expand: false);
        item.UIHierarchyItems.Expanded = false;
    }
};

//Normal Mode:C Command
var CCp = new CommandParser();
normalMode.CommandParsers.Add(CCp);

CCp.CommandEquals = (x) =>
{
    return (x.Char == 'C');
};
CCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    if (solutionExplorer.UIHierarchyItems.CountEx() == 0)
    {
        return;
    }
    UIHierarchyItem topItem = solutionExplorer.UIHierarchyItems.Item(1);
    topItem.Select(vsUISelectionType.vsUISelectionTypeSelect);

    CollapseOrExpand(topItem, expand: false);
};

//Normal Mode:d Command
var dCp = new CommandParser();
normalMode.CommandParsers.Add(dCp);

dCp.CommandEquals = (x) =>
{
    return (x.Char == 'd');
};
dCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    ProjectItem pi = GetSelectedProjectItem();
    SwitchDeleteMode(pi?.Name);
};

//Normal Mode:e Command
var eCp = new CommandParser();
normalMode.CommandParsers.Add(eCp);

eCp.CommandEquals = (x) =>
{
    return (x.Char == 'e');
};
eCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem item = GetSelectedItem();
    if (0 < item.UIHierarchyItems.CountEx())
    {
        CollapseOrExpand(item, expand: true);
        item.UIHierarchyItems.Expanded = true;
    }
};

//Normal Mode:E Command
var ECp = new CommandParser();
normalMode.CommandParsers.Add(ECp);

ECp.CommandEquals = (x) =>
{
    return (x.Char == 'E');
};
ECp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    if (solutionExplorer.UIHierarchyItems.CountEx() == 0)
    {
        return;
    }
    UIHierarchyItem topItem = solutionExplorer.UIHierarchyItems.Item(1);
    CollapseOrExpand(topItem, expand: true);
};

//Normal Mode:f Command
var fCp = new CommandParser();
normalMode.CommandParsers.Add(fCp);

fCp.CommandEquals = (x) =>
{
    return (x.Char == 'f');
};
fCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    SwitchSearchMode(forward: true);
};

//Normal Mode:F Command
var FCp = new CommandParser();
normalMode.CommandParsers.Add(FCp);

FCp.CommandEquals = (x) =>
{
    return (x.Char == 'F');
};
FCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    SwitchSearchMode(forward: false);
};

//Normal Mode:gg Command
var ggCp = new CommandParser();
normalMode.CommandParsers.Add(ggCp);

ggCp.CommandEquals = (x) =>
{
    return (normalMode.Buffer == "gg");
};
ggCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem topItem = solutionExplorer.UIHierarchyItems.Item(1);
    topItem.Select(vsUISelectionType.vsUISelectionTypeSelect);
};

//Normal Mode:G Command
var GCp = new CommandParser();
normalMode.CommandParsers.Add(GCp);

GCp.CommandEquals = (x) =>
{
    return (x.Char == 'G');
};
GCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem item = GetSelectedItem();
    int count = item.UIHierarchyItems.CountEx();
    if (count == 0)
    {
        UIHierarchyItem parentItem = GetSelectedItem()?.Collection.Parent as UIHierarchyItem;
        if (parentItem == null)
            return;
        count = parentItem.UIHierarchyItems.CountEx();
        if (0 < count)
        {
            parentItem.UIHierarchyItems.Expanded = true;
            parentItem.UIHierarchyItems.Item(count).Select(vsUISelectionType.vsUISelectionTypeSelect);
        }
    }
    else
    {
        item.UIHierarchyItems.Expanded = true;
        item.UIHierarchyItems.Item(count).Select(vsUISelectionType.vsUISelectionTypeSelect);
    }
};

//Normal Mode:h Command
var hCp = new CommandParser();
normalMode.CommandParsers.Add(hCp);

hCp.Regex = new Regex("^[0-9]*h$");
hCp.CommandEquals = (x) =>
{
    return hCp.Regex.IsMatch(normalMode.Buffer);
};
hCp.CommandAction = (x) =>
{
    int count = RegexUtil.GetCount(normalMode.Buffer);
    normalMode.Buffer = string.Empty;
    for (int i = 0; i < count; i++)
    {
        UIHierarchyItem item = GetSelectedItem();
        UIHierarchyItem parentItem = item?.Collection.Parent as UIHierarchyItem;

        if (parentItem == null)
            return;

        parentItem.Select(vsUISelectionType.vsUISelectionTypeSelect);
    }
};

//Normal Mode:j Command
var jCp = new CommandParser();
normalMode.CommandParsers.Add(jCp);

jCp.Regex = new Regex("^[0-9]*j$");
jCp.CommandEquals = (x) =>
{
    return jCp.Regex.IsMatch(normalMode.Buffer);
};
jCp.CommandAction = (x) =>
{
    int count = RegexUtil.GetCount(normalMode.Buffer);
    normalMode.Buffer = string.Empty;
    for (int i = 0; i < count; i++)
    {
        solutionExplorer.SelectDown(vsUISelectionType.vsUISelectionTypeSelect, 1);
    }
};

//Normal Mode:k Command
var kCp = new CommandParser();
normalMode.CommandParsers.Add(kCp);

kCp.Regex = new Regex("^[0-9]*k$");
kCp.CommandEquals = (x) =>
{
    return kCp.Regex.IsMatch(normalMode.Buffer);
};
kCp.CommandAction = (x) =>
{
    int count = RegexUtil.GetCount(normalMode.Buffer);
    normalMode.Buffer = string.Empty;
    for (int i = 0; i < count; i++)
    {
        solutionExplorer.SelectUp(vsUISelectionType.vsUISelectionTypeSelect, 1);
    }
};

//Normal Mode:l Command
var lCp = new CommandParser();
normalMode.CommandParsers.Add(lCp);

lCp.CommandEquals = (x) =>
{
    return (x.Char == 'l');
};
lCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem item = GetSelectedItem();
    if (0 < item.UIHierarchyItems.CountEx())
    {
        item.UIHierarchyItems.Expanded = true;
        item.UIHierarchyItems.Item(1).Select(vsUISelectionType.vsUISelectionTypeSelect);
    }
};

//Normal Mode:n Command
var nCp = new CommandParser();
normalMode.CommandParsers.Add(nCp);

nCp.CommandEquals = (x) =>
{
    return (x.Char == 'n');
};
nCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    Search(lastSearchKeyword, solutionExplorer, startItem: GetSelectedItem(), forward: lastSearchForward, switchNormalMode: false);
};

//Normal Mode:N Command
var NCp = new CommandParser();
normalMode.CommandParsers.Add(NCp);

NCp.CommandEquals = (x) =>
{
    return (x.Char == 'N');
};
NCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    Search(lastSearchKeyword, solutionExplorer, startItem: GetSelectedItem(), forward: !lastSearchForward, switchNormalMode: false);
};

//Normal Mode:r Command
var rCp = new CommandParser();
normalMode.CommandParsers.Add(rCp);

rCp.CommandEquals = (x) =>
{
    return (x.Char == 'r');
};
rCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    EndIntercept();
    DTE.ExecuteCommand("View.SolutionExplorer");
    DTE.ExecuteCommand("File.Rename");
};

//Normal Mode:/ Command
var slashCp = new CommandParser();
normalMode.CommandParsers.Add(slashCp);

slashCp.CommandEquals = (x) =>
{
    return (x.Char == '/');
};
slashCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    SwitchIncrementalSearchMode(forward: true);
};

//Normal Mode:? Command
var questionCp = new CommandParser();
normalMode.CommandParsers.Add(questionCp);

questionCp.CommandEquals = (x) =>
{
    return (x.Char == '?');
};
questionCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    SwitchIncrementalSearchMode(forward: false);
};

//Normal Mode:Escape Command
var escapeCp = new CommandParser();
normalMode.CommandParsers.Add(escapeCp);

escapeCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Escape);
};
escapeCp.CommandAction = (x) =>
{
    EndIntercept();
    //SwitchExitMode();
};

//Normal Mode:Enter Command
var enterCp = new CommandParser();
normalMode.CommandParsers.Add(enterCp);

enterCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Enter);
};
enterCp.CommandAction = (x) =>
{
    normalMode.Buffer = string.Empty;
    UIHierarchyItem item = GetSelectedItem();
    ProjectItem pi = GetSelectedProjectItem();
    if (pi != null && (pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
    {
        EndIntercept();
    }
    solutionExplorer.DoDefaultAction();
};

//Normal Mode:Need More
var needCp = new CommandParser();
normalMode.CommandParsers.Add(needCp);

needCp.Regex = new Regex("^[0-9]*$|^g$");
needCp.CommandEquals = (x) =>
{
    return needCp.Regex.IsMatch(normalMode.Buffer);
};
needCp.CommandAction = (x) =>
{
    //no action
};

/* Search Mode */
searchMode.OnKeyInputStart = searchMode.OnKeyInputStartCommandPromptMode;

//Search Mode:Escape Command
var searchEscapeCp = new CommandParser();
searchMode.CommandParsers.Add(searchEscapeCp);

searchEscapeCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Escape);
};
searchEscapeCp.CommandAction = (x) =>
{
    SwitchNormalMode(swichMessage: true);
};

//Search Mode:Enter Command
var searchEnterCp = new CommandParser();
searchMode.CommandParsers.Add(searchEnterCp);

searchEnterCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Enter);
};
searchEnterCp.CommandAction = (x) =>
{
    Search(searchMode.Buffer, solutionExplorer, startItem: GetSelectedItem(), forward: currentSearchForward, switchNormalMode: true);
};

/* Incremental Search Mode */
incrementalSearchMode.OnKeyInputStart = incrementalSearchMode.OnKeyInputStartCommandPromptMode;

//Incremental Search Mode:Escape Command
var incrementalSearchEscapeCp = new CommandParser();
incrementalSearchMode.CommandParsers.Add(incrementalSearchEscapeCp);

incrementalSearchEscapeCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Escape);
};
incrementalSearchEscapeCp.CommandAction = (x) =>
{
    SwitchNormalMode(swichMessage: true);
};

//Incremental Search Mode:Enter Command
var incrementalSearchEnterCp = new CommandParser();
incrementalSearchMode.CommandParsers.Add(incrementalSearchEnterCp);

incrementalSearchEnterCp.CommandEquals = (x) =>
{
    return (x.Key == VimKey.Enter);
};
incrementalSearchEnterCp.CommandAction = (x) =>
{
    Search(incrementalSearchMode.Buffer, solutionExplorer, startItem: GetSelectedItem(), forward: currentSearchForward, switchNormalMode: true);
};

//Incremental Search Mode:Incremental Search Command
var incrementalSearchCp = new CommandParser();
incrementalSearchMode.CommandParsers.Add(incrementalSearchCp);

incrementalSearchCp.CommandEquals = (x) =>
{
    if (x.RawChar.IsSome() && !char.IsControl(x.Char))
    {
        return true;
    }
    if (x.Key == VimKey.Back)
    {
        return true;
    }
    return false;
};
incrementalSearchCp.CommandAction = (x) =>
{
    Search(incrementalSearchMode.Buffer, solutionExplorer, startItem: GetSelectedItem(), forward: currentSearchForward, switchNormalMode: false);
};

/* Delete Mode */
deleteMode.OnKeyInputStart = deleteMode.OnKeyInputStartNormalMode;

//Delete Mode:yes
var deleteYesCp = new CommandParser();
deleteMode.CommandParsers.Add(deleteYesCp);

deleteYesCp.CommandEquals = (x) =>
{
    return (char.ToLower(x.Char) == 'y');
};
deleteYesCp.CommandAction = (x) =>
{
    ProjectItem pi = GetSelectedProjectItem();
    //pi?.Remove(); //Removes the project item from the collection.
    pi?.Delete(); //Removes the item from its project and its storage.
    SwitchNormalMode(swichMessage: true);
};

//Delete Mode:no
var deleteNoCp = new CommandParser();
deleteMode.CommandParsers.Add(deleteNoCp);

deleteNoCp.CommandEquals = (x) =>
{
    return (char.ToLower(x.Char) == 'n');
};
deleteNoCp.CommandAction = (x) =>
{
    SwitchNormalMode(swichMessage: true);
};

/* Exit Mode */
exitMode.OnKeyInputStart = exitMode.OnKeyInputStartNormalMode;

//Exit Mode:yes
var exitYesCp = new CommandParser();
exitMode.CommandParsers.Add(exitYesCp);

exitYesCp.CommandEquals = (x) =>
{
    return (char.ToLower(x.Char) == 'y');
};
exitYesCp.CommandAction = (x) =>
{
    EndIntercept();
};

//Exit Mode:no
var exitNoCp = new CommandParser();
exitMode.CommandParsers.Add(exitNoCp);

exitNoCp.CommandEquals = (x) =>
{
    return (char.ToLower(x.Char) == 'n');
};
exitNoCp.CommandAction = (x) =>
{
    SwitchNormalMode(swichMessage: true);
};

UIHierarchyItem item = GetSelectedItem();
item.Select(vsUISelectionType.vsUISelectionTypeSelect);

SwitchNormalMode(true);
VimBuffer.KeyInputStart += OnKeyInputStart;
VimBuffer.Closed += OnBufferClosed;

autoHides = solutionExplorer.Parent.AutoHides;

solutionExplorer.Parent.AutoHides = false;
solutionExplorer.Parent.Activate();
DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    currentVimMode?.OnKeyInputStart(sender, e);
    messageAction?.Invoke();
}
public void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
public void EndIntercept()
{
    VimBuffer.KeyInputStart -= OnKeyInputStart;
    VimBuffer.Closed -= OnBufferClosed;
    currentVimMode = null;
    messageAction = null;
    VimBuffer.DisplayStatus(string.Empty);
    solutionExplorer.Parent.AutoHides = autoHides;
}
private void SwitchExitMode()
{
    messageAction = () => VimBuffer.DisplayStatus("Do you want to exit? (y/n)");
    messageAction.Invoke();
    currentVimMode = exitMode;
}
private void SwitchDeleteMode(string fileName)
{
    messageAction = () => VimBuffer.DisplayStatus($"delete {fileName}? (y/n)");
    messageAction.Invoke();
    currentVimMode = deleteMode;
}
private void SwitchNormalMode(bool swichMessage)
{
    if (swichMessage)
    {
        messageAction = () => VimBuffer.DisplayStatus("-- SOLUTION EXPLORER OPERATION MODE --");
    }
    messageAction?.Invoke();
    currentVimMode = normalMode;
}
private void SwitchSearchMode(bool forward)
{
    searchMode.Buffer = string.Empty;
    messageAction = () => VimBuffer.DisplayStatus($":{(forward ? "/" : "?")}{searchMode.Buffer}");
    messageAction.Invoke();

    currentSearchForward = forward;
    currentVimMode = searchMode;
}
private void SwitchIncrementalSearchMode(bool forward)
{
    incrementalSearchMode.Buffer = string.Empty;
    messageAction = () => VimBuffer.DisplayStatus($"{(forward ? "/" : "?")}{incrementalSearchMode.Buffer}");
    messageAction.Invoke();

    currentSearchForward = forward;
    currentVimMode = incrementalSearchMode;
}
private void Search(string keyword, UIHierarchy solutionExplorer, object startItem, bool forward, bool switchNormalMode)
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        messageAction = () => VimBuffer.DisplayStatus("nothing keyword.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    if (solutionExplorer.UIHierarchyItems.CountEx() == 0)
    {
        messageAction = () => VimBuffer.DisplayStatus("solution item 0.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    var searchList = new List<SearchItem>();
    UIHierarchyItem rootNode = solutionExplorer.UIHierarchyItems.Item(1);

    GetSearchList(ref searchList, rootNode);

    if (searchList.Count == 0)
    {
        messageAction = () => VimBuffer.DisplayStatus("search item 0.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    keyword = keyword.ToLower();

    if (!forward)
        searchList.Reverse();

    int startIndex = -1;
    if (startItem != null)
    {
        startIndex = searchList.FindIndex((x) => object.ReferenceEquals(x.Item, startItem));
    }
    startIndex++;

    int index = -1;
    for (int i = startIndex; i < searchList.Count(); i++)
    {
        if (0 <= searchList[i].Name.IndexOf(keyword))
        {
            index = i;
            break;
        }
    }
    if (index < 0)
    {
        for (int i = 0; i < startIndex; i++)
        {
            if (0 <= searchList[i].Name.IndexOf(keyword))
            {
                index = i;
                break;
            }
        }
    }

    if (index < 0)
    {
        messageAction = () => VimBuffer.DisplayStatus("not found.");
        switchNormalMode = true;
    }
    else
    {
        searchList[index].Item.Select(vsUISelectionType.vsUISelectionTypeSelect);
        lastSearchKeyword = keyword;
        lastSearchForward = currentSearchForward;
    }

    if (!switchNormalMode)
        return;

    SwitchNormalMode(swichMessage: 0 <= index);
}
private void CollapseOrExpand(UIHierarchyItem item, bool expand)
{
    foreach (UIHierarchyItem uiItem in item.UIHierarchyItems)
    {
        ProjectItem pi = uiItem.Object as ProjectItem;
        if (pi != null && (pi.Kind == VirtualFolder || pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
        {
            continue;
        }

        if (0 < uiItem.UIHierarchyItems.CountEx())
        {
            CollapseOrExpand(uiItem, expand);

            uiItem.UIHierarchyItems.Expanded = expand;
        }
    }
}
private UIHierarchyItem GetSelectedItem()
{
    var selectedItems = solutionExplorer.SelectedItems as UIHierarchyItem[];
    return selectedItems?.FirstOrDefault();
}
private ProjectItem GetSelectedProjectItem()
{
    return GetSelectedItem()?.Object as ProjectItem;
}
private void GetSearchList(ref List<SearchItem> searchList, UIHierarchyItem item)
{
    foreach (UIHierarchyItem uiItem in item.UIHierarchyItems)
    {
        ProjectItem pi = uiItem.Object as ProjectItem;
        if (pi != null && pi.Kind == VirtualFolder)
        {
            continue;
        }

        searchList.Add(new SearchItem() { Name = uiItem.Name.ToLower(), Item = uiItem });

        if (pi != null && (pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
        {
            continue;
        }

        if (0 < uiItem.UIHierarchyItems.CountEx())
        {
            GetSearchList(ref searchList, uiItem);
        }
    }
}
private static int CountEx(this UIHierarchyItems items)
{
    //UIHierarchyItems can not get count unless expanded.
    if (items.Expanded == false)
    {
        items.Expanded = true;
        items.Expanded = false;
    }
    return items.Count;
}
public class RegexUtil
{
    private static Regex countRegex = new Regex("^[0-9]*");
    public static int GetCount(string buffer)
    {
        Match match = countRegex.Match(buffer);
        int count = 1;
        if (match.Success)
        {
            int.TryParse(match.Value, out count);
            count = Math.Max(count, 1);
        }

        return count;
    }
}
private class SearchItem
{
    public string Name { get; set; } = string.Empty;
    public UIHierarchyItem Item { get; set; } = null;
}
