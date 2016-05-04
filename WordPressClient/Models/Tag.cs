using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Tag
    {
        public Tag() { }

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

        public static List<Tag> ListFromJson(String json)
        {
            List<Tag> tagList = new List<Tag>();
            JArray jarray = JArray.Parse(json);

            foreach (JObject jobject in jarray.Children<JObject>())
            {
                string jsonElement = JsonConvert.SerializeObject(jobject);
                tagList.Add(JsonConvert.DeserializeObject<Tag>(jsonElement));
            }
            return tagList;
        }

        public int id { get; set; }
        public int count { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string taxonomy { get; set; }
        public Links _links { get; set; }
    }
}
