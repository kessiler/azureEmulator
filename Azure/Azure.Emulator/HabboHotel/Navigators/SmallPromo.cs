#region

using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Navigators
{
    /// <summary>
    /// Class SmallPromo.
    /// </summary>
    public class SmallPromo
    {
        /// <summary>
        /// The index
        /// </summary>
        private readonly int Index;

        /// <summary>
        /// The header
        /// </summary>
        private readonly string Header;

        /// <summary>
        /// The body
        /// </summary>
        private readonly string Body;

        /// <summary>
        /// The button
        /// </summary>
        private readonly string Button;

        /// <summary>
        /// The in game promo
        /// </summary>
        private readonly int inGamePromo;

        /// <summary>
        /// The special action
        /// </summary>
        private readonly string SpecialAction;

        /// <summary>
        /// The image
        /// </summary>
        private readonly string Image;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmallPromo"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        /// <param name="button">The button.</param>
        /// <param name="inGame">The in game.</param>
        /// <param name="specialAction">The special action.</param>
        /// <param name="image">The image.</param>
        public SmallPromo(int index, string header, string body, string button, int inGame, string specialAction, string image)
        {
            Index = index;
            Header = header;
            Body = body;
            Button = button;
            inGamePromo = inGame;
            SpecialAction = specialAction;
            Image = image;
        }

        /// <summary>
        /// Serializes the specified composer.
        /// </summary>
        /// <param name="Composer">The composer.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage Serialize(ServerMessage Composer)
        {
            Composer.AppendInteger(Index);
            Composer.AppendString(Header);
            Composer.AppendString(Body);
            Composer.AppendString(Button);
            Composer.AppendInteger(inGamePromo);
            Composer.AppendString(SpecialAction);
            Composer.AppendString(Image);
            return Composer;
        }
    }
}