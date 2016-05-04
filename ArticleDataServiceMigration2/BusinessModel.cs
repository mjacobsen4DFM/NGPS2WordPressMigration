using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataModels;

namespace ArticleDataServiceMigration2
{
    class BusinessModel
    {
       public enum asset
        {
            article = 102,
            image = 108,
            freeform = 111,
            pdf = 104
        }

       static public List<IMAGE> GetImageItems(string uid)
       {
           using (livee_11Entities database = new livee_11Entities())
           {
               var ImageItems = from IMAGE in database.IMAGEs
                                         where IMAGE.CONTENT_ITEM_UID.Equals(uid)
                                         select IMAGE;

               return ImageItems.ToList();
           }


       }

       static public List<CONTENT_ITEM_REL> GetRelatedContentitems(string uid)
        {
            var  ContentItems = new List<CONTENT_ITEM_REL>();
            using (livee_11Entities database = new livee_11Entities())
            {
                try
                {
                    var RelatedContentItems = from CONTENT_ITEM_REL in database.CONTENT_ITEM_REL
                                              where CONTENT_ITEM_REL.CONTENT_ITEM_UID.Equals(uid)
                                              select CONTENT_ITEM_REL;
                    return RelatedContentItems.ToList();

                }
                catch ( System.Exception e) {


                }
                return ContentItems.ToList();
            }
        }// ListRelatedContent

        static public List<CONTENT_ITEM> GetContentItems(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var ContentItems = from CONTENT_ITEM in database.CONTENT_ITEM
                                   where CONTENT_ITEM.CONTENT_ITEM_UID.Equals(uid) && (CONTENT_ITEM.STATE_UCODE.Equals("PRODDEPLOY") || CONTENT_ITEM.STATE_UCODE.Equals("EXPIRED"))
                                   select CONTENT_ITEM;

                return ContentItems.ToList();
            }
        }// ListRelatedContent



        static public List<CONTENT_ITEM> GetContentItemsAll(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var ContentItems = from CONTENT_ITEM in database.CONTENT_ITEM
                                   where CONTENT_ITEM.CONTENT_ITEM_UID.Equals(uid) 
                                   select CONTENT_ITEM;

                return ContentItems.ToList();
            }
        }// ListRelatedContent



        static public List<FREEFORM> GetFreeFormItems(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var FreeformItems = from FREEFORM in database.FREEFORMs
                                    where FREEFORM.CONTENT_ITEM_UID.Equals(uid)
                                    select FREEFORM;

                return FreeformItems.ToList();
            }
        }// ListRelatedContent

        static public List<PDF_VIEW> GetPdfItems(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var pdfItems = from PDF_VIEW in database.PDF_VIEW
                               where PDF_VIEW.CONTENT_ITEM_UID.Equals(uid)
                               select PDF_VIEW;

                return pdfItems.ToList();
            }
        }// ListPdfs


        static private List<CONTENT_ITEM> GetRelatedContentitems(string article_uid, int type_uid)
        {
            List<CONTENT_ITEM> ciList = new List<CONTENT_ITEM>();
            List<CONTENT_ITEM_REL> RelatedContentItems = GetRelatedContentitems(article_uid);
            foreach (CONTENT_ITEM_REL rci in RelatedContentItems)
            {
                string uid = rci.RELATED_CONTENT_ITEM_UID;
                List<CONTENT_ITEM> ContentItems = GetContentItems(uid);
                foreach (CONTENT_ITEM ci in ContentItems)
                {
                    if (ci.CONTENT_TYPE_UID.Equals(type_uid))
                    {
                        ciList.Add(ci);
                    } // end if
                }

            }
            return ciList.ToList();

        } //  GetRelatedContentitems



        static public List<FREEFORM> GetFreeforms(string article_uid)
        {
            List<FREEFORM> ffList = new List<FREEFORM>();
            List<CONTENT_ITEM> ContentItems = GetRelatedContentitems(article_uid, (int)asset.freeform);
            foreach (CONTENT_ITEM ci in ContentItems)
            {
                string uid = ci.CONTENT_ITEM_UID;
                List<FREEFORM> forms = GetFreeFormItems(uid);
                foreach (FREEFORM form in forms)
                {
                    form.FREEFORM_HTML = (form.FREEFORM_HTML == null) ? "" : form.FREEFORM_HTML;
                    ffList.Add(form);
                }
            }// end for each
            return ffList.ToList();
        } // GetFreeforms




        static public List<PDF_VIEW> GetPdf(string article_uid)
        {
            List<PDF_VIEW> pdfList = new List<PDF_VIEW>();
            List<CONTENT_ITEM> ContentItems = GetRelatedContentitems(article_uid, (int)asset.pdf);
            foreach (CONTENT_ITEM ci in ContentItems)
            {
                string uid = ci.CONTENT_ITEM_UID;
                List<PDF_VIEW> pdfs = GetPdfItems(uid);
                foreach (PDF_VIEW pdf in pdfs)
                {
                    pdf.CAPTION = (pdf.CAPTION == null) ? "" : pdf.CAPTION;
                    pdf.BINARY_URL = (pdf.BINARY_URL == null) ? "" : pdf.BINARY_URL;
                    pdfList.Add(pdf);
                }
            }// end for each
            return pdfList.ToList();
        } // GetFreeforms




        static public List<CONTENT_ITEM> 
            
            GetContentItems(string siteid, int content_type, DateTime startdate, DateTime enddate)
        {

            var ContentItems = new List<CONTENT_ITEM>();

            try
            {
            using (livee_11Entities database = new livee_11Entities())
            {
                database.Connection.Open();
                database.CommandTimeout = 180;
                var InnerContentItems = from CONTENT_ITEM in database.CONTENT_ITEM
                                   where CONTENT_ITEM.SITE_UID.Equals(siteid) && CONTENT_ITEM.CONTENT_TYPE_UID.Equals(content_type) && 
                                            CONTENT_ITEM.FIRST_PUBLICATION_TIMESTAMP >= startdate && CONTENT_ITEM.FIRST_PUBLICATION_TIMESTAMP <= enddate && 
                                            (CONTENT_ITEM.STATE_UCODE.Equals("PRODDEPLOY") || CONTENT_ITEM.STATE_UCODE.Equals("EXPIRED"))
                                        orderby CONTENT_ITEM.START_DATE ascending
                                   select CONTENT_ITEM;

                return InnerContentItems.ToList();
            }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error: {0} ", e.Message);
            }
            return ContentItems.ToList();
        }


        static public List<CONTENT_ITEM_CONTENT_GROUP> GetContentItemContentGroups(string uid, string siteid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var ContentItemContentGroups = from CONTENT_ITEM_CONTENT_GROUP in database.CONTENT_ITEM_CONTENT_GROUP
                                              where CONTENT_ITEM_CONTENT_GROUP.CONTENT_ITEM_UID.Equals(uid) && CONTENT_ITEM_CONTENT_GROUP.SITE_UID.Equals(siteid)
                                              select CONTENT_ITEM_CONTENT_GROUP;

                return ContentItemContentGroups.ToList();
            }


        } // GetContentItemContentGroup


        static public List<CONTENT_GROUP> GetContentGroup(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var ContentGroup = from CONTENT_GROUP in database.CONTENT_GROUP
                                   where CONTENT_GROUP.CONTENT_GROUP_UID.Equals(uid)
                                   select CONTENT_GROUP;

                return ContentGroup.ToList();
            }


        } // GetContentItemContentGroup


        static public List<ARTICLE> GetArticle(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var articles = from ART in database.ARTICLEs
                               where ART.CONTENT_ITEM_UID.Equals(uid)
                               select ART;
                return articles.ToList();
            }
        } // GetArticle

        static public List<SECTION_ITEM> GetSection(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var sections = from SEC in database.SECTION_ITEM
                               where SEC.SECTION_ITEM_UID.Equals(uid)
                               select SEC;
                return sections.ToList();
            }
        } // GetSection

        static public List<SITE_URL> GetSiteURL(string uid)
        {
            using (livee_11Entities database = new livee_11Entities())
            {
                var urls = from su in database.SITE_URL
                               where su.SITE_UID.Equals(uid) && su.SITE_URL_TYPE_UCODE.Equals("PRODUCTION")
                               select su;
                return urls.ToList();
            }
        } // GetSiteURL


        public class CONTENT
        {

            public string uid { get; set; } 
        }


        static  public List<CONTENT>  GetRelatedArticle(string uid, string siteid )
        {
            DateTime dt = DateTime.Now; 
            List<CONTENT> contentitems = new List<CONTENT>();
            using (livee_11Entities database = new livee_11Entities())
            {
                var query = from CONTENT_GROUP in database.CONTENT_GROUP
                            join CONTENT_ITEM_CONTENT_GROUP in database.CONTENT_ITEM_CONTENT_GROUP on CONTENT_GROUP.CONTENT_GROUP_UID equals CONTENT_ITEM_CONTENT_GROUP.CONTENT_GROUP_UID
                            where CONTENT_ITEM_CONTENT_GROUP.CONTENT_ITEM_UID.Equals(uid) && CONTENT_ITEM_CONTENT_GROUP.SITE_UID.Equals(siteid) && CONTENT_GROUP.CONTENT_GROUP_TYPE_UCODE.Equals("PACKAGE")
                            select new { CONTENT_ITEM_CONTENT_GROUP.CONTENT_GROUP_UID, CONTENT_GROUP.GROUP_NAME };

                foreach (var v in query)
                {

                    string groupuid = v.CONTENT_GROUP_UID;
                    var query2 = from CONTENT_ITEM in database.CONTENT_ITEM
                                 join CONTENT_ITEM_CONTENT_GROUP in database.CONTENT_ITEM_CONTENT_GROUP on CONTENT_ITEM.CONTENT_ITEM_UID equals CONTENT_ITEM_CONTENT_GROUP.CONTENT_ITEM_UID
                                 where CONTENT_ITEM_CONTENT_GROUP.CONTENT_GROUP_UID.Equals(groupuid) && CONTENT_ITEM_CONTENT_GROUP.SITE_UID.Equals(siteid) && (CONTENT_ITEM.STATE_UCODE.Equals("PRODDEPLOY") || CONTENT_ITEM.STATE_UCODE.Equals("EXPIRED"))
                                 && (CONTENT_ITEM.CONTENT_TYPE_UID.Equals(102) || CONTENT_ITEM.CONTENT_TYPE_UID.Equals(127)) && CONTENT_ITEM_CONTENT_GROUP.DISPLAY_START_DATE < dt
                                 select new { CONTENT_ITEM.CONTENT_ITEM_UID };

                    foreach (var vv in query2)
                    {
                        CONTENT c = new CONTENT();
                        c.uid = vv.CONTENT_ITEM_UID;
                        contentitems.Add(c);

                    }

                }

            }
            return contentitems.Take(10).ToList();
        } // GetRelatedArticle



    }
}
