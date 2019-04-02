using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace XiguaDanmakuHelper
{
    public struct Gift
    {
        public User user;
        private readonly long ID;
        public static long RoomID = 0;
        public long count;
        public static Dictionary<long, string> GiftList = new Dictionary<long, string>();

        public Gift(JObject j)
        {
            ID = 0;
            count = 0;
            user = new User(j);
            if (j["common"]?["room_id"] != null)
            {
                RoomID = (long) j["common"]["room_id"];
                UpdateGiftList();
            }
            if (j["extra"]?["present_end_info"] != null && j["extra"]["present_end_info"].Any())
            {
                ID = (long) j["extra"]["present_end_info"]["id"];
                count = (long) j["extra"]["present_end_info"]["count"];
            }
            else if (j["extra"]?["present_info"] != null && j["extra"]["present_info"].Any())
            {
                ID = (long) j["extra"]["present_info"]["id"];
                count = (long) j["extra"]["present_info"]["repeat_count"];
            }
            if (ID != 0 && !GiftList.ContainsKey(ID))
            {
                UpdateGiftList();
            }
        }

        private void UpdateGiftList()
        {
            GiftList = new Dictionary<long, string>();
            GiftList.Add(10001, "西瓜");
            var _text = Common.HttpGet($"https://i.snssdk.com/videolive/gift/get_gift_list?room_id={RoomID}&version_code=730&device_platform=android");
            var j = JObject.Parse(_text);
            if (j["gift_info"].Any())
                foreach (var g in j["gift_info"])
                    if (GiftList.ContainsKey((long) g["id"]))
                    {
                        GiftList[(long) g["id"]] = (string) g["name"];
                    }
                    else
                    {
                        GiftList.Add((long) g["id"], (string) g["name"]);
                    }
        }

        public override string ToString()
        {
            return $"感谢 {user} 送出的 {count} 个 {GetName()}";
        }

        public string GetName()
        {
            string GiftN;
            if (GiftList.ContainsKey(ID))
                GiftN = GiftList[ID];
            else
                GiftN = $"未知礼物{ID}";

            return GiftN;
        }

        public static async void UpdateGiftListAsync(long roomId)
        {
            GiftList = new Dictionary<long, string>();
            var _text = await Common.HttpGetAsync($"https://i.snssdk.com/videolive/gift/get_gift_list?room_id={roomId}");
            var j = JObject.Parse(_text);
            if (j["gift_info"] != null)
                foreach (var g in j["gift_info"])
                    if (GiftList.ContainsKey((long) g["id"]))
                    {
                        GiftList[(long) g["id"]] = (string) g["name"];
                    }
                    else
                    {
                        GiftList.Add((long) g["id"], (string) g["name"]);
                    }
        }
    }
}