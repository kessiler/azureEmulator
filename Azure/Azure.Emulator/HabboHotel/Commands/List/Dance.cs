#region

using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Dance. This class cannot be inherited.
    /// </summary>
    internal sealed class Dance : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dance"/> class.
        /// </summary>
        public Dance()
        {
            MinRank = 1;
            Description = "Makes you dance.";
            Usage = ":dance [danceId(0 - 4)]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            ushort result;
            ushort.TryParse(pms[0], out result);

            if (result > 4)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_dance_false"));
                result = 0;
            }
            var message = new ServerMessage();
            message.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
            message.AppendInteger(session.CurrentRoomUserId);
            message.AppendInteger(result);
            session.GetHabbo().CurrentRoom.SendMessage(message);

            return true;
        }
    }
}