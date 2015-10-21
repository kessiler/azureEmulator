#region

using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MuteBots. This class cannot be inherited.
    /// </summary>
    internal sealed class MutePets : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MutePets" /> class.
        /// </summary>
        public MutePets()
        {
            MinRank = -2;
            Description = "Mute pets in your own room.";
            Usage = ":mutepets";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            room.MutedPets = !room.MutedPets;
            session.SendNotif(Azure.GetLanguage().GetVar("user_room_mute_pets"));

            return true;
        }
    }
}