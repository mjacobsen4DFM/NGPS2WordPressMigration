using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Post
    {
        public Post() { }
        
        public static Post FromJson(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Post post =  jsonSerializer.Deserialize<Post>(json);
            return post;
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

        public static List<Post> ListFromJson(String json)
        {
            List<Post> postList = new List<Post>();
            JArray jarray = JArray.Parse(json);

            foreach (JObject jobject in jarray.Children<JObject>())
            {
                string jsonElement = JsonConvert.SerializeObject(jobject);
                postList.Add(JsonConvert.DeserializeObject<Post>(jsonElement));
            }
            return postList;
        }

        public string id { get; set; }
        
        //public string date { get; set; }
        public string date_gmt { get; set; }
        
        //public string modified { get; set; }
        public string modified_gmt { get; set; }

        //public string slug { get; set; }
        public string type { get; set; } = "post";
        public string link { get; set; }
        public string title { get; set; }
        public string status { get; set; } = "publish";
        public string content { get; set; }
        public string excerpt { get; set; }
        public int author { get; set; }
        public int featured_media { get; set; }
        public string comment_status { get; set; } = "closed";
        public string ping_status { get; set; }
        public bool sticky { get; set; } = false;
        public string format { get; set; } = "standard";
        public List<int> categories { get; set; }
        public List<int> tags { get; set; }
        public List<int> location { get; set; }
    }

    public class PostBack
    {
        public PostBack() { }
        
        public static PostBack FromJson(String json)
        {
            string jsonPost = json.TrimStart('[').TrimEnd(']');
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize<PostBack>(jsonPost);
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

        public string id { get; set; }
        public string date { get; set; }
        public string date_gmt { get; set; }
        public Guid guid { get; set; }
        public string modified { get; set; }
        public string modified_gmt { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public Title title { get; set; }
        public Content content { get; set; }
        public Excerpt excerpt { get; set; }
        public int author { get; set; }
        public int featured_media { get; set; }
        public string comment_status { get; set; }
        public string ping_status { get; set; }
        public string sticky { get; set; }
        public string format { get; set; }
        public List<int> categories { get; set; }
        public List<int> tags { get; set; }
        public List<int> location { get; set; }
        public Links _links { get; set; }
    }

    public class Guid
    {
        public string rendered { get; set; }
    }

    public class Title
    {
        public string rendered { get; set; }
    }

    public class Content
    {
        public string rendered { get; set; }
    }

    public class Excerpt
    {
        public string rendered { get; set; }
    }
}
