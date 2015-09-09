#region

using System.Threading;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Polls;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class StartQuestion. This class cannot be inherited.
    /// </summary>
    internal sealed class StartQuestion : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartQuestion"/> class.
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
            if (poll == null || poll.Type != Poll.PollType.Matching)
            {
                client.SendWhisper("Poll doesn't exists or isn't a matching poll.");
                return true;
            }
            poll.answersPositive = 0;
            poll.answersNegative = 0;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
            message.AppendString("MATCHING_POLL");
            message.AppendInteger(poll.Id); //poll id
            message.AppendInteger(poll.Id); //question_id
            message.AppendInteger(15580);
            message.AppendInteger(poll.Id); //question_id
            message.AppendInteger(29); //number
            message.AppendInteger(5); //type
            message.AppendString(poll.PollName);
            client.GetHabbo().CurrentRoom.SendMessage(message);
            Thread ShowPoll = new Thread(delegate () { MatchingPollResults(client.GetHabbo().CurrentRoom, poll); });
            ShowPoll.Start();
            return true;
        }

        internal static void MatchingPollResults(Room room, Poll poll)
        {
            if (poll == null || poll.Type != Poll.PollType.Matching || room == null)
                return;
            for(int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                var result = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollResultMessageComposer"));
                result.AppendInteger(poll.Id);//question_id
                result.AppendInteger(2);//while
                result.AppendString("0");
                result.AppendInteger(poll.answersNegative);
                result.AppendString("1");
                result.AppendInteger(poll.answersPositive);
                room.SendMessage(result);
            }
        }
    }
}