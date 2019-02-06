#load "util.csx"

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Vim;

IVimBuffer vimBuffer;
IWpfTextView textView;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}

if (!vimBuffer.TryGetWpfTextView(out textView))
{
    Vim.DisplayError("Can not get WpfTextView");
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

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

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
        vimBuffer.SwitchMode(ModeKind.Command, ModeArgument.None);

        vimBuffer.KeyInputStart -= OnKeyInputStart;

        vimBuffer.Process("'<,'>move '<-2", enter: true);
        vimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);

        textView.Selection.Clear();

        vimBuffer.Process("0", enter: false);
        vimBuffer.SwitchMode(ModeKind.VisualLine, ModeArgument.None);

        if (1 < lineCount)
        {
            vimBuffer.Process((lineCount - 1).ToString() + "j", enter: false);
        }
        vimBuffer.KeyInputStart += OnKeyInputStart;
    }
    else if (e.KeyInput.Char == 'j')
    {
        lineNumber = textView.Selection.End.Position.GetContainingLine().LineNumber;
        if (lineNumber == (textView.TextSnapshot.LineCount - 1))
        {
            return;
        }
        vimBuffer.SwitchMode(ModeKind.Command, ModeArgument.None);

        vimBuffer.KeyInputStart -= OnKeyInputStart;

        vimBuffer.Process("'<,'>move '>+1", enter: true);
        vimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);

        textView.Selection.Clear();

        vimBuffer.Process("0", enter: false);
        vimBuffer.SwitchMode(ModeKind.VisualLine, ModeArgument.None);
        if (1 < lineCount)
        {
            vimBuffer.Process((lineCount - 1).ToString() + "j", enter: false);
        }
        vimBuffer.KeyInputStart += OnKeyInputStart;
    }
    else
    {
        EndIntercept();
        return;
    }
}
private void EndIntercept()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
}
private void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
