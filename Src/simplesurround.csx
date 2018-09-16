#load "util.csx"

using System;
using Vim;

readonly string START_TAG = "<";
readonly string END_TAG = ">";

bool tagStarted = false;
string addText = string.Empty;

IVimBuffer vimBuffer;

if(!TryGetActiveVimBuffer(out vimBuffer))
{
	DisplayError("Can not get VimBuffer");
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
        InterceptEnd();
        return;
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        if (tagStarted && !(string.IsNullOrWhiteSpace(addText)))
        {

            //START
            Process(vimBuffer, "I", enter: false);
            string sendText = START_TAG + addText + END_TAG;
            Process(vimBuffer, sendText, enter: false);
            Process(vimBuffer, KeyInputUtil.EscapeKey);

            //END
            string tag = addText;
            int idx = addText.IndexOf(' ');
            if (0 < idx)
            {
                tag = addText.Substring(0, idx);
            }
            Process(vimBuffer, "A", enter: false);
            sendText = START_TAG + "/" + tag + END_TAG;
            Process(vimBuffer, sendText, enter: false);
            Process(vimBuffer, KeyInputUtil.EscapeKey);
        }
        InterceptEnd();
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
        DisplayStatus(START_TAG + addText);
    }
}
private void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    DisplayStatus(string.Empty);
}
private void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}
