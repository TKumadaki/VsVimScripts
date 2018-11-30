#load "util.csx"

using System;
using Vim;

readonly string START_TAG = "<";
readonly string END_TAG = ">";

bool tagStarted = false;
string addText = string.Empty;

IVimBuffer vimBuffer;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    if (e.KeyInput.Char == 't' && !tagStarted)
    {
        tagStarted = true;
    }
    else if (e.KeyInput.Key == VimKey.Escape)
    {
        EndIntercept();
        return;
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        if (tagStarted && !(string.IsNullOrWhiteSpace(addText)))
        {

            //START
            vimBuffer.Process("I", enter: false);
            string sendText = START_TAG + addText + END_TAG;
            vimBuffer.Process(sendText, enter: false);
            vimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);

            //END
            string tag = addText;
            int idx = addText.IndexOf(' ');
            if (0 < idx)
            {
                tag = addText.Substring(0, idx);
            }
            vimBuffer.Process("A", enter: false);
            sendText = START_TAG + "/" + tag + END_TAG;
            vimBuffer.Process(sendText, enter: false);
            vimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);
        }
        EndIntercept();
        return;
    }
    else if (e.KeyInput.Key == VimKey.Back)
    {
        if (1 < addText.Length)
        {
            addText = addText.Substring(0, addText.Length - 1);
        }
        else if (addText.Length == 1)
        {
            addText = string.Empty;
        }
    }
    else
    {
        addText += e.KeyInput.Char;
    }
    if (tagStarted)
    {
        Vim.DisplayStatus(START_TAG + addText);
    }
}
private void EndIntercept()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    Vim.DisplayStatus(string.Empty);
}
private void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
