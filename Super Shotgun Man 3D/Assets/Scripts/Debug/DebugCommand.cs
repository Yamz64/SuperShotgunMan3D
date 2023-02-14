using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    private string _command_id;
    private string _command_description;
    private string _command_format;

    public string command_id { get { return _command_id; } }
    public string command_description { get { return _command_description; } }
    public string command_format { get { return _command_format; } }

    public DebugCommandBase(string id, string description, string format)
    {
        _command_id = id;
        _command_description = description;
        _command_format = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action command;

    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> command;

    public DebugCommand(string id, string description, string format, Action<T> command) : base(id, description, format)
    {
        this.command = command;
    }
    public void Invoke(T value)
    {
        command.Invoke(value);
    }
}
