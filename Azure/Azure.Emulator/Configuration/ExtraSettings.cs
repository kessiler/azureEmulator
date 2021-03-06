﻿using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Azure.Configuration
{
    /// <summary>
    /// Class ExtraSettings.
    /// </summary>
    internal class ExtraSettings
    {
        /// <summary>
        /// The currenc y_ loo p_ enabled
        /// </summary>
        internal static bool CurrencyLoopEnabled = true;

        /// <summary>
        /// The current y_ loo p_ tim e_ i n_ minutes
        /// </summary>
        internal static int CurrentyLoopTimeInMinutes = 15;

        /// <summary>
        /// The credit s_ t o_ give
        /// </summary>
        internal static int CreditsToGive = 3000;

        /// <summary>
        /// The pixel s_ t o_ give
        /// </summary>
        internal static int PixelsToGive = 100;

        /// <summary>
        /// The youtub e_ thumbnai l_ suburl
        /// </summary>
        internal static string YoutubeThumbnailSuburl = "youtubethumbnail.php?Video";

        /// <summary>
        /// The diamond s_ loo p_ enabled
        /// </summary>
        internal static bool DiamondsLoopEnabled = true;

        /// <summary>
        /// The diamond s_ vi p_ only
        /// </summary>
        internal static bool DiamondsVipOnly = true;

        /// <summary>
        /// The diamond s_ t o_ give
        /// </summary>
        internal static int DiamondsToGive = 1;

        /// <summary>
        /// The chang e_ nam e_ staff
        /// </summary>
        internal static bool ChangeNameStaff = true;

        /// <summary>
        /// The chang e_ nam e_ vip
        /// </summary>
        internal static bool ChangeNameVip = true;

        /// <summary>
        /// The chang e_ nam e_ everyone
        /// </summary>
        internal static bool ChangeNameEveryone = true;

        /// <summary>
        /// The ne w_users_gifts_ enabled
        /// </summary>
        internal static bool NewUsersGiftsEnabled = true;

        /// <summary>
        /// The ServerCamera from Stories
        /// </summary>
        internal static string StoriesApiServerUrl = "";

        /// <summary>
        /// The ServerCamera from Stories
        /// </summary>
        internal static string StoriesApiThumbnailServerUrl = "";

        /// <summary>
        /// The ServerCamera from Stories
        /// </summary>
        internal static string StoriesApiHost = "";

        /// <summary>
        /// The enabl e_ bet a_ camera
        /// </summary>
        internal static bool EnableBetaCamera = true;

        /// <summary>
        /// The ne w_ use r_ GIF t_ ytt V2_ identifier
        /// </summary>
        internal static uint NewUserGiftYttv2Id = 4930;

        /// <summary>
        /// The everyon e_ us e_ floor
        /// </summary>
        internal static bool EveryoneUseFloor = true;

        /// <summary>
        /// The new page commands
        /// </summary>
        internal static bool NewPageCommands;

        /// <summary>
        /// The figuredata URL
        /// </summary>
        internal static string FiguredataUrl = "http://localhost/gamedata/figuredata/1.xml";

        /// <summary>
        /// The furni data URL
        /// </summary>
        internal static string FurniDataUrl;

        /// <summary>
        /// The admin can use HTML
        /// </summary>
        internal static bool AdminCanUseHtml = true, CryptoClientSide;

        internal static string WelcomeMessage = "";
        internal static string GameCenterStoriesUrl;

        /// <summary>
        /// Runs the extra settings.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool RunExtraSettings()
        {
            if (File.Exists("Settings/Welcome/message.txt"))
                WelcomeMessage = File.ReadAllText("Settings/Welcome/message.txt");

            if (!File.Exists("Settings/other.ini"))
                return false;

            foreach (var theParams in from line in File.ReadAllLines("Settings/other.ini", Encoding.Default) where !string.IsNullOrWhiteSpace(line) && line.Contains("=") select line.Split('='))
            {
                switch (theParams[0])
                {
                    case "currency.loop.enabled":
                        CurrencyLoopEnabled = theParams[1] == "true";
                        break;

                    case "youtube.thumbnail.suburl":
                        YoutubeThumbnailSuburl = theParams[1];
                        break;

                    case "gamecenter.stories.url":
                        GameCenterStoriesUrl = theParams[1];
                        break;

                    case "currency.loop.time.in.minutes":
                        int i;
                        if (int.TryParse(theParams[1], out i))
                            CurrentyLoopTimeInMinutes = i;
                        break;

                    case "credits.to.give":
                        int j;
                        if (int.TryParse(theParams[1], out j))
                            CreditsToGive = j;
                        break;

                    case "pixels.to.give":
                        int k;
                        if (int.TryParse(theParams[1], out k))
                            PixelsToGive = k;
                        break;

                    case "diamonds.loop.enabled":
                        DiamondsLoopEnabled = theParams[1] == "true";
                        break;

                    case "diamonds.to.give":
                        int l;
                        if (int.TryParse(theParams[1], out l))
                            DiamondsToGive = l;
                        break;

                    case "diamonds.vip.only":
                        DiamondsVipOnly = theParams[1] == "true";
                        break;

                    case "change.name.staff":
                        ChangeNameStaff = theParams[1] == "true";
                        break;

                    case "change.name.vip":
                        ChangeNameVip = theParams[1] == "true";
                        break;

                    case "change.name.everyone":
                        ChangeNameEveryone = theParams[1] == "true";
                        break;

                    case "enable.beta.camera":
                        EnableBetaCamera = theParams[1] == "true";
                        break;

                    case "newuser.gifts.enabled":
                        NewUsersGiftsEnabled = theParams[1] == "true";
                        break;

                    case "newuser.gift.yttv2.id":
                        uint u;
                        if (uint.TryParse(theParams[1], out u))
                            NewUserGiftYttv2Id = u;
                        break;

                    case "everyone.use.floor":
                        EveryoneUseFloor = theParams[1] == "true";
                        break;

                    case "figuredata.url":
                        FiguredataUrl = theParams[1];
                        break;

                    case "furnidata.url":
                        FurniDataUrl = theParams[1];
                        break;

                    case "admin.can.useHTML":
                        AdminCanUseHtml = theParams[1] == "true";
                        break;

                    case "commands.new.page":
                        NewPageCommands = theParams[1] == "true";
                        break;

                    case "stories.api.url":
                        StoriesApiServerUrl = theParams[1];
                        break;

                    case "stories.api.thumbnail.url":
                        StoriesApiThumbnailServerUrl = theParams[1];
                        break;

                    case "stories.api.host":
                        StoriesApiHost = theParams[1];
                        break;

                    case "rc4.client.side.enabled":
                        CryptoClientSide = theParams[1] == "true";
                        break;
                }
            }

            return true;
        }
    }
}