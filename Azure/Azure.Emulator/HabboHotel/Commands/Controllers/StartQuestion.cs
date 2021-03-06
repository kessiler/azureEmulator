﻿using System.Threading;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Polls;
using Azure.HabboHotel.Polls.Enums;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class StartQuestion. This class cannot be inherited.
    /// </summary>
    internal sealed class StartQuestion : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartQuestion" /> class.
        /// </summary>
        public StartQuestion()
        {
            MinRank = 7;
            Description = "Starts a matching question.";
            Usage = ":startquestion [id]";
            MinParams = 1;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var id = uint.Parse(pms[0]);
            var poll = Azure.GetGame().GetPollManager().TryGetPollById(id);
            if (poll == null || poll.Type != PollType.Matching)
            {
                client.SendWhisper("Poll doesn't exists or isn't a matching poll.");
                return true;
            }
            poll.AnswersPositive = 0;
            poll.AnswersNegative = 0;
            MatchingPollAnswer(client, poll);
            var showPoll = new Thread(delegate () { MatchingPollResults(client, poll); });
            showPoll.Start();
            return true;
        }

        internal static void MatchingPollAnswer(GameClient client, Poll poll)
        {
            if (poll == null || poll.Type != PollType.Matching)
                return;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
            message.AppendString("MATCHING_POLL");
            message.AppendInteger(poll.Id);
            message.AppendInteger(poll.Id);
            message.AppendInteger(15580);
            message.AppendInteger(poll.Id);
            message.AppendInteger(29);
            message.AppendInteger(5);
            message.AppendString(poll.PollName);
            client.GetHabbo().CurrentRoom.SendMessage(message);
        }

        internal static void MatchingPollResults(GameClient client, Poll poll)
        {
            var room = client.GetHabbo().CurrentRoom;
            if (poll == null || poll.Type != PollType.Matching || room == null)
                return;

            var users = room.GetRoomUserManager().GetRoomUsers();

            for (var i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                foreach (var roomUser in users)
                {
                    var user = Azure.GetHabboById(roomUser.UserId);
                    if (user.AnsweredPool)
                    {
                        var result =
                            new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollResultMessageComposer"));
                        result.AppendInteger(poll.Id);
                        result.AppendInteger(2);
                        result.AppendString("0");
                        result.AppendInteger(poll.AnswersNegative);
                        result.AppendString("1");
                        result.AppendInteger(poll.AnswersPositive);
                        client.SendMessage(result);
                    }
                }
            }

            foreach (var roomUser in users)
                Azure.GetHabboById(roomUser.UserId).AnsweredPool = false;
        }
    }
}