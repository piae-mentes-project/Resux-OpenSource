using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指令管理，撤回重做
/// </summary>
public class CommandManager
{
    private static CommandManager _instance;

    public static CommandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CommandManager();
            }

            return _instance;
        }
    }

    #region properties

    /// <summary>撤回重做的次数</summary>
    private int undoRedoCount = 20;

    private FixedCountStack<ICommand> undoCmdStack;
    private FixedCountStack<ICommand> redoCmdStack;

    #endregion

    private CommandManager()
    {
        undoCmdStack = new FixedCountStack<ICommand>(undoRedoCount);
        redoCmdStack = new FixedCountStack<ICommand>(undoRedoCount);

        PlayerInputManager.Instance.OnCtrlZAction += () =>
        {
            // 如果撤回失败，也就是没有可以撤回的命令
            if (!Undo())
            {
                Popup.ShowMessage("可撤回的命令栈为空！", Color.red);
            }
        };

        PlayerInputManager.Instance.OnCtrlYAction += () =>
        {
            // 如果重做失败，也就是没有可以重做的命令
            if (!Redo())
            {
                Popup.ShowMessage("可重做的命令栈为空！", Color.red);
            }
        };
    }

    #region Public Method

    /// <summary>
    /// 执行指令后加入撤回
    /// </summary>
    /// <param name="command">执行的命令</param>
    public void AddCommandToUndo(ICommand command)
    {
        undoCmdStack.Push(command);

        // 说明做的不是重做的部分
        if (command != redoCmdStack.Peek())
        {
            redoCmdStack.Clear();
        }

        UndoStackView.Instance.AddCommand(command.Description);
    }

    /// <summary>
    /// 撤回后将指令加入重做
    /// </summary>
    /// <param name="command">撤回的命令</param>
    public void AddCommandToRedo(ICommand command)
    {
        redoCmdStack.Push(command);

        UndoStackView.Instance.PopCommand();
    }

    public bool Undo()
    {
        if (undoCmdStack.Count <= 0)
        {
            return false;
        }

        var command = undoCmdStack.Pop();
        command.Undo();

        return true;
    }

    public bool Redo()
    {
        if (redoCmdStack.Count <= 0)
        {
            return false;
        }

        var command = redoCmdStack.Pop();
        command.Execute();

        return true;
    }

    #endregion
}
