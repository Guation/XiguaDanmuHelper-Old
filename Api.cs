using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace XiguaDanmakuHelper
{
    public class Api
    {
        public delegate void Log(string msg);

        public delegate void RoomCounting(long popularity);

        public delegate void WhenMessage(MessageModel m);
        
        public delegate void WhenLeave();

//        public delegate void WhenLotteryFinished();
        private long _roomPopularity;
        protected string cursor = "0";
        public bool isLive = false;
        public bool isValidRoom = false;
        public string RoomID = "0";
        private string CharID = "0";
        public string Title = "";
        public User user;
        private int _updRoomCount = 0;
        private string liverName;
/*
        public Api()
        {
            liverName = "sy挂神";
        }

        public Api(string name)
        {
            liverName = name;
        }
*/
        public Api()
        {
            RoomID = "162474";
        }
        public Api(string id)
        {
            RoomID = id;
        }


        public static event WhenMessage OnMessage;
        public static event RoomCounting OnRoomCounting;
        public static event Log LogMessage;
        public static event WhenLeave OnLeave;
//        public static event WhenLotteryFinished OnLotteryFinished;

        public async Task<bool> ConnectAsync()
        {
            //await UpdateRoomInfoAsync();
            //await UpdateRoomAsync();
            UpdateRoom();
            if (!isValidRoom)
            {
                LogMessage?.Invoke("请确认输入的房间号是否正确");
                return false;
            }

            if (!isLive)
            {
                LogMessage?.Invoke("主播未开播");
                return false;
            }
            LogMessage?.Invoke("连接成功");
            return true;
        }

        public void _updateRoomInfo(JObject j)
        {
            if (j["extra"]?["member_count"] != null) _roomPopularity = (long) j["extra"]["member_count"];
            if (j["data"]?["popularity"] != null) _roomPopularity = (long) j["data"]["popularity"];

            OnRoomCounting?.Invoke(_roomPopularity);
        }

        public async Task<bool> UpdateRoomAsync() {
            isLive = true;
            var url = $"https://live.ixigua.com/{RoomID}";
            string _text;
            try
            {
                _text = await Common.HttpGetAsync(url);
            }
            catch (WebException)
            {
                LogMessage?.Invoke("网络错误");
                return false;
            }
            //LogMessage?.Invoke(_text);
            /*
            var j = JObject.Parse(_text);
            if (!(j["data"] is null))
            {
                isValidRoom = true;
                CharID = (string)j["data"]["id"];
            }*/
            return true;
        }

        public bool UpdateRoom()
        {
            isLive = true;
            var url = $"https://live.ixigua.com/{RoomID}";
            string _text;
            try
            {
                _text = Common.HttpGet(url,true);
            }
            catch (WebException)
            {
                LogMessage?.Invoke("网络错误");
                return false;
            }
            //LogMessage?.Invoke(_text);
            try
            {
                string[] data1 = _text.Split(new String[] { "id=\"SSR_HYDRATED_DATA\">" }, StringSplitOptions.RemoveEmptyEntries);
                string[] data2 = data1[1].Split(new String[] { "</script>" }, StringSplitOptions.RemoveEmptyEntries);
                //LogMessage?.Invoke(data2[0]);
                var j = JObject.Parse(data2[0]);
                if (!(j["roomData"] is null))
                {
                    isValidRoom = true;
                    CharID = (string)j["roomData"]["id"];
                }
            }
            catch (Exception) {
                LogMessage?.Invoke("出现故障");
            } 
            return true;
        }


        public async Task<bool> UpdateRoomInfoAsync()
        {
            if (isLive)
            {
                var url = $"https://i.snssdk.com/videolive/room/enter?version_code=730&device_platform=android";
                var data = $"room_id={RoomID}&version_code=730&device_platform=android";
                string _text;
                try
                {
                    _text = await Common.HttpPostAsync(url, data);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (j["room"] is null)
                {
                    LogMessage?.Invoke("无法获取Room信息，请与我联系");
                    return false;
                }

                Title = (string)j["room"]["title"];
                user = new User(j);
                if (isLive && (int)j["room"]?["status"] != 2)
                {
                    OnLeave?.Invoke();
                }
                isLive = (int)j["room"]?["status"] == 2;
                return true;
            }
            else
            {
                var url = $"https://security.snssdk.com/video/app/search/live/?version_code=730&device_platform=android&format=json&keyword={liverName}";
                string _text;
                try
                {
                    _text = await Common.HttpGetAsync(url);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (!(j["data"] is null))
                {
                    foreach (var _j in j["data"])
                    {
                        if ((int) _j["block_type"] != 0)
                        {
                            continue;
                        }

                        if (_j["cells"].Any())
                        {
                            if (new User((JObject)_j["cells"][0]).ToString() == liverName)
                            {
                                isValidRoom = true;
                                isLive = (bool)_j["cells"][0]["anchor"]["user_info"]["is_living"];
                                RoomID = (string)_j["cells"][0]["anchor"]["room_id"];
                                liverName = new User((JObject)_j["cells"][0]).ToString();
                                user = new User((JObject)_j["cells"][0]);
                            }
                            else {
                                return await UpdateRoomInfoAsync();
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (isLive)
                    {
                        return await UpdateRoomInfoAsync();
                    }
                }
                return false;
            }
        }

        public bool UpdateRoomInfo()
        {
            if (isLive)
            {
                var url = $"https://i.snssdk.com/videolive/room/enter?version_code=730&device_platform=android";
                var data = $"room_id={CharID}&version_code=730&device_platform=android";
                string _text;
                try
                {
                    _text = Common.HttpPost(url, data);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (j["room"] is null)
                {
                    LogMessage?.Invoke("无法获取Room信息，请与我联系");
                    return false;
                }

                isValidRoom = (int)j["base_resp"]?["status_code"] == 0;
                Title = (string) j["data"]["title"];
                RoomID = (string) j["data"]["id"];
                user = new User(j);

                isLive = (int) j["room"]?["status"] == 2;
                return true;
            }
            else
            {
                var url = $"https://security.snssdk.com/video/app/search/live/?version_code=730&device_platform=android&format=json&keyword={liverName}";
                string _text;
                try
                {
                    _text = Common.HttpGet(url);
                }
                catch (WebException)
                {
                    LogMessage?.Invoke("网络错误");
                    return false;
                }
                var j = JObject.Parse(_text);
                if (!(j["data"] is null))
                {
                    foreach (var _j in j["data"])
                    {
                        if ((int) _j["block_type"] != 0)
                        {
                            continue;
                        }

                        if (_j["cells"].Any())
                        {
                            isValidRoom = true;
                            isLive = (bool) _j["cells"][0]["anchor"]["user_info"]["is_living"];
                            RoomID = (string)_j["cells"][0]["anchor"]["room_id"];
                            liverName = (new User((JObject)_j["cells"][0])).ToString();
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (isLive)
                    {
                        return UpdateRoomInfo();
                    }
                }
                return false;
            }
        }

        public void GetDanmaku()
        {
            if (!isValidRoom)
            {
                //UpdateRoomInfo();
                UpdateRoom();
                return;
            }

            var url =
                $"https://i.snssdk.com/videolive/im/get_msg?cursor={cursor}&room_id={CharID}&version_code=730&device_platform=android";
            string _text;
            try
            {
                _text = Common.HttpGet(url);
            }
            catch (WebException)
            {
                LogMessage?.Invoke("网络错误");
                return;
            }

            var j = JObject.Parse(_text);
            if (j["extra"]?["cursor"] is null)
            {
                LogMessage?.Invoke("cursor 数据结构改变，请与我联系");
                Console.Read();
                return;
            }

            cursor = (string) j["extra"]["cursor"];
            if (j["data"] is null)
            {
                //UpdateRoomInfo();
                UpdateRoom();
                return;
            }

            foreach (var m in j["data"])
            {
                if (m?["common"]?["method"] is null) continue;
                switch ((string) m["common"]["method"])
                {
                    case "VideoLivePresentMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Gifting, new Gift((JObject) m)));
                        break;
                    case "VideoLivePresentEndTipMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Gift, new Gift((JObject) m)));
                        break;
                    case "VideoLiveRoomAdMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Ad, (JObject) m));
                        break;
                    case "VideoLiveChatMessage":
                        OnMessage?.Invoke(new MessageModel(new Chat((JObject) m)));
                        break;
                    case "VideoLiveMemberMessage":
                        _updateRoomInfo((JObject) m);
//                        OnEnter?.Invoke(new Gift((JObject)m));
//                        OnMessage?.Invoke(new MessageModel(new Chat((JObject)m)));
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Enter, (JObject) m));
                        break;
                    case "VideoLiveSocialMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Subscribe, new User((JObject) m)));
                        break;
                    case "VideoLiveJoinDiscipulusMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Join, new User((JObject) m)));
                        break;
                    case "VideoLiveControlMessage":
                        UpdateRoomInfo();
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Leave));
                        break;
                    case "VideoLiveDiggMessage":
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Like, new User((JObject) m)));
                        break;
                    default:
                        OnMessage?.Invoke(new MessageModel(MessageEnum.Other, (JObject) m));
                        break;
                }
            }
        }
    }
}