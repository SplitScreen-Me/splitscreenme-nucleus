using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal class RemoveGame
    {
        public static bool Remove(UserGameInfo userGameInfo, bool confirm)
        {
            if (!confirm)
            {
                return _Remove(userGameInfo);              
                //return true;
            }

            DialogResult dialogResult = 
                MessageBox.Show($"Are you sure you want to uninstall {userGameInfo.HandlerInfo.GameName} handler?", "Uninstall handler?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                return _Remove(userGameInfo);
            }

            return false;
        }

        private static bool _Remove(UserGameInfo userGameInfo)
        {
            GameManager.Instance.RemoveUserGameInfo(userGameInfo);

            //just check here if it exist so we skip the "no data found" prompt
            if (Directory.Exists(userGameInfo.HandlerInfo.GenericGameInfo.Content_Folder))
            {
                CleanGameContent.CleanContentFolder(userGameInfo.HandlerInfo.GenericGameInfo, true);
            }

            GameAssets.CleanGamesAssets(userGameInfo,true);
            Core_Interface.RemoveGameFromList(userGameInfo.GameGuid);
            
            Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|{userGameInfo?.HandlerInfo.GenericGameInfo.GameName}@* *@{OSDHighlightColors.Regular}|has been uninstalled successfully@*");

            return true;
        }
    }
}
