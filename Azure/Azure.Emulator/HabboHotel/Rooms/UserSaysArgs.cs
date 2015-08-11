#region

using System;

#endregion

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class UserSaysArgs.
    /// </summary>
    public class UserSaysArgs : EventArgs
    {
        /// <summary>
        /// The user
        /// </summary>
        internal readonly RoomUser User;

        /// <summary>
        /// The message
        /// </summary>
        internal readonly string Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSaysArgs"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        public UserSaysArgs(RoomUser user, string message)
        {
            User = user;
            Message = message;
        }
    }
}