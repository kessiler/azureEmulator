#region

using System.Linq;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassEnable. This class cannot be inherited.
    /// </summary>
    internal sealed class MassEnable : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassEnable" /> class.
        /// </summary>
        public MassEnable()
        {
            MinRank = 7;
            Description = "Mass enable.";
            Usage = ":massenable [id]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            ushort effectId;
            if (!ushort.TryParse(pms[0], out effectId)) return true;

            var room = Azure.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            foreach (var user in room.GetRoomUserManager().GetRoomUsers().Where(user => !user.RidingHorse))
                user.ApplyEffect(effectId);
            return true;
        }
    }
}