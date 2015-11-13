using System;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Pathfinding;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class Kill. This class cannot be inherited.
    /// </summary>
    internal sealed class Kill : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Kill" /> class.
        /// </summary>
        public Kill()
        {
            MinRank = -3;
            Description = "Kill someone.";
            Usage = ":kill";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            if (room == null) return true;

            var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().LastSelectedUser);
            if (user2 == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            var user =
                room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().UserName);
            if (PathFinder.GetDistance(user.X, user.Y, user2.X, user2.Y) > 1)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("kil_command_error_1"));

                return true;
            }
            if (user2.IsLyingDown || user2.IsSitting)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("kil_command_error_2"));
                return true;
            }
            if (
                !string.Equals(user2.GetUserName(), session.GetHabbo().UserName,
                    StringComparison.CurrentCultureIgnoreCase))
            {
                user2.Statusses.Add("lay", "0.55");
                user2.IsLyingDown = true;
                user2.UpdateNeeded = true;
                user.Chat(user.GetClient(), Azure.GetLanguage().GetVar("command.kill.user"), true, 0, 3);
                user2.Chat(user2.GetClient(), Azure.GetLanguage().GetVar("command.kill.userdeath"), true, 0,
                    3);
                return true;
            }
            user.Chat(session, "I am sad", false, 0);
            return true;
        }
    }
}