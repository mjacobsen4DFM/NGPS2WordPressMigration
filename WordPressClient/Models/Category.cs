using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Category
    {
        public Category() { }

        public static Category FromJson(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Category category = jsonSerializer.Deserialize<Category>(json);
            return category;
        }
        public static List<Category> ListFromJson(String json)
        {
            List<Category> lc = new List<Category>();
            JArray a = JArray.Parse(json);

            foreach (JObject o in a.Children<JObject>())
            {
                string j = JsonConvert.SerializeObject(o);
                lc.Add(JsonConvert.DeserializeObject<Category>(j));
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
