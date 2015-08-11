#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Azure.Collections;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Games
{
    internal class GameManager
    {
        internal int[] TeamPoints;
        private QueuedDictionary<uint, RoomItem> _redTeamItems;
        private QueuedDictionary<uint, RoomItem> _blueTeamItems;
        private QueuedDictionary<uint, RoomItem> _greenTeamItems;
        private QueuedDictionary<uint, RoomItem> _yellowTeamItems;
        private Room _room;

        public GameManager(Room room)
        {
            TeamPoints = new int[5];
            _redTeamItems = new QueuedDictionary<uint, RoomItem>();
            _blueTeamItems = new QueuedDictionary<uint, RoomItem>();
            _greenTeamItems = new QueuedDictionary<uint, RoomItem>();
            _yellowTeamItems = new QueuedDictionary<uint, RoomItem>();
            _room = room;
        }

        internal event TeamScoreChangedDelegate OnScoreChanged;

        internal event RoomEventDelegate OnGameStart;

        internal event RoomEventDelegate OnGameEnd;

        internal int[] Points
        {
            get { return TeamPoints; }
            set { TeamPoints = value; }
        }

        internal void OnCycle()
        {
            _redTeamItems.OnCycle();
            _blueTeamItems.OnCycle();
            _greenTeamItems.OnCycle();
            _yellowTeamItems.OnCycle();
        }

        internal QueuedDictionary<uint, RoomItem> GetItems(Team team)
        {
            switch (team)
            {
                case Team.red:
                    return _redTeamItems;

                case Team.green:
                    return _greenTeamItems;

                case Team.blue:
                    return _blueTeamItems;

                case Team.yellow:
                    return _yellowTeamItems;

                default:
                    return new QueuedDictionary<uint, RoomItem>();
            }
        }

        internal Team GetWinningTeam()
        {
            var result = 1;
            var num = 0;
            for (var i = 1; i < 5; i++)
            {
                if (TeamPoints[i] <= num) continue;
                num = TeamPoints[i];
                result = i;
            }
            return (Team)result;
        }

        internal void AddPointToTeam(Team team, RoomUser user)
        {
            AddPointToTeam(team, 1, user);
        }

        internal void AddPointToTeam(Team team, int points, RoomUser user)
        {
            var num = (TeamPoints[(int)team] += points);

            if (num < 0) num = 0;

            TeamPoints[(int)team] = num;
            if (OnScoreChanged != null) OnScoreChanged(null, new TeamScoreChangedArgs(num, team, user));
            foreach (
                var current in
                    GetFurniItems(team).Values.Where(current => !IsSoccerGoal(current.GetBaseItem().InteractionType)))
            {
                current.ExtraData = TeamPoints[(int)team].ToString();
                current.UpdateState();
            }

            _room.GetWiredHandler().ExecuteWired(Interaction.TriggerScoreAchieved, user);
        }

        internal void Reset()
        {
            {
                AddPointToTeam(Team.blue, GetScoreForTeam(Team.blue) * -1, null);
                AddPointToTeam(Team.green, GetScoreForTeam(Team.green) * -1, null);
                AddPointToTeam(Team.red, GetScoreForTeam(Team.red) * -1, null);
                AddPointToTeam(Team.yellow, GetScoreForTeam(Team.yellow) * -1, null);
            }
        }

        internal void AddFurnitureToTeam(RoomItem item, Team team)
        {
            switch (team)
            {
                case Team.red:
                    _redTeamItems.Add(item.Id, item);
                    return;

                case Team.green:
                    _greenTeamItems.Add(item.Id, item);
                    return;

                case Team.blue:
                    _blueTeamItems.Add(item.Id, item);
                    return;

                case Team.yellow:
                    _yellowTeamItems.Add(item.Id, item);
                    return;

                default:
                    return;
            }
        }

        internal void RemoveFurnitureFromTeam(RoomItem item, Team team)
        {
            switch (team)
            {
                case Team.red:
                    _redTeamItems.Remove(item.Id);
                    return;

                case Team.green:
                    _greenTeamItems.Remove(item.Id);
                    return;

                case Team.blue:
                    _blueTeamItems.Remove(item.Id);
                    return;

                case Team.yellow:
                    _yellowTeamItems.Remove(item.Id);
                    return;

                default:
                    return;
            }
        }

        internal RoomItem GetFirstScoreBoard(Team team)
        {
            switch (team)
            {
                case Team.red:
                    goto IL_BF;
                case Team.green:
                    break;

                case Team.blue:
                    using (var enumerator = _blueTeamItems.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (current.GetBaseItem().InteractionType != Interaction.FreezeBlueCounter) continue;
                            var result = current;
                            return result;
                        }
                        goto IL_151;
                    }
                case Team.yellow:
                    goto IL_108;
                default:
                    goto IL_151;
            }
            using (var enumerator2 = _greenTeamItems.Values.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    var current2 = enumerator2.Current;
                    if (current2.GetBaseItem().InteractionType != Interaction.FreezeGreenCounter) continue;
                    var result = current2;
                    return result;
                }
                goto IL_151;
            }
        IL_BF:
            using (var enumerator3 = _redTeamItems.Values.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    var current3 = enumerator3.Current;
                    if (current3.GetBaseItem().InteractionType != Interaction.FreezeRedCounter) continue;
                    var result = current3;
                    return result;
                }
                goto IL_151;
            }
        IL_108:
            foreach (
                var result in
                    _yellowTeamItems.Values.Where(
                        current4 => current4.GetBaseItem().InteractionType == Interaction.FreezeYellowCounter))
                return result;
            IL_151:
            return null;
        }

        internal void UnlockGates()
        {
            foreach (var current in _redTeamItems.Values) UnlockGate(current);
            foreach (var current2 in _greenTeamItems.Values) UnlockGate(current2);
            foreach (var current3 in _blueTeamItems.Values) UnlockGate(current3);
            foreach (var current4 in _yellowTeamItems.Values) UnlockGate(current4);
        }

        internal void LockGates()
        {
            foreach (var current in _redTeamItems.Values) LockGate(current);
            foreach (var current2 in _greenTeamItems.Values) LockGate(current2);
            foreach (var current3 in _blueTeamItems.Values) LockGate(current3);
            foreach (var current4 in _yellowTeamItems.Values) LockGate(current4);
        }

        internal void StopGame()
        {
            var team = GetWinningTeam();
            var item = GetFirstHighscore();
            if (item == null || _room == null) return;
            var winners = new List<RoomUser>();
            switch (team)
            {
                case Team.blue:
                    winners = GetRoom().GetTeamManagerForFreeze().BlueTeam;
                    break;

                case Team.red:
                    winners = GetRoom().GetTeamManagerForFreeze().RedTeam;
                    break;

                case Team.yellow:
                    winners = GetRoom().GetTeamManagerForFreeze().YellowTeam;
                    break;

                case Team.green:
                    winners = GetRoom().GetTeamManagerForFreeze().GreenTeam;
                    break;
            }
            var score = GetScoreForTeam(team);
            foreach (var winner in winners) item.HighscoreData.addUserScore(item, winner.GetUserName(), score);
            item.UpdateState(false, true);
            if (OnGameEnd != null) OnGameEnd(null, null);
        }

        internal void StartGame()
        {
            if (OnGameStart != null) OnGameStart(null, null);
            GetRoom().GetWiredHandler().ResetExtraString(Interaction.ActionGiveScore);
        }

        internal Room GetRoom()
        {
            return _room;
        }

        internal void Destroy()
        {
            Array.Clear(TeamPoints, 0, TeamPoints.Length);
            _redTeamItems.Destroy();
            _blueTeamItems.Destroy();
            _greenTeamItems.Destroy();
            _yellowTeamItems.Destroy();
            TeamPoints = null;
            OnScoreChanged = null;
            OnGameStart = null;
            OnGameEnd = null;
            _redTeamItems = null;
            _blueTeamItems = null;
            _greenTeamItems = null;
            _yellowTeamItems = null;
            _room = null;
        }

        private static bool IsSoccerGoal(Interaction type)
        {
            return type == Interaction.FootballGoalBlue || type == Interaction.FootballGoalGreen ||
                   type == Interaction.FootballGoalRed || type == Interaction.FootballGoalYellow;
        }

        private int GetScoreForTeam(Team team)
        {
            return TeamPoints[(int)team];
        }

        private QueuedDictionary<uint, RoomItem> GetFurniItems(Team team)
        {
            switch (team)
            {
                case Team.red:
                    return _redTeamItems;

                case Team.green:
                    return _greenTeamItems;

                case Team.blue:
                    return _blueTeamItems;

                case Team.yellow:
                    return _yellowTeamItems;

                default:
                    return new QueuedDictionary<uint, RoomItem>();
            }
        }

        private void LockGate(RoomItem item)
        {
            var interactionType = item.GetBaseItem().InteractionType;
            if (!InteractionTypes.AreFamiliar(GlobalInteractions.GameGate, interactionType)) return;
            foreach (var current in _room.GetGameMap().GetRoomUsers(new Point(item.X, item.Y))) current.SqState = 0;
            _room.GetGameMap().GameMap[item.X, item.Y] = 0;
        }

        private void UnlockGate(RoomItem item)
        {
            var interactionType = item.GetBaseItem().InteractionType;
            if (!InteractionTypes.AreFamiliar(GlobalInteractions.GameGate, interactionType)) return;

            foreach (var current in _room.GetGameMap().GetRoomUsers(new Point(item.X, item.Y))) current.SqState = 1;
            _room.GetGameMap().GameMap[item.X, item.Y] = 1;
        }

        internal RoomItem GetFirstHighscore()
        {
            using (var enumerator = _room.GetRoomItemHandler().FloorItems.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current2 = enumerator.Current;
                    if (current2.GetBaseItem().InteractionType != Interaction.WiredHighscore) continue;
                    var result = current2;
                    return result;
                }
            }
            return null;
        }
    }
}