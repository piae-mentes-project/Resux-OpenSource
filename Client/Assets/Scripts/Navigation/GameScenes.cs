using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resux.Configuration
{
    /// <summary>场景</summary>
    public enum GameScene
    {
        None,
        /// <summary>曲目选集场景</summary>
        ChapterListScene,
        /// <summary>（选歌）主场景</summary>
        MusicListScene,
        /// <summary>各功能入口场景</summary>
        MainMenuScene,
        /// <summary>游戏入口场景</summary>
        EntryScene,
        /// <summary>游玩场景</summary>
        PlayScene,
        /// <summary>游戏结算场景</summary>
        ResultScene,
        /// <summary>游戏设置场景</summary>
        SettingScene,
        /// <summary>剧情阅读场景</summary>
        DialogScene,
        /// <summary>章节地图场景</summary>
        ChapterMapScene,
        /// <summary>剧情模式主场景</summary>
        StoryScene,
        /// <summary>玩家个人信息场景</summary>
        PlayerInfoScene,
    }
}
