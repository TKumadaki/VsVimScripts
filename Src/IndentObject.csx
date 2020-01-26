#r "Microsoft.VisualStudio.Text.Data.dll"

#load "util.csx"

using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using Vim;

// Run the following two processes in the script
// Then, unintended behavior was caused.
// 
// VimBuffer.SwitchMode(ModeKind.VisualCharacter, ModeArgument.None);
// textView.Selection.Select(virtualStartPoint, virtualEndPoint);
//
// So I decided to do the processing in the OnSwitchedMode event.
// However, VsVim executes scripts asynchronously.
// Depending on your environment, this script may not work as expected.

VimBuffer.SwitchedMode += OnSwitchedMode;

public void OnSwitchedMode(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedMode;

    IWpfTextView textView;

    if (!VimBuffer.TryGetWpfTextView(out textView))
    {
        VimBuffer.DisplayError("Can not get WpfTextView");
        return;
    }

    ITextSnapshot snapshot = textView.TextSnapshot;
    List<ITextSnapshotLine> lines = snapshot.Lines.ToList();
    int startLineNumber = lines[0].Start.GetContainingLine().LineNumber;
    int caretLineNumber = textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

    int caretPositionIndex = 0;
    caretPositionIndex = caretLineNumber - startLineNumber;

    int currentIndent;

    currentIndent = GetIndent(lines[caretPositionIndex]);

    int indentLevel = 1;
    if (3 < Arguments.Trim().Length)
    {
        int.TryParse(Arguments.Trim().Substring(3), out indentLevel);
    }

    int indent = 0;
    int startIndentIndex = 0;
    int endIndentIndex = lines.Count - 1;
    int nextIndent = 0;

    for (var i = 0; i < indentLevel; i++)
    {
        nextIndent = int.MaxValue;
        for (var j = caretPositionIndex - 1; 0 <= j; j--)
        {
            indent = GetIndent(lines[j]);
            if (indent < 0)
                continue;

            if (indent < currentIndent)
            {
                nextIndent = indent;
                if (i == (indentLevel - 1))
                {
                    startIndentIndex = j;
                }
                break;
            }
        }

        for (var j = caretPositionIndex + 1; j < lines.Count; j++)
        {
            indent = GetIndent(lines[j]);
            if (indent < 0)
                continue;

            if (indent < currentIndent)
            {
                if (i == (indentLevel - 1))
                {
                    endIndentIndex = j;
                }
                break;
            }
        }
        currentIndent = nextIndent;
    }

    string textKind = Arguments.Trim().Substring(1, 2);

    switch (textKind)
    {
        case "ai":
            endIndentIndex--;
            break;

        case "aI":
            break;

        default: //"ii" "iI"
            startIndentIndex++;
            endIndentIndex--;
            break;
    }

    startIndentIndex = Math.Max(0, startIndentIndex);
    endIndentIndex = Math.Min(endIndentIndex, lines.Count - 1);

    textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, lines[endIndentIndex].End));

    string procesKind = Arguments.Trim().Substring(0, 1);

    VirtualSnapshotPoint virtualStartPoint;
    VirtualSnapshotPoint virtualEndPoint;
    ITextEdit edit;

    switch (procesKind)
    {
        case "c":
            //Change
            VimBuffer.SwitchedMode += OnSwitchedModeForChange;
            break;
        case "d":
            //Delete
            VimBuffer.SwitchedMode += OnSwitchedModeForDelete;
            break;
        case "v":
            break;
        case "y":
            //Yank
            VimBuffer.SwitchedMode += OnSwitchedModeForYank;
            break;
    }
    textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, lines[endIndentIndex].End));

    //When the script does the following, VsVim switches to Visual Mode.
    virtualStartPoint = new VirtualSnapshotPoint(snapshot, lines[startIndentIndex].Start);
    virtualEndPoint = new VirtualSnapshotPoint(snapshot, lines[endIndentIndex].End);

    textView.Selection.Select(virtualStartPoint, virtualEndPoint);
}

private int GetIndent(ITextSnapshotLine line)
{
    string text;

    text = line.GetText();

    if (string.IsNullOrWhiteSpace(text))
        return -1;

    return text.Length - text.TrimStart().Length;
}
public void OnSwitchedModeForChange(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedModeForChange;
    VimBuffer.Process("c", enter: false);
}
public void OnSwitchedModeForDelete(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedModeForDelete;
    VimBuffer.Process("d", enter: false);
}
public void OnSwitchedModeForYank(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedModeForYank;
    VimBuffer.Process("y", enter: false);
}


