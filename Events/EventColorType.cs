using System;
using System.Collections.Generic;

namespace LethalHUD.Events;
public class EventColorEntry
{
    public string Name { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public Dictionary<string, string> Overrides { get; set; } = [];
}

public class EventColorConfig
{
    public List<EventColorEntry> Events { get; set; } = [];
}
