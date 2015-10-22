using System;
using System.Collections.Generic;
using System.Data;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.Security
{
    internal class BobbaFilter
    {
        internal static List<string> Word;

        internal static bool CanTalk(GameClient session, string message)
        {
            if (CheckForBannedPhrases(message) && session.GetHabbo().Rank < 4)
            {
                if (!Azure.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id))
                    session.GetHabbo().BobbaFiltered++;

                if (session.GetHabbo().BobbaFiltered < 3)
                {
                    session.SendNotif("Your language is inappropriate. If you do not change this , measures are being taken by the automated system of Habbo.");
                }
                else if (session.GetHabbo().BobbaFiltered >= 3)
                {
                    if (session.GetHabbo().BobbaFiltered == 3)
                    {
                        session.GetHabbo().BobbaFiltered = 4;
                        Azure.MutedUsersByFilter.Add(session.GetHabbo().Id, uint.Parse((Azure.GetUnixTimeStamp() + (300 * 60)).ToString()));

                        return false;
                    }
                    if (session.GetHabbo().BobbaFiltered == 4)
                    {
                        session.SendNotif("Now you can not talk for 5 minutes . This is because your exhibits inappropriate language in Habbo Hotel.");
                    }
                    else if (session.GetHabbo().BobbaFiltered == 5)
                    {
                        session.SendNotif("You risk a ban if you continue to scold it . This is your last warning");
                    }
                    else if (session.GetHabbo().BobbaFiltered >= 7)
                    {
                        session.GetHabbo().BobbaFiltered = 0;

                        int length = 3600;
                        Azure.GetGame().GetBanManager().BanUser(session, "Auto-system-ban", length, "ban.", false, false);
                    }
                }
            }

            if (Azure.MutedUsersByFilter.ContainsKey(session.GetHabbo().Id))
            {
                if (Azure.MutedUsersByFilter[session.GetHabbo().Id] < Azure.GetUnixTimeStamp())
                {
                    Azure.MutedUsersByFilter.Remove(session.GetHabbo().Id);
                }
                else
                {
                    DateTime now = DateTime.Now;
                    TimeSpan timeStillBanned = now - Azure.UnixToDateTime(Azure.MutedUsersByFilter[session.GetHabbo().Id]);

                    session.SendNotif("Damn! you can't talk for " + timeStillBanned.Minutes.ToString().Replace("-", "") + " minutes and " + timeStillBanned.Seconds.ToString().Replace("-", "") + " seconds.");
                    return false;
                }
            }

            return true;
        }

        internal static void InitSwearWord()
        {
            Word = new List<string>();
            Word.Clear();
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT `word` FROM wordfilter");
                var table = adapter.GetTable();
                if (table == null) return;

                foreach (DataRow row in table.Rows)
                {
                    Word.Add(row[0].ToString().ToLower());
                }
            }

            Out.WriteLine("Loaded " + Word.Count + " Bobba Filters", "Azure.Security.BobbaFilter");
            Console.WriteLine();
        }

        internal static bool CheckForBannedPhrases(string message)
        {
            message = message.ToLower();

            message = message.Replace(".", "");
            message = message.Replace(" ", "");
            message = message.Replace("-", "");
            message = message.Replace(",", "");

            foreach (string mWord in Word)
            {
                if (message.Contains(mWord.ToLower()))
                    return true;
            }

            return false;
        }

        private static string ReplaceEx(string original,
                    string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.Ordinal)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }
    }
}