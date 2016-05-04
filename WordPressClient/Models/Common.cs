using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.DFM.FeedHub.WordPressClient.Models
{
    public class Self
    {
        public string href { get; set; }
    }

    public class Collection
    {
        public string href { get; set; }
    }

    public class About
    {
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class Author
    {
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class Reply
    {
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class VersionHistory
    {
        public string href { get; set; }
    }

    public class HttpsApiWOrgAttachment
    {
        public string href { get; set; }
    }

    public class HttpsApiWOrgTerm
    {
        public string taxonomy { get; set; }
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class HttpsApiWOrgMeta
    {
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class HttpsApiWOrgFeaturedmedia
    {
        public bool embeddable { get; set; }
        public string href { get; set; }
    }

    public class Links
    {
        public List<Self> self { get; set; }
        public List<Collection> collection { get; set; }
        public List<About> about { get; set; }
        public List<Author> author { get; set; }
        public List<Reply> replies { get; set; }
        public List<VersionHistory> versionhistory { get; set; }
        //public List<HttpsApiWOrgAttachment> HttpsApiWOrgAttachment { get; set; }
        //public List<HttpsApiWOrgTerm> HttpsApiWOrgTerm { get; set; }
        //public List<HttpsApiWOrgMeta> HttpsApiWOrgMeta { get; set; }
        //public List<HttpsApiWOrgFeaturedmedia> HttpsApiWOrgFeaturedmedia { get; set; }
    }
}
