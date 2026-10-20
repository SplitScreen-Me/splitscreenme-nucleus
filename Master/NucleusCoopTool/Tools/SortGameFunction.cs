using Nucleus.Coop.UI;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleus.Coop.Tools
{
    public static class SortGameFunction
    {

        public static List<UserGameInfo> SortGames(List<SortOptions> options)
        {
            List<UserGameInfo> tempList = new List<UserGameInfo>();
            List<UserGameInfo> userGameInfos = Core_Interface.GameManager.User.Games;

            bool sortDescending = false;
            bool sortUpdate = false;
            bool sortMaxPlayer = false;
            bool sortPlayTime = false;
            bool sortLastPlayed = false;

            foreach (SortOptions sopt in options)
            {
                switch (sopt)
                {
                    case SortOptions.Gamepads:
                        {
                            var find = userGameInfos.FindAll(g => (g.HandlerInfo.GenericGameInfo.Hook.XInputEnabled || g.HandlerInfo.GenericGameInfo.ProtoInput.XinputHook || g.HandlerInfo.GenericGameInfo.Hook.DInputEnabled || g.HandlerInfo.GenericGameInfo.Hook.XInputReroute || g.HandlerInfo.GenericGameInfo.ProtoInput.DinputDeviceHook || g.HandlerInfo.GenericGameInfo.Hook.SDL2Enabled || g.HandlerInfo.GenericGameInfo.Hook.SDL3Enabled) && tempList.All(tl => tl != g));
                            tempList.AddRange(find);
                            break;
                        }
                    case SortOptions.SKbm:
                        {
                            var find = userGameInfos.FindAll(g => g.HandlerInfo.GenericGameInfo.SupportsKeyboard && tempList.All(tl => tl != g));
                            tempList.AddRange(find);
                            break;
                        }
                    case SortOptions.Kbm:
                        {
                            var find = userGameInfos.FindAll(g => g.HandlerInfo.GenericGameInfo.SupportsMultipleKeyboardsAndMice && tempList.All(tl => tl != g));
                            tempList.AddRange(find);
                            break;
                        }
                    case SortOptions.MaxPLayer:
                        {
                            sortMaxPlayer = true;
                            break;
                        }
                    case SortOptions.PlayTime:
                        {
                            sortPlayTime = true;
                            break;
                        }

                    case SortOptions.LastPLayed:
                        {
                            sortLastPlayed = true;
                            break;
                        }

                    case SortOptions.Descending:
                        {
                            sortDescending = true;
                            break;
                        }
                    case SortOptions.Update:
                        {
                            sortUpdate = true;
                            break;
                        }
                }
            }

            if (tempList.Count == 0)
            {
                tempList = userGameInfos;
            }

            if (sortPlayTime)
            {
                tempList = tempList.OrderByDescending(g => g.HandlerInfo.GenericGameInfo.MetaInfo.TotalPlayTime).ToList();
            }
            else if (sortUpdate)
            {
                tempList = tempList.OrderByDescending(g => g.UpdateAvailable).ToList();

            }
            else if (sortLastPlayed)
            {
                var notPlayedList = tempList.Where(g => g.HandlerInfo.GenericGameInfo.MetaInfo.LastPlayedAtFull == "...").ToList();
                tempList = tempList.Where(g => g.HandlerInfo.GenericGameInfo.MetaInfo.LastPlayedAtFull != "...").OrderByDescending(g => DateTime.Parse(g.HandlerInfo.GenericGameInfo.MetaInfo.LastPlayedAtFull)).ToList();

                if (notPlayedList != null)
                {
                    tempList.AddRange(notPlayedList);
                }
            }
            else if (sortMaxPlayer)
            {
                tempList = tempList.OrderByDescending(g => g.HandlerInfo.GenericGameInfo.MaxPlayers).ToList();
            }
            else if (sortDescending)
            {
                tempList = tempList.OrderByDescending(g => g.HandlerInfo.GenericGameInfo.GameName).ToList();
            }

            if (App_Misc.ShowFavoriteOnly)
            {
                var favoritesOnly = tempList.Where(g => g.HandlerInfo.GenericGameInfo.MetaInfo.Favorite == true).ToList();

                if (favoritesOnly.Count > 0)
                {
                    tempList = favoritesOnly;
                }
            }

            //so the current selected control stay visible
            if (Core_Interface.Current_UserGameInfo != null && userGameInfos.Contains(Core_Interface.Current_UserGameInfo))
            {
                if (!tempList.Contains(Core_Interface.Current_UserGameInfo))
                {
                    tempList.Insert(0, Core_Interface.Current_UserGameInfo);
                }
            }

            return tempList;
        }

        public static List<UserGameInfo> SortGamesTextInput(string userInput)
        {
            List<UserGameInfo> tempList = new List<UserGameInfo>();
            List<UserGameInfo> userGameInfos = Core_Interface.GameManager.User.Games;

            for (int i = 0; i < userGameInfos.Count; i++)
            {
                UserGameInfo userGameInfo = userGameInfos[i];

                if(userGameInfo?.HandlerInfo?.GenericGameInfo == null)
                {
                    continue;
                }

                List<string> splittedName = userGameInfo.HandlerInfo.GenericGameInfo.GameName.ToLower().Split(' ').ToList();
                List<string> splittedSearch = userInput.ToLower().Split(' ').ToList();

                bool found = (userGameInfo.HandlerInfo.GenericGameInfo.GameName.ToLower().Contains(userInput.ToLower()) && userInput.Length > 1) ||
                              userGameInfo.HandlerInfo.GenericGameInfo.GameName.ToLower().StartsWith(userInput.ToLower());

                if (!found)
                {
                    found = splittedName.Any(s => s.StartsWith(userInput.ToLower())) && userInput.Length > 1;

                    if (!found)
                    {
                        found = splittedName.Any(s => s.StartsWith(userInput.ToLower()));
                    }
                }

                if (!found)
                {
                    continue;
                }

                tempList.Add(userGameInfo);
            }

            //so the current selected gamecontrol stay visible
            if (Core_Interface.Current_UserGameInfo != null && userGameInfos.Contains(Core_Interface.Current_UserGameInfo))
            {
                if (!tempList.Contains(Core_Interface.Current_UserGameInfo))
                {
                    tempList.Insert(0, Core_Interface.Current_UserGameInfo);
                }
            }

            return tempList;
        }
    }
}
