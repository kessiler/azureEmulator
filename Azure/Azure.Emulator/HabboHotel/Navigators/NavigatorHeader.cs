#region

using System;

#endregion

namespace Azure.HabboHotel.Navigators
{
    /// <summary>
    /// Struct NavigatorHeader
    /// </summary>
    internal struct NavigatorHeader
    {
        /// <summary>
        /// The room identifier
        /// </summary>
        internal UInt32 RoomId;

        /// <summary>
        /// The caption
        /// </summary>
        internal String Caption;

        /// <summary>
        /// The image
        /// </summary>
        internal String Image;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatorHeader"/> struct.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="image">The image.</param>
        internal NavigatorHeader(UInt32 roomId, String caption, String image)
        {
            RoomId = roomId;
            Caption = caption;
            Image = image;
        }
    }
}