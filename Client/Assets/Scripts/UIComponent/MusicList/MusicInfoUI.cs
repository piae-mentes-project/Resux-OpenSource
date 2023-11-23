using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Resux.GamePlay;
using Resux.LevelData;
using UnityEngine.UI;

namespace Resux.UI
{
    public class MusicInfoUI : MonoBehaviour
    {
        [SerializeField] private ScrollImage musicScroll;

        [SerializeField] private Text currentTitleText;  // 单行
        [SerializeField] private Text currentTitleText1;  // 两行的上面
        [SerializeField] private Text currentTitleText2;  // 两行的下面
        [SerializeField] private Text musicAuthorText;

        [SerializeField] private Text previousTitleText;
        [SerializeField] private Text previousTitleLabel;
        [SerializeField] private Text nextTitleText;
        [SerializeField] private Text nextTitleLabel;

        [SerializeField] private GameObject previousGameObject;
        [SerializeField] private GameObject nextGameObject;

        private List<LevelData.LevelDetail> musicConfigs;

        private void Start()
        {
            // 本地化
            previousTitleLabel.text = "SONG_PREVIOUS".Localize();
            nextTitleLabel.text = "SONG_NEXT".Localize();

            musicScroll.onIndexChanged.AddListener(OnMusicScrollIndexChanged);
            musicConfigs = MusicScoreSelection.ChapterDetail.musicConfigs;
            OnMusicScrollIndexChanged();
        }

        private void OnMusicScrollIndexChanged()
        {
            var index = musicScroll.Index;

            var currentConfig = musicConfigs[index];
            musicAuthorText.text = currentConfig._artistName;

            if (string.IsNullOrEmpty(currentConfig._subTitle))
            {
                currentTitleText.gameObject.SetActive(true);
                currentTitleText.text = currentConfig._songName;
                currentTitleText1.gameObject.SetActive(false);
                currentTitleText2.gameObject.SetActive(false);
            }
            else
            {
                currentTitleText.gameObject.SetActive(false);
                currentTitleText1.gameObject.SetActive(true);
                currentTitleText2.gameObject.SetActive(true);
                currentTitleText1.text = currentConfig._songName;
                currentTitleText2.text = currentConfig._subTitle;
            }

            if (index <= 0)
            {
                previousGameObject.SetActive(false);
            }
            else
            {
                previousGameObject.SetActive(true);
                previousTitleText.text = musicConfigs[index - 1]._songName;
            }

            if (index >= musicScroll.MaxIndex)
            {
                nextGameObject.SetActive(false);
            }
            else
            {
                nextGameObject.SetActive(true);
                nextTitleText.text = musicConfigs[index + 1]._songName;
            }
        }
    }
}