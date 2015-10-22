using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FastWalk. This class cannot be inherited.
    /// </summary>
    internal sealed class FastWalk : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FastWalk" /> class.
        /// </summary>
        public FastWalk()
        {
            MinRank = -3;
            Description = "Enable/Disable Fast Walk.";
            Usage = ":fastwalk";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user =
                Azure.GetGame()
                    .GetRoomManager()
                    .GetRoom(session.GetHabbo().CurrentRoomId)
                    .GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().Id);
            user.FastWalking = !user.FastWalking;
            return true;
        }
    }
}