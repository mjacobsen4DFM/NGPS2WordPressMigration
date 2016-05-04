using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Location
    {
        public Location() { }

        public static Location FromJson(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Location location = jsonSerializer.Deserialize<Location>(json);
            return location;
        }
        public static List<Location> ListFromJson(String json)
        {
            List<Location> lc = new List<Location>();
            JArray a = JArray.Parse(json);

            foreach (JObject o in a.Children<JObject>())
            {
                string j = JsonConvert.SerializeObject(o);
                lc.Add(JsonConvert.DeserializeObject<Location>(j));
            }
            return lc;
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

        public int id { get; set; }
        public int count { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string taxonomy { get; set; }
        public int parent { get; set; }
        public Links _links { get; set; }
    }
}
