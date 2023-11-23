using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Data
{
    #region enum

    /// <summary>
    /// 教程的总阶段
    /// </summary>
    public enum TutorialPhase
    {
        Start,
        Playing,
        End
    }

    #endregion

    #region class/sturct

    public class User
    {
        /// <summary>用于标识游戏内玩家的id，会展示给玩家看</summary>
        public string Uid { get; private set; }
        public string NickName { get; private set; }
        public Sprite Avatar { get; private set; }

        public User(string nickname, string avatarUrl)
        {
            this.NickName = nickname;
            SetAvatar(avatarUrl);
        }

        public void SetAvatar(string avatarUrl)
        {
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                Assets.ImageLoader.LoadSpriteFromUrl(avatarUrl, avatarImage =>
                {
                    Avatar = avatarImage;
                });
            }
            else
            {
                Avatar = Resources.Load<Sprite>("Image/DefaultImage/DefaultAvatar");
            }
        }

        /// <summary>
        /// 设置游戏内的uid，此id唯一
        /// </summary>
        /// <param name="uid"></param>
        public void SetUid(string uid)
        {
            this.Uid = uid;
        }

        public override string ToString()
        {
            return $"id: {Uid}, nickName: {NickName}";
        }
    }

    /// <summary>
    /// 存档
    /// </summary>
    public class SavedArchive
    {
        public List<ScoreRecord> ScoreRecords;

        public SavedArchive()
        {
            ScoreRecords = new List<ScoreRecord>();
        }
    }

    #endregion
}
