#load "util.csx"

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Vim;

IVimBuffer vimBuffer;
IWpfTextView textView;

if (!TryGetActiveVimBuffer(out vimBuffer))
{
    DisplayError("Can not get VimBuffer");
    return;
}

if (!TryGetWpfTextView(vimBuffer, out textView))
{
    DisplayError("Can not get WpfTextView");
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
        Process(vimBuffer, KeyInputUtil.EscapeKey);
        Process(vimBuffer, ":", enter: false);
        Process(vimBuffer, "'<,'>move '<-2", enter: true);
        Process(vimBuffer, KeyInputUtil.EscapeKey);

        textView.Selection.Clear();

        Process(vimBuffer, "0", enter: false);
        Process(vimBuffer, "V", enter: false);

        if (1 < lineCount)
        {
            Process(vimBuffer, (lineCount - 1).ToString() + "j", enter: false);
        }
    }
    else if (e.KeyInput.Char == 'j')
    {
        lineNumber = textView.Selection.End.Position.GetContainingLine().LineNumber;
        if (lineNumber == (textView.TextSnapshot.LineCount - 1))
        {
            return;
        }

        Process(vimBuffer, KeyInputUtil.EscapeKey);
        Process(vimBuffer, ":", enter: false);
        Process(vimBuffer, "'<,'>move '>+1", enter: true);
        Process(vimBuffer, KeyInputUtil.EscapeKey);

        textView.Selection.Clear();

        Process(vimBuffer, "0", enter: false);
        Process(vimBuffer, "V", enter: false);
        if (1 < lineCount)
        {
            Process(vimBuffer, (lineCount - 1).ToString() + "j", enter: false);
        }
    }
    else
    {
        InterceptEnd();
        return;
    }
}
private void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
}
private void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}
