using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Resux.Data
{
    public static class AccountManager
    {
        #region properties

        public static User User { get; set; }

        /// <summary>是否开启教程</summary>
        public static bool IsEnableTutorial => TutorialPhase < TutorialPhase.End;

        private static TutorialPhase tutorialPhase;

        /// <summary>教程的阶段</summary>
        public static TutorialPhase TutorialPhase
        {
            get => tutorialPhase;
            set
            {
                tutorialPhase = value;

                UserLocalSettings.SetInt(ConstConfigs.LocalKey.TutorialKey, (int)value);
            }
        }

        #endregion

        #region Public Method

        static AccountManager()
        {
            User = new User("ResuxPlayer", string.Empty);
            TutorialPhase = (TutorialPhase) UserLocalSettings.GetInt(ConstConfigs.LocalKey.TutorialKey);
        }

        public static void FinishTutorial()
        {
            TutorialPhase = TutorialPhase.End;
            // UserLocalSettings.SetInt(ConstConfigs.LocalKey.TutorialKey, 1);
        }

        #endregion

        #region Private Method



        #endregion
    }
}