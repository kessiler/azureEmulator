#region

using System.Collections.Generic;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class EffectUser : IWiredItem
    {
        public EffectUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type
        {
            get
            {
                return Interaction.ActionTeleportTo;
            }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            List<Interaction> list = new List<Interaction> { Interaction.TriggerRepeater };
            if (stuff[0] == null) return false;
            var roomUser = (RoomUser)stuff[0];
            var item = (Interaction)stuff[1];

            List<Interaction> ConnetWired = new List<Interaction> { Interaction.ActionEffectUser };
            {
                int effectID = 0;
                if (int.TryParse(OtherString, out effectID))
                {
                    if (roomUser != null && !string.IsNullOrEmpty(OtherString))
                    {
                        roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectID);
                    }
                }
            }
            return true;
        }
    }
}