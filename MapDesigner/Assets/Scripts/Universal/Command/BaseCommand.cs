using System;
using System.Collections.Generic;

public abstract class BaseCommand : ICommand
{
    public abstract string Description { get; }

    public virtual void Execute()
    {
        CommandManager.Instance.AddCommandToUndo(this);
    }

    public virtual void Undo()
    {
        CommandManager.Instance.AddCommandToRedo(this);
    }
}
