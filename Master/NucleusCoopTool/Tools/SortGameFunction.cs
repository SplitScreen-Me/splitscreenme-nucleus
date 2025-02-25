using Nucleus.Coop.UI;
using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleus.Coop.Tools
{
    public static class SortGameFunction
    {
        public static void SortGames(List<SortOptions> options)
        {
            List<UserGameInfo> tempList = new List <UserGameInfo>();
            List<UserGameInfo> games = Core_Interface.GameManager.User.Games;

            bool sortDescending = false;
            bool sortUpdate = false;
            bool sortMaxPlayer  = false;
            bool sortPlayTime   = false;
            bool sortLastPlayed = false;

            foreach (SortOptions sopt in options)
            {
                switch (sopt)
                {
                    case SortOptions.Gamepads:
                        {
                            var find = games.FindAll(g => (g.Game.Hook.XInputEnabled || g.Game.ProtoInput.XinputHook || g.Game.Hook.DInputEnabled || g.Game.Hook.XInputReroute || g.Game.ProtoInput.DinputDeviceHook || g.Game.Hook.SDL2Enabled) && tempList.All(tl => tl != g));
                            tempList.AddRange(find);
                            break;
                        }
                    case SortOptions.SKbm:
                        {
                            var find = games.FindAll(g => g.Game.SupportsKeyboard && tempList.All(tl => tl != g));
                            tempList.AddRange(find);
                            break;
                        }
                    case SortOptions.Kbm:
                        {
                            var find = games.FindAll(g => g.Game.SupportsMultipleKeyboardsAndMice && tempList.All(tl => tl != g));
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
                tempList = Core_Interface.GameManager.User.Games;
            }
     
            if (sortPlayTime)
            {
                tempList = tempList.OrderByDescending(g => g.Game.MetaInfo.TotalPlayTime).ToList();
            }
            else if (sortUpdate)
            {
                tempList = tempList.OrderBy(g => g.Game.UpdateAvailable).ToList();
            }
            else if (sortLastPlayed)
            {
                var notPlayedList = tempList.Where(g => g.Game.MetaInfo.LastPlayedAt == "...").ToList();
                tempList = tempList.Where(g => g.Game.MetaInfo.LastPlayedAt != "...").OrderByDescending(g => DateTime.Parse(g.Game.MetaInfo.LastPlayedAt)).ToList();
                
                if(notPlayedList != null)
                {
                    tempList.AddRange(notPlayedList);
                }              
            }
            else if (sortMaxPlayer)
            {
                tempList = tempList.OrderByDescending(g => g.Game.MaxPlayers).ToList();
            }
            else if(sortDescending)
            {
                tempList = tempList.OrderByDescending(g => g.Game.GameName).ToList();
            }

            Core_Interface.RefreshGames(null, tempList);                           
        }
    }
}
