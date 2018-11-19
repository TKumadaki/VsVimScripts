using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

public class TinyVimMode
{
    public string Buffer { get; set; } = string.Empty;
    public List<CommandParser> CommandParsers { get; set; } = new List<CommandParser>();
    public Action<object, KeyInputStartEventArgs> OnKeyInputStart { get; set; } = null;

    public void OnKeyInputStartNormalMode(object sender, KeyInputStartEventArgs e)
    {
        if (e.KeyInput.RawChar.IsSome() && !char.IsControl(e.KeyInput.Char))
        {
            Buffer += e.KeyInput.Char.ToString();
        }
        else
        {
            Buffer = string.Empty;
        }
        foreach (var cp in CommandParsers)
        {
            if (cp.CommandEquals(e.KeyInput))
            {
                cp.CommandAction(e.KeyInput);
                return;
            }
        }
        //no match
        Buffer = string.Empty;
    }
    public void OnKeyInputStartCommandPromptMode(object sender, KeyInputStartEventArgs e)
    {
        if (e.KeyInput.RawChar.IsSome() && !char.IsControl(e.KeyInput.Char))
        {
            Buffer += e.KeyInput.Char.ToString();
        }
        else if (e.KeyInput.Key == VimKey.Back)
        {
            if (1 < Buffer.Length)
            {
                Buffer = Buffer.Substring(0, Buffer.Length - 1);
            }
            else if (Buffer.Length == 1)
            {
                Buffer = string.Empty;
            }
        }
        foreach (var cp in CommandParsers)
        {
            if (cp.CommandEquals(e.KeyInput))
            {
                cp.CommandAction(e.KeyInput);
                return;
            }
        }
    }
}
public class CommandParser
{
    public Regex Regex { get; set; } = null;

    public Func<KeyInput, bool> CommandEquals { get; set; }

    public Action<KeyInput> CommandAction { get; set; }
}
