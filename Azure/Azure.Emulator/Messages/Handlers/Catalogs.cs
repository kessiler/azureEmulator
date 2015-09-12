#region

using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Catalogs;
using Azure.HabboHotel.Groups.Structs;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Catalogues the index.
        /// </summary>
        public void CatalogueIndex()
        {
            var rank = Session.GetHabbo().Rank;
            if (rank < 1)
                rank = 1;
            Session.SendMessage(StaticMessage.CatalogOffersConfiguration);
            Session.SendMessage(CatalogPacket.ComposeIndex(rank, Request.GetString().ToUpper()));
        }

        /// <summary>
        /// Catalogues the page.
        /// </summary>
        public void CataloguePage()
        {
            var pageId = Request.GetInteger();
            int Num = Request.GetInteger();
            var cPage = Azure.GetGame().GetCatalog().GetPage(pageId);
            if (cPage == null || !cPage.Enabled || !cPage.Visible || cPage.MinRank > Session.GetHabbo().Rank)
                return;
            Session.SendMessage(cPage.CachedContentsMessage);
        }

        /// <summary>
        /// Catalogues the club page.
        /// </summary>
        public void CatalogueClubPage()
        {
            var requestType = Request.GetInteger();
            Session.SendMessage(CatalogPacket.ComposeClubPurchasePage(Session, requestType));
        }

        /// <summary>
        /// Reloads the ecotron.
        /// </summary>
        public void ReloadEcotron()
        {
            Response.Init(LibraryParser.OutgoingRequest("ReloadEcotronMessageComposer"));
            Response.AppendInteger(1);
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Gifts the wrapping configuration.
        /// </summary>
        public void GiftWrappingConfig()
        {
            Response.Init(LibraryParser.OutgoingRequest("GiftWrappingConfigurationMessageComposer"));
            Response.AppendBool(true); //enabled
            Response.AppendInteger(1); //cost
            Response.AppendInteger(GiftWrappers.GiftWrappersList.Count);
            foreach (var i in GiftWrappers.GiftWrappersList)
                Response.AppendInteger(i);

            Response.AppendInteger(8);
            for(var i = 0u; i != 8; i++)
                Response.AppendInteger(i);

            Response.AppendInteger(11);
            for (var i = 0u; i != 11; i++)
                Response.AppendInteger(i);

            Response.AppendInteger(GiftWrappers.OldGiftWrappers.Count);
            foreach (var i in GiftWrappers.OldGiftWrappers)
                Response.AppendInteger(i);
            SendResponse();
        }

        /// <summary>
        /// Gets the recycler rewards.
        /// </summary>
        public void GetRecyclerRewards()
        {
            Response.Init(LibraryParser.OutgoingRequest("RecyclerRewardsMessageComposer"));
            var ecotronRewardsLevels = Azure.GetGame().GetCatalog().GetEcotronRewardsLevels();
            Response.AppendInteger(ecotronRewardsLevels.Count);
            foreach (var current in ecotronRewardsLevels)
            {
                Response.AppendInteger(current);
                Response.AppendInteger(current);
                var ecotronRewardsForLevel = Azure.GetGame().GetCatalog().GetEcotronRewardsForLevel(uint.Parse(current.ToString()));
                Response.AppendInteger(ecotronRewardsForLevel.Count);
                foreach (var current2 in ecotronRewardsForLevel)
                {
                    Response.AppendString(current2.GetBaseItem().PublicName);
                    Response.AppendInteger(1);
                    Response.AppendString(current2.GetBaseItem().Type.ToString());
                    Response.AppendInteger(current2.GetBaseItem().SpriteId);
                }
            }
            SendResponse();
        }

        /// <summary>
        /// Purchases the item.
        /// </summary>
        public void PurchaseItem()
        {
            if (Session == null || Session.GetHabbo() == null) return;
            if (Session.GetHabbo().GetInventoryComponent().TotalItems >= 2799)
            {
                Session.SendMessage(CatalogPacket.PurchaseOk(0, string.Empty, 0));
                Session.SendMessage(StaticMessage.AdvicePurchaseMaxItems);
                return;
            }
            int pageId = Request.GetInteger();
            uint itemId = Request.GetUInteger();
            string extraData = Request.GetString();
            int priceAmount = Request.GetInteger();
            Azure.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, priceAmount, false, "", "", 0, 0, 0, false, 0u);
        }

        /// <summary>
        /// Purchases the gift.
        /// </summary>
        public void PurchaseGift()
        {
            int pageId = Request.GetInteger();
            uint itemId = Request.GetUInteger();
            string extraData = Request.GetString();
            string giftUser = Request.GetString();
            string giftMessage = Request.GetString();
            int giftSpriteId = Request.GetInteger();
            int giftLazo = Request.GetInteger();
            int giftColor = Request.GetInteger();
            var undef = Request.GetBool();
            Azure.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, 1, true, giftUser, giftMessage, giftSpriteId, giftLazo, giftColor, undef, 0u);
        }

        /// <summary>
        /// Checks the name of the pet.
        /// </summary>
        public void CheckPetName()
        {
            var petName = Request.GetString();
            var i = 0;
            if (petName.Length > 15)
                i = 1;
            else if (petName.Length < 3)
                i = 2;
            else if (!Azure.IsValidAlphaNumeric(petName))
                i = 3;
            Response.Init(LibraryParser.OutgoingRequest("CheckPetNameMessageComposer"));
            Response.AppendInteger(i);
            Response.AppendString(petName);
            SendResponse();
        }

        /// <summary>
        /// Catalogues the offer.
        /// </summary>
        public void CatalogueOffer()
        {
            var num = Request.GetInteger();
            var catalogItem = Azure.GetGame().GetCatalog().GetItemFromOffer(num);
            if (catalogItem == null || Catalog.LastSentOffer == num)
                return;
            Catalog.LastSentOffer = num;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("CatalogOfferMessageComposer"));
            CatalogPacket.ComposeItem(catalogItem, message);
            Session.SendMessage(message);
        }

        /// <summary>
        /// Catalogues the offer configuration.
        /// </summary>
        public void CatalogueOfferConfig()
        {
            Response.Init(LibraryParser.OutgoingRequest("CatalogueOfferConfigMessageComposer"));
            Response.AppendInteger(100);
            Response.AppendInteger(6);
            Response.AppendInteger(1);
            Response.AppendInteger(1);
            Response.AppendInteger(2);
            Response.AppendInteger(40);
            Response.AppendInteger(99);
            SendResponse();
        }

        /// <summary>
        /// Serializes the group furni page.
        /// </summary>
        internal void SerializeGroupFurniPage()
        {
            var userGroups = Azure.GetGame().GetGroupManager().GetUserGroups(Session.GetHabbo().Id);
            Response.Init(LibraryParser.OutgoingRequest("GroupFurniturePageMessageComposer"));

            var responseList = new List<ServerMessage>();
            foreach (var HabboGroup in userGroups.Where(current => current != null).Select(current => Azure.GetGame().GetGroupManager().GetGroup(current.GroupId)))
            {
                if (HabboGroup == null)
                    continue;

                ServerMessage subResponse = new ServerMessage();
                subResponse.AppendInteger(HabboGroup.Id);
                subResponse.AppendString(HabboGroup.Name);
                subResponse.AppendString(HabboGroup.Badge);
                subResponse.AppendString(Azure.GetGame().GetGroupManager().SymbolColours.Contains(HabboGroup.Colour1)
                    ? ((GroupSymbolColours)
                    Azure.GetGame().GetGroupManager().SymbolColours[HabboGroup.Colour1]).Colour
                    : "4f8a00");
                subResponse.AppendString(
                    Azure.GetGame().GetGroupManager().BackGroundColours.Contains(HabboGroup.Colour2)
                    ? ((GroupBackGroundColours)
                    Azure.GetGame().GetGroupManager().BackGroundColours[HabboGroup.Colour2]).Colour
                    : "4f8a00");
                subResponse.AppendBool(HabboGroup.CreatorId == Session.GetHabbo().Id);
                subResponse.AppendInteger(HabboGroup.CreatorId);
                subResponse.AppendBool(HabboGroup.HasForum);

                responseList.Add(subResponse);
            }

            Response.AppendInteger(responseList.Count());
            Response.AppendServerMessages(responseList);

            responseList.Clear();
            responseList = null;

            SendResponse();
        }
    }
}