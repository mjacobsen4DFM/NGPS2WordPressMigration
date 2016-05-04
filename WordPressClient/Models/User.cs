using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class User
    {
        public User() { }

        public static Tag FromJson(String json)
        {
            return JsonConvert.DeserializeObject<Tag>(json);
        }

        public string ToJSON()
        {
            return (JsonConvert.SerializeObject(this));
        }

        public string ToXML()
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stringwriter, this);
            return stringwriter.ToString();
        }

        public static List<User> ListFromJson(String json)
        {
            List<User> userList = new List<User>();
            JArray jarray = JArray.Parse(json);

            foreach (JObject jobject in jarray.Children<JObject>())
            {
                string jsonElement = JsonConvert.SerializeObject(jobject);
                userList.Add(JsonConvert.DeserializeObject<User>(jsonElement));
            }
            return userList;
        }
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string slug { get; set; }
        //public AvatarUrls avatar_urls { get; set; }
        public Links _links { get; set; }
    }
}
