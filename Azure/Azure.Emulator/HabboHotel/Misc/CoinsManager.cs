#region

using System;
using System.Linq;
using System.Timers;
using Azure.Configuration;

#endregion

namespace Azure.HabboHotel.Misc
{
    /// <summary>
    /// Class CoinsManager.
    /// </summary>
    internal class CoinsManager
    {
        /// <summary>
        /// The _timer
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        internal void StartTimer()
        {
            if (!ExtraSettings.CURRENCY_LOOP_ENABLED)
                return;
            _timer = new Timer(ExtraSettings.CURRENTY_LOOP_TIME_IN_MINUTES * 60000);
            _timer.Elapsed += GiveCoins;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Gives the coins.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        internal void GiveCoins(object source, ElapsedEventArgs e)
        {
            try
            {
                var clients = Azure.GetGame().GetClientManager().Clients.Values;
                foreach (
                    var client in clients.Where(client => client != null && client.GetHabbo() != null))
                {
                    client.GetHabbo().Credits += ExtraSettings.CREDITS_TO_GIVE;
                    client.GetHabbo().UpdateCreditsBalance();
                    client.GetHabbo().ActivityPoints += ExtraSettings.PIXELS_TO_GIVE;
                    if (ExtraSettings.DIAMONDS_LOOP_ENABLED)
                        if (ExtraSettings.DIAMONDS_VIP_ONLY)
                            if (client.GetHabbo().VIP || client.GetHabbo().Rank >= 6) client.GetHabbo().Diamonds += ExtraSettings.DIAMONDS_TO_GIVE;
                            else client.GetHabbo().Diamonds += ExtraSettings.DIAMONDS_TO_GIVE;
                    client.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
            }
            catch (Exception ex)
            {
                Writer.Writer.LogException(ex.ToString());
            }
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            _timer.Dispose();
            _timer = null;
        }
    }
}