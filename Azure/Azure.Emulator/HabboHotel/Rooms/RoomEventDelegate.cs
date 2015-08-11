#region

using System;

#endregion

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Delegate RoomEventDelegate
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    public delegate void RoomEventDelegate(object sender, EventArgs e);
}