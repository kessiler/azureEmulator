﻿using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class SetVideos. This class cannot be inherited.
    /// </summary>
    internal sealed class AddVideo : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetVideos" /> class.
        /// </summary>
        public AddVideo()
        {
            MinRank = -1;
            Description = "Add Youtube Video";
            Usage = ":setvideo [YOUTUBE VIDEO URL]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().GetYoutubeManager().AddUserVideo(session, pms[0]);
            return true;
        }
    }
}