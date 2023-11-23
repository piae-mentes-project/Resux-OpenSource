/// <summary>
/// 指令接口
/// </summary>
public interface ICommand
{
    /// <summary>指令描述</summary>
    string Description { get;}

    /// <summary>执行</summary>
    void Execute();

    /// <summary>撤销</summary>
    void Undo();
}
