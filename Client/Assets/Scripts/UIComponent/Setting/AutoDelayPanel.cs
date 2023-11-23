using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resux.Manager;
using System;
using Resux.Assets;
using Resux.Data;
using Resux.UI.Component.Effect;

namespace Resux.UI
{
    public class AutoDelayPanel : BasePopupView
    {
        #region properties

        #region Scene Object

        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private RawImage backgroundImage;
        [SerializeField, Tooltip("响应延迟")] private ExtendEventTrigger AddDelayButton;
        [SerializeField, Tooltip("播放")] private Button PlayButton;
        [SerializeField] private Button OKButton;
        [SerializeField] private Text PlayButtonText;
        [SerializeField] private Text OkButtonText;
        [SerializeField] private Text DelayText;
        [SerializeField] private Text BackButtonText;
        [SerializeField] private Text tapButtonText;
        [Space]
        [Header("四个校准点（ms)")]
        [SerializeField] private List<int> recordJudgeTap = new List<int>();

        #endregion

        private List<int> offsetList = new List<int>();
        private int maxDelay = 600;
        private int minDelay = -600;
        private double autoDelayMusicPlayingDspTimeMS = 0;

        private bool hasAlreadyFourTapsFlowOver;
        private int currentIndex = 0;
        private bool isPlaying;
        private event Action OnOkCallback;

        private bool interactable;

        private GamePreSetting preSetting;

        #endregion

        #region UnityEngine

        void OnDisable()
        {
            AudioPlayManager.Instance.PlayBGM(Sounds.Bgm.RootScene);
        }

        void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            var offset = (int)((AudioSettings.dspTime - autoDelayMusicPlayingDspTimeMS) * 1000 + 0.5f);
            if (offset > recordJudgeTap[3])
            {
                OnPlayEnd();
            }
        }

        #endregion

        #region Public Method

        public override void Initialize()
        {
            Logger.Log("AutoDelayPanel Init");

            // 这里添加了back的监听
            base.Initialize();

            interactable = false;

            // 添加按钮监听
            OKButton.onClick.AddListener(OnAutoDelayConfirmButtonClicked);
            // 刚点进来的时候禁用点Ok
            OKButton.interactable = false;
            PlayButton.onClick.AddListener(OnPlayButtonClicked);
            AddDelayButton.onPointerDown += _ => OnAddDelayButtonClicked();

            titleText.text = "SET_OFFSET_WIZARD_TITLE".Localize();
            hintText.text = "SET_OFFSET_WIZARD_HINT".Localize();
            BackButtonText.text = "SET_OFFSET_WIZARD_BACK".Localize();
            tapButtonText.text = "SET_OFFSET_WIZARD_TAP".Localize();
            PlayButtonText.text = "SET_OFFSET_WIZARD_PLAY".Localize();
            OkButtonText.text = "SET_OFFSET_WIZARD_FINISH".Localize();
        }
        
        public void SetSetting(GamePreSetting gamePreSetting)
        {
            preSetting = gamePreSetting;
        }

        public void SetBackground(Texture2D bg)
        {
            backgroundImage.texture = bg;
            backgroundImage.GetComponent<ImageBlurGPU>().StartBlur(false);
        }

        public void AddOkButtonListener(Action onOk)
        {
            OnOkCallback += onOk;
        }

        public void SetDelayText(int delay)
        {
            // 第四拍再显示延迟
            if (currentIndex > 3)
            {
                DelayText.text = delay > 0 ? $"+{delay}" : delay.ToString();
            }
            else
            {
                DelayText.text = "-";
            }
        }

        //the parameter of the next funtion is a void function to be called when the button is clicked 
        public void OnAutoDelayConfirmButtonClicked()
        {
            Logger.Log("On AutoDelayConfirmButtonClicked");
            if (hasAlreadyFourTapsFlowOver)
            {
                //get the value of the delay
                if (currentIndex >= 4)
                {
                    SetDelay(GetAutoDelay());
                    // close the panel
                    Close();
                }
                else
                {
                    Logger.Log("fourTapsRealityNums:" + currentIndex);
                }
            }
        }

        public override void Close()
        {
            OnOkCallback?.Invoke();
            base.Close();
        }

        #endregion

        #region Protected Method



        #endregion

        #region Private Method

        private void OnAddDelayButtonClicked()
        {
            Logger.Log("On AddDelayButtonClicked");

            if (!interactable)
            {
                return;
            }

            // 点击的时间距离音乐起始时间的偏移
            // 先算出偏移的秒数再转换
            int clickOffset = (int)((AudioSettings.dspTime - autoDelayMusicPlayingDspTimeMS) * 1000 + 0.5f);
            Logger.Log("On AddDelayButtonClicked:clickOffset:" + clickOffset);
            if (currentIndex < 4)
            {
                var currentCondition = recordJudgeTap[currentIndex];
                // 实际偏移ms
                var offset = clickOffset - currentCondition;
                offset = Mathf.Clamp(offset, minDelay, maxDelay);
                offsetList.Add(offset);
                currentIndex++;
            }
            else
            {
                interactable = false;
            }

            SetDelayText(GetAutoDelay());
        }

        private void OnPlayButtonClicked()
        {
            Logger.Log("delay PlayMusic");
            currentIndex = 0;

            PlayButton.interactable = false;
            OKButton.interactable = false;
            interactable = true;
            //清空offsetList，防止上一次的偏移影响下一次的偏移
            offsetList.Clear();
            autoDelayMusicPlayingDspTimeMS = AudioSettings.dspTime;
            hasAlreadyFourTapsFlowOver = false;
            isPlaying = true;

            AudioPlayManager.Instance.PlayBGM(Sounds.Bgm.AudioDelay, false);
        }

        private void SetDelay(int delay)
        {
            Logger.Log("delay:" + delay);
            if (delay >= minDelay && delay <= maxDelay)
            {
                preSetting.Offset = delay;
            }
        }

        private int GetAutoDelay()
        {
            int sumOfTheDelay = 0;
            foreach (var offset in offsetList)
            {
                sumOfTheDelay += offset;
            }
            //transform the sum of the delay to the average of the delay
            //and return the int value of the average of the delay
            var average = sumOfTheDelay / offsetList.Count;
            Logger.Log("sumOfTheDelay:" + sumOfTheDelay);
            Logger.Log("recordTimes:" + offsetList.Count);
            Logger.Log($"average: {average}");
            return average;
        }

        private void OnPlayEnd()
        {
            Logger.Log("delay effect Play End");
            hasAlreadyFourTapsFlowOver = true;
            isPlaying = false;
            if (hasAlreadyFourTapsFlowOver)
            {
                PlayButtonText.text = "SET_OFFSET_WIZARD_REPLAY".Localize();
            }
            PlayButton.interactable = true;
            OKButton.interactable = true;
        }

        #endregion
    }
}