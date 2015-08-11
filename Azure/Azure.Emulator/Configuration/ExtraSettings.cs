#region

using System;
using System.IO;
using System.Linq;
using System.Text;

#endregion

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
        internal static bool CURRENCY_LOOP_ENABLED = true;

        /// <summary>
        /// The current y_ loo p_ tim e_ i n_ minutes
        /// </summary>
        internal static int CURRENTY_LOOP_TIME_IN_MINUTES = 15;

        /// <summary>
        /// The credit s_ t o_ give
        /// </summary>
        internal static int CREDITS_TO_GIVE = 3000;

        /// <summary>
        /// The pixel s_ t o_ give
        /// </summary>
        internal static int PIXELS_TO_GIVE = 100;

        /// <summary>
        /// The youtub e_ thumbnai l_ suburl
        /// </summary>
        internal static string YOUTUBE_THUMBNAIL_SUBURL = "youtubethumbnail.php?Video";

        /// <summary>
        /// The diamond s_ loo p_ enabled
        /// </summary>
        internal static bool DIAMONDS_LOOP_ENABLED = true;

        /// <summary>
        /// The diamond s_ vi p_ only
        /// </summary>
        internal static bool DIAMONDS_VIP_ONLY = true;

        /// <summary>
        /// The diamond s_ t o_ give
        /// </summary>
        internal static int DIAMONDS_TO_GIVE = 1;

        /// <summary>
        /// The chang e_ nam e_ staff
        /// </summary>
        internal static bool CHANGE_NAME_STAFF = true;

        /// <summary>
        /// The chang e_ nam e_ vip
        /// </summary>
        internal static bool CHANGE_NAME_VIP = true;

        /// <summary>
        /// The chang e_ nam e_ everyone
        /// </summary>
        internal static bool CHANGE_NAME_EVERYONE = true;

        /// <summary>
        /// The ne w_users_gifts_ enabled
        /// </summary>
        internal static bool NEW_users_gifts_ENABLED = true;

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
        internal static bool ENABLE_BETA_CAMERA = true;

        /// <summary>
        /// The ne w_ use r_ GIF t_ ytt V2_ identifier
        /// </summary>
        internal static uint NEW_USER_GIFT_YTTV2_ID = 4930;

        /// <summary>
        /// The everyon e_ us e_ floor
        /// </summary>
        internal static bool EVERYONE_USE_FLOOR = true;

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
        internal static bool AdminCanUseHTML = true, CryptoClientSide;

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
            foreach (var @params in from line in File.ReadAllLines("Settings/other.ini", Encoding.Default) where !String.IsNullOrWhiteSpace(line) && line.Contains("=") select line.Split('='))
            {
                switch (@params[0])
                {
                    case "currency.loop.enabled":
                        CURRENCY_LOOP_ENABLED = @params[1] == "true";
                        break;

                    case "youtube.thumbnail.suburl":
                        YOUTUBE_THUMBNAIL_SUBURL = @params[1];
                        break;

                    case "gamecenter.stories.url":
                        GameCenterStoriesUrl = @params[1];
                        break;

                    case "currency.loop.time.in.minutes":
                        int i;
                        if (int.TryParse(@params[1], out i))
                            CURRENTY_LOOP_TIME_IN_MINUTES = i;
                        break;

                    case "credits.to.give":
                        int j;
                        if (int.TryParse(@params[1], out j))
                            CREDITS_TO_GIVE = j;
                        break;

                    case "pixels.to.give":
                        int k;
                        if (int.TryParse(@params[1], out k))
                            PIXELS_TO_GIVE = k;
                        break;

                    case "diamonds.loop.enabled":
                        DIAMONDS_LOOP_ENABLED = @params[1] == "true";
                        break;

                    case "diamonds.to.give":
                        int l;
                        if (int.TryParse(@params[1], out l))
                            DIAMONDS_TO_GIVE = l;
                        break;

                    case "diamonds.vip.only":
                        DIAMONDS_VIP_ONLY = @params[1] == "true";
                        break;

                    case "change.name.staff":
                        CHANGE_NAME_STAFF = @params[1] == "true";
                        break;

                    case "change.name.vip":
                        CHANGE_NAME_VIP = @params[1] == "true";
                        break;

                    case "change.name.everyone":
                        CHANGE_NAME_EVERYONE = @params[1] == "true";
                        break;

                    case "enable.beta.camera":
                        ENABLE_BETA_CAMERA = @params[1] == "true";
                        break;

                    case "newuser.gifts.enabled":
                        NEW_users_gifts_ENABLED = @params[1] == "true";
                        break;

                    case "newuser.gift.yttv2.id":
                        uint u;
                        if (uint.TryParse(@params[1], out u))
                            NEW_USER_GIFT_YTTV2_ID = u;
                        break;

                    case "everyone.use.floor":
                        EVERYONE_USE_FLOOR = @params[1] == "true";
                        break;

                    case "figuredata.url":
                        FiguredataUrl = @params[1];
                        break;

                    case "furnidata.url":
                        FurniDataUrl = @params[1];
                        break;

                    case "admin.can.useHTML":
                        AdminCanUseHTML = @params[1] == "true";
                        break;

                    case "commands.new.page":
                        NewPageCommands = @params[1] == "true";
                        break;
                     
                    case "stories.api.url":
                        StoriesApiServerUrl = @params[1];
                        break;

                    case "stories.api.thumbnail.url":
                        StoriesApiThumbnailServerUrl = @params[1];
                        break;

                    case "stories.api.host":
                        StoriesApiHost = @params[1];
                        break;

                    case "rc4.client.side.enabled":
                        CryptoClientSide = @params[1] == "true";
                        break;
                }
            }
            return true;
        }
    }
}