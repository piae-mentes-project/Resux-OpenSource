/// <summary>
/// ָ��ӿ�
/// </summary>
public interface ICommand
{
    /// <summary>ָ������</summary>
    string Description { get;}

    /// <summary>ִ��</summary>
    void Execute();

    /// <summary>����</summary>
    void Undo();
}
