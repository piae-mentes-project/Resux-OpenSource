using System;
using System.Collections;
using System.Collections.Generic;
using Resux.Assets;
using Resux.GamePlay;
using Resux.LevelData;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class Swipe : MonoBehaviour
    {
        #region Delegate

        private Action<int, LevelData.LevelDetail> onFocusChange;

        #endregion

        public bool needWait = true;

        /// <summary>
        /// 垂直的Scrollbar游戏物件
        /// </summary>
        [SerializeField] Scrollbar SB;

        [SerializeField] int currentFocus;

        private List<LevelData.LevelDetail> configs => MusicScoreSelection.ChapterDetail.musicConfigs;

        private void Start()
        {
            // 初始化
            // 之后会从保存的配置读出来
            currentFocus = 0;
            needWait = false;

            var bars = new List<MusicSelectBarUI>();
            foreach (var config in configs)
            {
                bars.Add(AddMusicBar(config));
            }

            var distance = 0.49f;
            var nativeDistance = 0.5f;

            if (bars.Count > 1)
            {
                distance = .5f / (2 + bars.Count);
                nativeDistance = .5f / bars.Count;
                Logger.Log($"distance: {distance}");
            }

            for (int i = 0; i < bars.Count; i++)
            {
                var bar = bars[i];
                bar.Initialize(i, (2 * i + 1) * nativeDistance, distance, SB, this);
                bar.AddListener(OnFocus);
            }

            // 聚焦到默认歌曲
            // needWait = true;
            bars[currentFocus].ForceFocusOn((() => needWait = false));
        }

        private void Update()
        {
            // Debug.Log($"scroll value : {SB.value}");
            // SB.value = Mathf.Clamp(SB.value, 0f, 1f);
        }

        #region Public Method

        /// <summary>
        /// 添加聚焦索引监听
        /// </summary>
        /// <param name="focusChange">绑定方法</param>
        public void AddListener(Action<int, LevelData.LevelDetail> focusChange)
        {
            onFocusChange += focusChange;
        }

        /// <summary>
        /// 移除聚焦索引监听
        /// </summary>
        /// <param name="focusChange">解绑方法</param>
        public void DeleteListener(Action<int, LevelData.LevelDetail> focusChange)
        {
            if (focusChange != null) onFocusChange -= focusChange;
        }

        /// <summary>
        /// 获取当前聚焦信息
        /// </summary>
        /// <returns>音乐配置信息</returns>
        public LevelData.LevelDetail GetCurrentFoucs()
        {
            return configs[currentFocus];
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 聚焦监听方法
        /// </summary>
        /// <param name="index"></param>
        /// <param name="musicInfo"></param>
        private void OnFocus(int index, float pos, LevelData.LevelDetail levelDetail)
        {
            currentFocus = index;
            // 这里采用实时获取子对象的形式是时间换空间（（
            var bars = transform.GetComponentsInChildren<MusicSelectBarUI>();
            for (int i = 0; i < bars.Length; i++)
            {
                if (i != index)
                {
                    bars[i].ForceFocusOff();
                }
            }
            // 这里发送聚焦变更的通知
            onFocusChange?.Invoke(index, levelDetail);
        }

        /// <summary>
        /// 添加选歌条目
        /// </summary>
        /// <param name="config"></param>
        private MusicSelectBarUI AddMusicBar(LevelData.LevelDetail config)
        {
            var barPrefab = Resources.Load<GameObject>("Prefabs/UI/MusicSelectBar");
            var bar = Instantiate(barPrefab, transform).GetComponent<MusicSelectBarUI>();
            bar.SetMusicConfig(config);
            return bar;
        }

        #endregion
    }
}
