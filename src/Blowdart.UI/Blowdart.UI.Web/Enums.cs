// ReSharper disable once CheckNamespace

using System;

public enum InputType
{
    Button,
    Checkbox,
    Color,
    Date,
    DateTimeLocal,
    Email,
    File,
    Hidden,
    Image,
    Month,
    Number,
    Password,
    Radio,
    Range,
    Reset,
    Search,
    Submit,
    Tel,
    Text,
    Time,
    Url,
    Week
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
