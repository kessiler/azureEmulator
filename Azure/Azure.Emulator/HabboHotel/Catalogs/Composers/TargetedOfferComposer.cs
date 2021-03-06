﻿using Azure.HabboHotel.Catalogs.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Catalogs.Composers
{
    internal class TargetedOfferComposer
    {
        internal static void GenerateMessage(ServerMessage message, TargetedOffer offer)
        {
            message.Init(LibraryParser.OutgoingRequest("TargetedOfferMessageComposer"));
            message.AppendInteger(1);
            message.AppendInteger(offer.Id);
            message.AppendString(offer.Identifier);
            message.AppendString(offer.Identifier);
            message.AppendInteger(offer.CostCredits);

            if (offer.CostDiamonds > 0)
            {
                message.AppendInteger(offer.CostDiamonds);
                message.AppendInteger(105);
            }
            else
            {
                message.AppendInteger(offer.CostDuckets);
                message.AppendInteger(0);
            }

            message.AppendInteger(offer.PurchaseLimit);

            var timeLeft = offer.ExpirationTime - Azure.GetUnixTimeStamp();

            message.AppendInteger(timeLeft);
            message.AppendString(offer.Title);
            message.AppendString(offer.Description);
            message.AppendString(offer.Image);
            message.AppendString(string.Empty);
            message.StartArray();

            foreach (var product in offer.Products)
            {
                message.AppendString(product);
                message.SaveArray();
            }

            message.EndArray();
        }
    }
}