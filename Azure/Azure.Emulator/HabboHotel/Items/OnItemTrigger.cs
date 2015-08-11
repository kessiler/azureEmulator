#region

using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Delegate OnItemTrigger
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    public delegate void OnItemTrigger(object sender, ItemTriggeredArgs e);
}