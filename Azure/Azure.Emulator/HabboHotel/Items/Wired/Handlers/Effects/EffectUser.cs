#region

using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;

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

        public Interaction Type => Interaction.ActionTeleportTo;

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
            var list = new List<Interaction> { Interaction.TriggerRepeater };
            if (stuff[0] == null) return false;
            var roomUser = (RoomUser)stuff[0];
            var item = (Interaction)stuff[1];

            var connetWired = new List<Interaction> { Interaction.ActionEffectUser };
            {
                var effectId = 0;
                if (int.TryParse(OtherString, out effectId))
                {
                    if (roomUser != null && !string.IsNullOrEmpty(OtherString))
                    {
                        roomUser.GetClient()
                            .GetHabbo()
                            .GetAvatarEffectsInventoryComponent()
                            .ActivateCustomEffect(effectId);
                    }
                }
            }
            return true;
        }
    }
}