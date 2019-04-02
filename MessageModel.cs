using Newtonsoft.Json.Linq;

namespace XiguaDanmakuHelper
{
    public enum MessageEnum
    {
        Like,
        Ad,
        Enter,
        Subscribe,
        Join,
        Gift,
        Gifting,
        Chat,
        Leave,
        Other
    }

    public class MessageModel
    {
        public MessageModel(MessageEnum type, Gift gift)
        {
            MsgType = type;
            GiftModel = gift;
        }

        public MessageModel(Chat chat)
        {
            MsgType = MessageEnum.Chat;
            ChatModel = chat;
        }

        public MessageModel(MessageEnum type, User user)
        {
            MsgType = type;
            UserModel = user;
        }

        public MessageModel(MessageEnum type)
        {
            MsgType = type;
        }

        public MessageModel(MessageEnum type, JObject j)
        {
            MsgType = type;
            Others = j;
        }

        public MessageEnum MsgType { get; set; }
        public Gift GiftModel { get; set; }
        public Chat ChatModel { get; set; }
        public User UserModel { get; set; }
        public JObject Others { get; set; }
    }
}