using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class PlayerInfoView : MonoBehaviour
    {
        #region properties

        [SerializeField] private Image avatar;
        [SerializeField] private Text nickName;
        [SerializeField] [Tooltip("玩家称号")] private Text title;
        [SerializeField] private Text playingTime;

        #endregion

        #region Public Method

        public void Initialize(Data.User user)
        {
            if (nickName != null)
            {
                nickName.text = user.NickName;
            }

            avatar.sprite = user.Avatar;
        }

        #endregion
    }
}
