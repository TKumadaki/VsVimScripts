#load "util.csx"

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Vim;

IWpfTextView textView;

if (!VimBuffer.TryGetWpfTextView(out textView))
{
    VimBuffer.DisplayError("Can not get WpfTextView");
    return;
}

if (textView.Selection.IsEmpty)
    return;


int lineCount = 0;
int startLine = 0;
int endLine = 0;

ITextSelection selection = textView.Selection;
VirtualSnapshotPoint start = selection.Start;
VirtualSnapshotPoint end = selection.End;

startLine = start.Position.GetContainingLine().LineNumber;
endLine = end.Position.GetContainingLine().LineNumber;
lineCount = Math.Abs(endLine - startLine);

VimBuffer.KeyInputStart += OnKeyInputStart;
VimBuffer.Closed += OnBufferClosed;

private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    long lineNumber;

    if (e.KeyInput.Char == 'k')
    {
        lineNumber = textView.Selection.Start.Position.GetContainingLine().LineNumber;
        if (lineNumber == 0)
        {
            return;
        }
        VimBuffer.SwitchMode(ModeKind.Command, ModeArgument.None);

        VimBuffer.KeyInputStart -= OnKeyInputStart;

        VimBuffer.Process("'<,'>move '<-2", enter: true);
        VimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);

        textView.Selection.Clear();

        VimBuffer.Process("0", enter: false);
        VimBuffer.SwitchMode(ModeKind.VisualLine, ModeArgument.None);

        if (1 < lineCount)
        {
            VimBuffer.Process((lineCount - 1).ToString() + "j", enter: false);
        }
        VimBuffer.KeyInputStart += OnKeyInputStart;
    }
    else if (e.KeyInput.Char == 'j')
    {
        lineNumber = textView.Selection.End.Position.GetContainingLine().LineNumber;
        if (lineNumber == (textView.TextSnapshot.LineCount - 1))
        {
            return;
        }
        VimBuffer.SwitchMode(ModeKind.Command, ModeArgument.None);

        VimBuffer.KeyInputStart -= OnKeyInputStart;

        VimBuffer.Process("'<,'>move '>+1", enter: true);
        VimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);

        textView.Selection.Clear();

        VimBuffer.Process("0", enter: false);
        VimBuffer.SwitchMode(ModeKind.VisualLine, ModeArgument.None);
        if (1 < lineCount)
        {
            VimBuffer.Process((lineCount - 1).ToString() + "j", enter: false);
        }
        VimBuffer.KeyInputStart += OnKeyInputStart;
    }
    else
    {
        EndIntercept();
        return;
    }
}
private void EndIntercept()
{
    VimBuffer.KeyInputStart -= OnKeyInputStart;
    VimBuffer.Closed -= OnBufferClosed;
}
private void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
