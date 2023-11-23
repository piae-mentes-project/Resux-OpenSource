using System.Collections;
using UnityEngine;
using System;

namespace Resux.UI
{
    /// <summary>
    /// 循环列表UI的物体接口
    /// </summary>
    public interface IRecycleListItem<T>
    {
        void Initialize();
        void AddOnEnterListener(Action action);
        void AddOnExitListener(Action action);
        void AddOnUpExitListener(Action action);
        void AddOnDownExitListener(Action action);
        void AddEventListener(Action<T> listener);
        void UpdateData(T data, int index);
        void OnEnter();
        void OnExit();
        void OnScroll(Vector2 viewArea);
        void Hide();
        void Show();
        float GetHeight();
        float GetWidth();
        T GetData();
        /// <summary>
        /// 更新UI位置
        /// </summary>
        /// <param name="index">该格子对应的实际数据索引</param>
        /// <param name="padding">每行之间的间隔</param>
        /// <param name="space">前后的预留行数</param>
        void UpdatePositon(int index, int padding, int space);
    }
}