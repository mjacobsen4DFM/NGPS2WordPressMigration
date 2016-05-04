using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class GuestAuthor
    {
        public GuestAuthor() { }

        public static GuestAuthor FromJson(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            GuestAuthor guestAuthor = jsonSerializer.Deserialize<GuestAuthor>(json);
            return guestAuthor;
        }

        public string ToJSON()
        {
            return (new JavaScriptSerializer().Serialize(this));
        }

        public string ToXML()
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stringwriter, this);
            return stringwriter.ToString();
        }

        public static List<GuestAuthor> ListFromJson(String json)
        {
            List<GuestAuthor> postList = new List<GuestAuthor>();
            JArray jarray = JArray.Parse(json);

            foreach (JObject jobject in jarray.Children<JObject>())
            {
                string jsonElement = JsonConvert.SerializeObject(jobject);
                postList.Add(JsonConvert.DeserializeObject<GuestAuthor>(jsonElement));
            }
            return postList;
        }
        public int id { get; set; }
        public string display_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string user_login { get; set; }
        public string user_email { get; set; }
        public string linked_account { get; set; }
        public string website { get; set; }
        public string aim { get; set; }
        public string yahooim { get; set; }
        public string jabber { get; set; }
        public string description { get; set; }
        public Links _links { get; set; }
    }
}
