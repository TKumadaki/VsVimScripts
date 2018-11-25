using System;
using System.Collections.Generic;
using System.Linq;

public class Options
{
    private List<Option> mOptionList = new List<Option>();

    public void Add(Option opt)
    {
        mOptionList.Add(opt);
    }

    public object this[string name]
    {
        get
        {
            int idx = mOptionList.FindIndex(x => x.Equals(name));
            if (0 <= idx)
            {
                return mOptionList[idx].OptionValue;
            }

            return null;
        }
    }

    public void Parse(string argString)
    {
        var args = argString.Split(' ').Select((x) => x = x.Trim()).ToArray();
        string val;

        foreach (var opt in mOptionList)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!opt.Equals(args[i]))
                    continue;

                val = string.Empty;
                if (!opt.NameOnly && (i + 1) < args.Length)
                {
                    val = args[i + 1];
                }
                opt.SetValue(opt, val);
                break;
            }
        }
    }

    public string[] Descriptions()
    {
        return mOptionList.Select((x) => x.Description).ToArray();
    }
    public string[] DescriptionsAndValues()
    {
        return mOptionList.Select((x) => $"{x.Description} {x.GetDisplayValue(x)}").ToArray();
    }
    public string GetDisplayValues()
    {
        var values = mOptionList.Select((x) => $"{x.Name} {x.GetDisplayValue(x)}");

        return string.Join(" ", values);
    }
}
public class Option
{
    public string Name { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public object OptionValue { get; set; } = null;
    public bool NameOnly { get; set; } = false;
    public string Description { get; set; } = string.Empty;

    public Action<Option, string> SetValue = null;

    public Func<Option, string> GetDisplayValue = null;

    public bool Equals(string name)
    {
        return (this.Name.ToLower() == name.ToLower() || this.Alias.ToLower() == name.ToLower());
    }
}
