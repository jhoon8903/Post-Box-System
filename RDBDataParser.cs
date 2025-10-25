using System;
using Firebase.Database;
using Projects.Scripts.Enums;

namespace Projects.Scripts.View.PostBox
{
    public static class RdbDataParser
    {
        public static PostItemData ParsePostData(DataSnapshot snapshot)
        {
            if (!int.TryParse(snapshot.Key, out int idx)) return null;
            if (!snapshot.Exists) return null;
            if (!int.TryParse(snapshot.Child("itemType").Value?.ToString(), out int type) ||
                !int.TryParse(snapshot.Child("qty").Value?.ToString(), out int qty) ||
                !bool.TryParse(snapshot.Child("consume").Value?.ToString(), out bool consume))
            {
                return null;
            }

            if (!Enum.IsDefined(typeof(UseType), type)) return null;

            return new PostItemData
            {
                Idx = idx,
                ItemType = (UseType)type,
                Qty = qty,
                Consume = consume
            };
        }
    }
}