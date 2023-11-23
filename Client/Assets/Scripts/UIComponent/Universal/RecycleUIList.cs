using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// 循环UI列表
    /// </summary>
    public class RecycleUIList<T>
    {
        #region properties

        private GameObject ItemPrefab;
        private ScrollRect scrollRect;
        private int padding;
        /// <summary>行间隔</summary>
        public int Padding
        {
            get => padding;
            set
            {
                if (value < 0)
                {
                    return;
                }

                padding = value;
                CalculateHeight();
                Refresh();
            }
        }

        private int spaceCount;
        /// <summary>列表前后预留的空间（行数）</summary>
        public int SpaceCount
        {
            get => spaceCount;
            set
            {
                if (value < 0)
                {
                    return;
                }

                spaceCount = value;
                CalculateHeight();
                Refresh();
            }
        }
        /// <summary>当前起始的真实索引</summary>
        private int currentIndex = 0;
        /// <summary>当前顶部UI的索引</summary>
        private int currentStartUIIndex = 0;
        /// <summary>循环列表的长度</summary>
        private int itemCount;
        /// <summary>循环利用的UI列表</summary>
        private List<IRecycleListItem<T>> recycleItems;
        /// <summary>全部的道具信息</summary>
        private List<T> itemDatas;

        #endregion

        #region Public Method

        public void Initialize(GameObject itemPrefab, ScrollRect scrollRect, List<T> itemDatas, Action<T> listener, int itemCount = 6)
        {
            ItemPrefab = itemPrefab;
            this.scrollRect = scrollRect;
            this.itemDatas = itemDatas;
            this.itemCount = itemCount;
            recycleItems = new List<IRecycleListItem<T>>();

            scrollRect.onValueChanged.AddListener(OnScrollChange);
            for (int i = 0; i < itemCount; i++)
            {
                var item = GameObject.Instantiate(itemPrefab, scrollRect.content).GetComponent<IRecycleListItem<T>>();
                item.Initialize();
                item.AddOnUpExitListener(OnUpExit);
                item.AddOnDownExitListener(OnDownExit);
                item.AddEventListener(listener);
                recycleItems.Add(item);
            }
            CalculateHeight();
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < itemCount; i++)
            {
                var index = RecycleIndex(currentStartUIIndex + i);
                var dataIndex = currentIndex + i;

                IRecycleListItem<T> item = recycleItems[index];
                item.UpdatePositon(dataIndex, Padding, SpaceCount);
                if (dataIndex >= 0 && dataIndex < itemDatas.Count)
                {
                    item.UpdateData(itemDatas[dataIndex], index);
                    item.Show();
                }
                else
                {
                    item.Hide();
                }
            }
        }

        #endregion

        #region Private Method

        private void CalculateHeight()
        {
            var height = recycleItems[0].GetHeight() + padding;
            var viewportSize = scrollRect.GetComponent<RectTransform>().rect.size;
            var ListContentViewHight = height * (itemDatas.Count + 2 * SpaceCount) - viewportSize.y;
            Vector2 newSize = scrollRect.content.sizeDelta;
            newSize.y = ListContentViewHight;
            scrollRect.content.sizeDelta = newSize;

            // Debug.Log($"itemDataCount: {itemDatas.Count} viewportSize: {viewportSize} height: {height}, content height: {ListContentViewHight}, sizeDelta: {scrollRect.content.sizeDelta}, size: {scrollRect.content.rect.size}");
        }

        private int RecycleIndex(int index)
        {
            if (index < 0)
            {
                index += itemCount;
            }

            return index % itemCount;
        }

        /// <summary>
        /// 列表滚动监听
        /// </summary>
        /// <param name="pos">vector2，值均为0-1的比例值</param>
        private void OnScrollChange(Vector2 pos)
        {
            var viewportSize = scrollRect.viewport.rect.size;
            var contentSize = scrollRect.content.sizeDelta;
            Vector2 viewArea = Vector2.zero;
            var yScale = Mathf.Max((1 - pos.y) * contentSize.y, 0);
            var xTop = Mathf.Max(contentSize.x * pos.x, 0);
            // x为区域顶部，y为区域底部
            // 数值为距离顶部的距离
            viewArea.x = yScale;
            viewArea.y = yScale + viewportSize.y;
            // Debug.Log($"<color=red>viewArea: {viewArea}, pos: {pos}, yScale: {yScale}</color>");

            recycleItems.ForEach(item => item.OnScroll(viewArea));
        }

        private void OnUpExit()
        {
            currentStartUIIndex = RecycleIndex(currentStartUIIndex + 1);
            currentIndex++;
            Refresh();
        }

        private void OnDownExit()
        {
            currentStartUIIndex = RecycleIndex(currentStartUIIndex - 1);
            currentIndex--;
            Refresh();
        }

        #endregion
    }
}