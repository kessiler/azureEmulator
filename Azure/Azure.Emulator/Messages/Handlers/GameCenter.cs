#region

using System;
using Azure.Configuration;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Games the center load game.
        /// </summary>
        internal void GameCenterLoadGame()
        {
            ServerMessage Achievements = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterGameAchievementsMessageComposer"));
            Achievements.AppendInteger(18);
            Achievements.AppendInteger(1);//count
            Achievements.AppendInteger(295);//id
            Achievements.AppendInteger(1);
            Achievements.AppendString("ACH_StoryChallengeChampion1");
            Achievements.AppendInteger(0);
            Achievements.AppendInteger(1);
            Achievements.AppendInteger(0);
            Achievements.AppendInteger(0);
            Achievements.AppendInteger(0);
            Achievements.AppendBool(false);
            Achievements.AppendString("games");
            Achievements.AppendString("elisa_habbo_stories");
            Achievements.AppendInteger(1);
            Achievements.AppendInteger(0);
            Achievements.AppendString("");
            Session.SendMessage(Achievements);

            ServerMessage WeeklyLeaderboard = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboardMessageComposer"));
            WeeklyLeaderboard.AppendInteger(2014);
            WeeklyLeaderboard.AppendInteger(49);
            WeeklyLeaderboard.AppendInteger(0);
            WeeklyLeaderboard.AppendInteger(0);
            WeeklyLeaderboard.AppendInteger(6526);
            WeeklyLeaderboard.AppendInteger(1);
            WeeklyLeaderboard.AppendInteger(Session.GetHabbo().Id);
            WeeklyLeaderboard.AppendInteger(0);
            WeeklyLeaderboard.AppendInteger(1);
            WeeklyLeaderboard.AppendString(Session.GetHabbo().UserName);
            WeeklyLeaderboard.AppendString(Session.GetHabbo().Look);
            WeeklyLeaderboard.AppendString(Session.GetHabbo().Gender);
            WeeklyLeaderboard.AppendInteger(1);
            WeeklyLeaderboard.AppendInteger(18);
            Session.SendMessage(WeeklyLeaderboard);

            ServerMessage WeeklyLeaderboard2 = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboard2MessageComposer"));
            WeeklyLeaderboard2.AppendInteger(2014);
            WeeklyLeaderboard2.AppendInteger(49);
            WeeklyLeaderboard2.AppendInteger(0);
            WeeklyLeaderboard2.AppendInteger(0);
            WeeklyLeaderboard2.AppendInteger(6526);
            WeeklyLeaderboard2.AppendInteger(1);
            WeeklyLeaderboard2.AppendInteger(Session.GetHabbo().Id);
            WeeklyLeaderboard2.AppendInteger(0);
            WeeklyLeaderboard2.AppendInteger(1);
            WeeklyLeaderboard2.AppendString(Session.GetHabbo().UserName);
            WeeklyLeaderboard2.AppendString(Session.GetHabbo().Look);
            WeeklyLeaderboard2.AppendString(Session.GetHabbo().Gender);
            WeeklyLeaderboard2.AppendInteger(0);
            WeeklyLeaderboard2.AppendInteger(18);
            Session.SendMessage(WeeklyLeaderboard2);

            ServerMessage WeeklyLeaderboard3 = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLeaderboard2MessageComposer"));
            WeeklyLeaderboard3.AppendInteger(2014);
            WeeklyLeaderboard3.AppendInteger(48);
            WeeklyLeaderboard3.AppendInteger(0);
            WeeklyLeaderboard3.AppendInteger(1);
            WeeklyLeaderboard3.AppendInteger(6526);
            WeeklyLeaderboard3.AppendInteger(1);
            WeeklyLeaderboard3.AppendInteger(Session.GetHabbo().Id);
            WeeklyLeaderboard3.AppendInteger(0);
            WeeklyLeaderboard3.AppendInteger(1);
            WeeklyLeaderboard3.AppendString(Session.GetHabbo().UserName);
            WeeklyLeaderboard3.AppendString(Session.GetHabbo().Look);
            WeeklyLeaderboard3.AppendString(Session.GetHabbo().Gender);
            WeeklyLeaderboard3.AppendInteger(0);
            WeeklyLeaderboard3.AppendInteger(18);
            Session.SendMessage(WeeklyLeaderboard3);

            ServerMessage GamesLeft = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterGamesLeftMessageComposer"));
            GamesLeft.AppendInteger(18);
            GamesLeft.AppendInteger(-1);
            GamesLeft.AppendInteger(0);
            Session.SendMessage(GamesLeft);

            ServerMessage PreviousWinner = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterPreviousWinnerMessageComposer"));
            PreviousWinner.AppendInteger(18);
            PreviousWinner.AppendInteger(0);

            PreviousWinner.AppendString("name");
            PreviousWinner.AppendString("figure");
            PreviousWinner.AppendString("gender");
            PreviousWinner.AppendInteger(0);
            PreviousWinner.AppendInteger(0);

            Session.SendMessage(PreviousWinner);

            /*ServerMessage Products = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterProductsMessageComposer"));
            Products.AppendInteger(18);//gameId
            Products.AppendInteger(0);//count
            Products.AppendInteger(6526);
            Products.AppendBool(false);
            Session.SendMessage(Products);*/

            ServerMessage AllAchievements = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterAllAchievementsMessageComposer"));
            AllAchievements.AppendInteger(0);//count

            //For Stories
            /*PacketName5.AppendInteger(18);
            PacketName5.AppendInteger(1);
            PacketName5.AppendInteger(191);
            PacketName5.AppendString("StoryChallengeChampion");
            PacketName5.AppendInteger(20);*/

            AllAchievements.AppendInteger(0);//gameId
            AllAchievements.AppendInteger(0);//count
            AllAchievements.AppendInteger(0);//achId
            AllAchievements.AppendString("SnowWarTotalScore");//achName
            AllAchievements.AppendInteger(0);//levels

            Session.SendMessage(AllAchievements);

            ServerMessage EnterInGame = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterEnterInGameMessageComposer"));
            EnterInGame.AppendInteger(18);
            EnterInGame.AppendInteger(0);
            Session.SendMessage(EnterInGame);
        }

        /// <summary>
        /// Games the center join queue.
        /// </summary>
        internal void GameCenterJoinQueue()
        {
            ServerMessage JoinQueue = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterJoinGameQueueMessageComposer"));
            JoinQueue.AppendInteger(18);
            Session.SendMessage(JoinQueue);

            ServerMessage LoadGame = new ServerMessage(LibraryParser.OutgoingRequest("GameCenterLoadGameUrlMessageComposer"));
            LoadGame.AppendInteger(18);
            LoadGame.AppendString(Convert.ToString(Azure.GetUnixTimeStamp()));
            LoadGame.AppendString(ExtraSettings.GameCenterStoriesUrl);
            Session.SendMessage(LoadGame);
        }
    }
}