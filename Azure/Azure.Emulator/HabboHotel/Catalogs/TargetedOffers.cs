#region

using System.Data;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Catalogs
{
    internal class TargetedOfferManager
    {
        internal TargetedOffer CurrentOffer;

        public TargetedOfferManager()
        {
            LoadOffer();
        }

        public void LoadOffer()
        {
            CurrentOffer = null;
            DataRow row;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM catalog_targetedoffers WHERE enabled = '1' LIMIT 1");
                row = queryReactor.GetRow();
                if (row == null) return;
                CurrentOffer = new TargetedOffer((int)row["id"], (string)row["identifier"], (int)row["cost_credits"], (int)row["cost_duckets"], (int)row["cost_diamonds"], (int)row["purchase_limit"], (int)row["expiration_time"], (string)row["title"], (string)row["description"], (string)row["image"], (string)row["products"]);
            }
        }
    }

    internal class TargetedOffer
    {
        internal int Id;
        internal string Identifier;
        internal int CostCredits, CostDuckets, CostDiamonds;
        internal int PurchaseLimit;
        internal int ExpirationTime;
        internal string Title, Description, Image;
        internal string[] Products;

        public TargetedOffer(int id, string identifier, int costCredits, int costDuckets, int costDiamonds, int purchaseLimit, int expirationTime, string title, string description, string image, string products)
        {
            Id = id;
            Identifier = identifier;
            CostCredits = costCredits;
            CostDuckets = costDuckets;
            CostDiamonds = costDiamonds;
            PurchaseLimit = purchaseLimit;
            ExpirationTime = expirationTime;
            Title = title;
            Description = description;
            Image = image;
            Products = products.Split(';');
        }

        internal void GenerateMessage(ServerMessage message)
        {
            message.Init(LibraryParser.OutgoingRequest("TargetedOfferMessageComposer"));
            message.AppendInteger(1);//show
            message.AppendInteger(Id);
            message.AppendString(Identifier);
            message.AppendString(Identifier);
            message.AppendInteger(CostCredits);
            if (CostDiamonds > 0)
            {
                message.AppendInteger(CostDiamonds);
                message.AppendInteger(105);
            }
            else
            {
                message.AppendInteger(CostDuckets);
                message.AppendInteger(0);
            }
            message.AppendInteger(PurchaseLimit);
            var TimeLeft = ExpirationTime - Azure.GetUnixTimeStamp();
            message.AppendInteger(TimeLeft);
            message.AppendString(Title);
            message.AppendString(Description);
            message.AppendString(Image);
            message.AppendString("");
            message.StartArray();
            foreach (string Product in Products)
            {
                message.AppendString(Product);
                message.SaveArray();
            }
            message.EndArray();
        }
    }
}