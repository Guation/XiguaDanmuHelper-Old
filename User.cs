using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace XiguaDanmakuHelper
{
    public struct User
    {
        public static bool showBrand = false;
        private readonly long ID;
        private readonly string Name;
        private readonly string brand;
        private readonly int level;
        private readonly int type;

        public User(JObject j)
        {
            ID = 0;
            Name = "";
            brand = "";
            level = 0;
            type = 0;
            if (j["anchor"] != null && j["anchor"].Any() && j["anchor"]["user_info"].Any())
            {
                ID = (long) j["anchor"]["user_info"]["user_id"];
                Name = (string) j["anchor"]["user_info"]["name"];
            }
            else if (j["room"] != null && j["room"].Any() && j["room"]["user_info"].Any())
            {
                ID = (long) j["room"]["user_info"]["user_id"];
                Name = (string) j["room"]["user_info"]["name"];
            }
            else if (j["extra"] != null && j["extra"].Any())
            {
                if (j["extra"]["user"] != null && j["extra"]["user"].Any())
                {
                    ID = (long) j["extra"]["user"]["user_id"];
                    Name = (string) j["extra"]["user"]["name"];
                }
                if (j["extra"]["im_discipulus_info"] != null && j["extra"]["im_discipulus_info"].Any())
                {
                    level = (int) j["extra"]["im_discipulus_info"]["level"];
                    brand = (string) j["extra"]["im_discipulus_info"]["discipulus_group_title"];
                }
                if (j["extra"]["user_room_auth_status"] != null && j["extra"]["user_room_auth_status"].Any())
                {
                    type = (int) j["extra"]["user_room_auth_status"]["user_type"];
                }
            }
        }

        public bool isImportant()
        {
            if (level > 6)
            {
                return true;
            }
            else if (type > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public override string ToString()
        {
            if (showBrand)
            {
                if (level == 0)
                {
                    switch (type)
                    {
                        case 1:
                            return $"[房管]{Name}";
                        case 2:
                            return $"[主播]{Name}";
                        default:
                            return $"{Name}";
                    }
                }

                return type != 0 ? $"[{brand}{level}]{Name}" : $"<{brand}{level}>{Name}";
            }

            return $"{Name}";
        }
    }
}