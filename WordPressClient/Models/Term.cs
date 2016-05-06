using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Term
    {
        public Term() { }

        public static Term FromJson(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Term category = jsonSerializer.Deserialize<Term>(json);
            return category;
        }
        public static List<Term> ListFromJson(String json)
        {
            List<Term> lc = new List<Term>();
            JArray a = JArray.Parse(json);

            foreach (JObject o in a.Children<JObject>())
            {
                string j = JsonConvert.SerializeObject(o);
                lc.Add(JsonConvert.DeserializeObject<Term>(j));
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

        public Int16 id { get; set; }
        public Int32 count { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string taxonomy { get; set; }
        public Int16 parent { get; set; }
        public Links _links { get; set; }
    }
}
