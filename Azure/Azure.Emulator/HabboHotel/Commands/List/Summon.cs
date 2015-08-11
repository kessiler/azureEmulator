#region

using System;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Summon. This class cannot be inherited.
    /// </summary>
    internal sealed class Summon : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Summon"/> class.
        /// </summary>
        public Summon()
        {
            MinRank = 7;
            Description = "Summon the selected user to where you are.";
            Usage = ":summon [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            if (String.Equals(userName, session.GetHabbo().UserName,
                StringComparison.CurrentCultureIgnoreCase))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("summon_yourself"));
                return true;
            }
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(userName);
            if (client == null)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (session.GetHabbo().CurrentRoom != null &&
                session.GetHabbo().CurrentRoomId != client.GetHabbo().CurrentRoomId)
                client.GetMessageHandler()
                    .PrepareRoomForUser(session.GetHabbo().CurrentRoom.RoomId,
                        session.GetHabbo().CurrentRoom.RoomData.PassWord);
            return true;
        }
    }
}