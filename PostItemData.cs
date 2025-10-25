using Projects.Scripts.Enums;

namespace Projects.Scripts.View.PostBox
{
    public class PostItemData
    {
        public int Idx { get; set; }
        public UseType ItemType { get; set; }
        public int Qty { get; set; }
        public bool Consume { get; set; }
    }
}