using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectInfoView : MonoBehaviour
{
    #region properties

    [SerializeField] private Button openButton;
    [SerializeField] private Text infoText;

    private MapDesignerSettings.MapDesignerProjectSetting setting;

    #endregion

    #region Public Method

    public void Initialize(MapDesignerSettings.MapDesignerProjectSetting setting,
        UnityEngine.Events.UnityAction<MapDesignerSettings.MapDesignerProjectSetting> onOpenProject)
    {
        this.setting = setting;
        infoText.text = $"Music Name: {setting.MusicName}\nMusic Type: {setting.Ext}";
        openButton.onClick.AddListener(() =>
        {
            onOpenProject(this.setting);
        });
    }

    #endregion
}
