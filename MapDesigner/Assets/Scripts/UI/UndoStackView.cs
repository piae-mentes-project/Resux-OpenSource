using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UndoStackView : BaseView
{
    public static UndoStackView Instance;

    #region properties

    [SerializeField] private Transform content;

    private GameObject contentPrefab;
    private Stack<UndoCommandDescriptionLine> commandDescriptions;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        Instance = this;
        commandDescriptions = new Stack<UndoCommandDescriptionLine>();
        contentPrefab = Resources.Load<GameObject>("Prefabs/CommandDescription");
        HideView();
    }

    public void AddCommand(string description)
    {
        var contentText = Instantiate<GameObject>(contentPrefab, content).GetComponent<UndoCommandDescriptionLine>();
        contentText.SetDescription(description);
        commandDescriptions.Push(contentText);
    }

    public void PopCommand()
    {
        Destroy(commandDescriptions.Pop().gameObject);
    }

    public void ClearCommandDescriptions()
    {
        while (commandDescriptions.Count > 0)
        {
            Destroy(commandDescriptions.Pop().gameObject);
        }

        commandDescriptions.Clear();
    }

    #endregion
}
