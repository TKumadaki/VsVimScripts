
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using Vim;
using Vim.Extensions;

public DTE2 GetDTE2()
{
    return Package.GetGlobalService(typeof(DTE)) as DTE2;
}
public void DisplayError(string input)
{
    Vim.ActiveStatusUtil.OnError(input);
}
public void DisplayStatus(string input)
{
    Vim.ActiveStatusUtil.OnStatus(input);
}
public void DisplayStatusLong(IEnumerable<string> value)
{
    Vim.ActiveStatusUtil.OnStatusLong(value);
}
public void DisplayWarning(string input)
{
    Vim.ActiveStatusUtil.OnWarning(input);
}
public bool TryGetActiveVimBuffer(out IVimBuffer vimBuffer)
{
    var activeVimBuffer = Vim.ActiveBuffer;
    if (activeVimBuffer.IsNone())
    {
        vimBuffer = null;
        return false;
    }
    vimBuffer = activeVimBuffer.Value;
    return true;
}
public bool TryGetWpfTextView(IVimBuffer vimBuffer, out IWpfTextView textView)
{
    textView = vimBuffer?.TextView as IWpfTextView;
    if (textView == null)
    {
        return false;
    }
    return true;
}
public void Process(IVimBuffer vimBuffer, KeyInput keyInput)
{
    vimBuffer.ProcessFromScript(keyInput);
}
public void Process(IVimBuffer vimBuffer, string input, bool enter = false)
{
    foreach (var c in input)
    {
        var i = KeyInputUtil.CharToKeyInput(c);
        Process(vimBuffer, i);
    }

    if (enter)
    {
        Process(vimBuffer, KeyInputUtil.EnterKey);
    }
}
