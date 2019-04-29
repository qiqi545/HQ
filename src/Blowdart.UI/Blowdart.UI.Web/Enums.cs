// ReSharper disable once CheckNamespace

using System;

public enum InputType : byte
{
	None = 0,
    Button = 1,
    Checkbox = 2,
    Color = 3,
    Date = 4,
    DateTimeLocal = 5,
    Email = 6,
    File = 7,
    Hidden = 8,
    Image = 9,
    Month = 10,
    Number = 11,
    Password = 12,
    Radio = 13,
    Range = 14,
    Reset = 15,
    Search = 16,
    Submit = 17,
    Tel = 18,
    Text = 19,
    Time = 20,
    Url = 21,
    Week = 22
}

[Flags]
public enum InputAttributes
{
    ReadOnly = 0 << 1,
    Disabled = 0 << 2,
    NoValidate = 0 << 3,
    AutoFocus = 0 << 4,
    FormNoValidate = 0 << 5,
    Multiple = 0 << 6,
    Required = 0 << 7,
    None = 0,
    All = 0xFFFFFF
}
