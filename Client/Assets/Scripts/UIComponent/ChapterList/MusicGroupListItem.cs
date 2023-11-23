using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class MusicGroupListItem : AbstractRecycleListItem<ChapterDetail>
    {
        #region properties

        [SerializeField] private Text GroupName;
        [SerializeField] private Button btn;

        private int defaultSize;

        #endregion

        #region override

        public override void Initialize()
        {
            base.Initialize();
            defaultSize = GroupName.fontSize;
            btn.onClick.AddListener((() => EventAction(data)));
        }

        public override void UpdateData(ChapterDetail data, int index)
        {
            base.UpdateData(data, index);
            GroupName.text = data.name.Localize();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnScroll(Vector2 viewArea)
        {
            var distance = 300f;
            int appendSize = 45;
            var offset = (yArea.x + yArea.y) / 2 - (viewArea.x + viewArea.y) / 2;
            // Debug.Log($"index: {index} offset:  {offset}");
            offset = Mathf.Abs(offset);
            offset = distance - Mathf.Clamp(offset, 0, distance);
            var scale = offset / distance;
            // 最大增加字体大小 / 最大生效距离
            GroupName.fontSize = defaultSize + (int)(scale * appendSize);
            // 当字体增加比例大于一半时，就是最大项，视为被选中了
            if (scale >= 0.5f)
            {
                EventAction?.Invoke(data);
            }
            base.OnScroll(viewArea);
        }

        #endregion
    }
}
