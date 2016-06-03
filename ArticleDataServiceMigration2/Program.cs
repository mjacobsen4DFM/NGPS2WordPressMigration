using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using NDesk.Options;
using DataModels;
using OWC;
using System.Configuration;
using UncAccess;
using BusinessRule;

namespace ArticleDataServiceMigration2
{
    class Program
    {

        static rest rc = new rest();
        static DbCommon.dbcommon Dbcommon = new DbCommon.dbcommon();
        static string LogFileName = "";
        static int failedArticles = 0;

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: ArticleDataServiceMigration [OPTIONS]+ ");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        } // Show Help


        static void wait()
        {
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        static void writetolog(string errmsg)
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string msg = string.Format(" message:{0} ", errmsg);
            string LogPath = string.Format("{0}\\{1}.log", path, LogFileName);
            StreamWriter sw = new StreamWriter(LogPath, true);
            sw.WriteLine(msg);
            sw.Close();
            Console.WriteLine(msg);
        } // writetolog


        static bool CheckArgument(List<string> arg, string name)
        {
            if (arg.Count == 1)
                return true;
            string _msg = String.Format("You must specify argument --{0}", name);
            if (arg.Count > 1)
                _msg = String.Format("You must specify only one argument with  --{0}", name);
            Console.WriteLine("{0}", _msg);
            return false;
        }


        static bool DoesFileExist(string uncBasePath, string user, string pwd, string dom, string filenamepath)
        {
            bool status = false;
            using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
            {
                if (unc.NetUseWithCredentials(uncBasePath, user, dom, pwd))
                {
                    if (File.Exists(filenamepath))
                        status = true;
                }
            }// End using 
            return status;
        }


        static int ProcessImages(string article_uid, List<CONTENT_ITEM> ContentItems)
        {
            int NumberOfImages = 0;
            foreach (CONTENT_ITEM ContentItem in ContentItems)
            {
                string uid = ContentItem.CONTENT_ITEM_UID;
                List<IMAGE> ImageItems = BusinessModel.GetImageItems(uid);
                foreach (IMAGE ImageItem in ImageItems)
                {
                    string errmsg = "";
                    string imgurl = ImageItem.IMAGE_URL;
                    bool fileExists = false;
                    string ImageUrl = ConfigurationManager.AppSettings["ImageUrl"];
                    if (imgurl.StartsWith(ImageUrl))
                    {
                        string uncBasePath = ConfigurationManager.AppSettings["uncBasePath"];
                        string image_unc = imgurl.Replace(ImageUrl, string.Format(@"{0}\", uncBasePath)).Replace(@"/", @"\");
                        string dom = ConfigurationManager.AppSettings["uncDomain"];
                        string user = ConfigurationManager.AppSettings["uncUser"];
                        string pwd = ConfigurationManager.AppSettings["uncPwd"];
                        fileExists = DoesFileExist(uncBasePath, user, pwd, dom, image_unc);
                    }
                    else
                    {
                        fileExists = rc.ImageExists(ImageItem.IMAGE_URL);
                    }
                    if (fileExists)
                    {
                        //Console.WriteLine("Image FOUND {0}", ImageItem.IMAGE_URL);
                        int? filesize = (ImageItem.FILE_SIZE == null) ? 0 : ImageItem.FILE_SIZE;
                        string caption = (ImageItem.CAPTION == null) ? "" : ImageItem.CAPTION;
                        string media_type = (ImageItem.MEDIA_SIZE_TYPE_UCODE == null) ? "" : ImageItem.MEDIA_SIZE_TYPE_UCODE;
                        Dbcommon.spExecute("LoadImage", ref errmsg, "@article_uid", article_uid, "@asset_uid", uid, "@imagepath", ImageItem.IMAGE_URL, "@position", "0", "@width", ImageItem.WIDTH, "@height", ImageItem.HEIGHT, "@caption", caption, "@filesize", filesize, "@media_type", media_type);
                        if (errmsg.Length > 0)
                            writetolog(string.Format("Article id: {0} LoadImage {1} ", article_uid, errmsg));
                        else
                            NumberOfImages += 1;
                    }// end if fileExists
                    if (!fileExists)
                    {
                        //Console.WriteLine("Image does not exist {0}", ImageItem.IMAGE_URL);
                    }
                } // foreach IMAGE
            } // foreach contentitem
            return NumberOfImages;
        } // ProcessImages





        static int ProcessPdfs(string article_uid, List<CONTENT_ITEM> PdfContentItems)
        {
            int NumberofPdfs = 0;
            foreach (CONTENT_ITEM PdfContentItem in PdfContentItems)
            {
                string uid = PdfContentItem.CONTENT_ITEM_UID;
                List<PDF_VIEW> PdfItems = BusinessModel.GetPdfItems(uid);
                foreach (PDF_VIEW PdfItem in PdfItems)
                {
                    PdfItem.CAPTION = (PdfItem.CAPTION == null) ? "" : PdfItem.CAPTION;
                    PdfItem.BINARY_URL = (PdfItem.BINARY_URL == null) ? "" : PdfItem.BINARY_URL;
                    string errmsg = "";
                    if (PdfItem.BINARY_URL.Length > 0)
                    {



                        string imgurl = PdfItem.BINARY_URL;
                        bool fileExists = false;
                        string ImageUrl = ConfigurationManager.AppSettings["ImageUrl"];
                        if (imgurl.StartsWith(ImageUrl))
                        {
                            string uncBasePath = ConfigurationManager.AppSettings["uncBasePath"];
                            string image_unc = imgurl.Replace(ImageUrl, string.Format(@"{0}\", uncBasePath)).Replace(@"/", @"\");
                            string dom = ConfigurationManager.AppSettings["uncDomain"];
                            string user = ConfigurationManager.AppSettings["uncUser"];
                            string pwd = ConfigurationManager.AppSettings["uncPwd"];
                            fileExists = DoesFileExist(uncBasePath, user, pwd, dom, image_unc);
                        }
                        else
                        {
                            fileExists = rc.ImageExists(PdfItem.BINARY_URL);
                        }

                        if (fileExists)
                        {
                            Dbcommon.spExecute("Loadpdf", ref errmsg, "@article_uid", article_uid, "@asset_uid", PdfItem.CONTENT_ITEM_UID, "@binaryurl", PdfItem.BINARY_URL, "@caption", PdfItem.CAPTION);
                            if (errmsg.Length > 0)
                                writetolog(string.Format("Article id: {0} Loadpdf {1} ", article_uid, errmsg));
                            else
                                NumberofPdfs += 1;

                        } // end fileExists
                        if (!fileExists)
                            Console.WriteLine("PDF Image does not exist {0}", PdfItem.BINARY_URL);

                    } // end PdfItem.BINARY_URL.Length
                } //foreach PDF_VIEW
            } // foreach CONTENT_ITEM
            return NumberofPdfs;
        } // ProcessPdfs

        static int ProcessFreeForms(string article_uid, List<CONTENT_ITEM> FreeformContentItems)
        {
            int Numberoffreeforms = 0;
            foreach (CONTENT_ITEM FreeformContentItem in FreeformContentItems)
            {
                string uid = FreeformContentItem.CONTENT_ITEM_UID;
                List<FREEFORM> FreeFormItems = BusinessModel.GetFreeFormItems(uid);
                foreach (FREEFORM FreeFormItem in FreeFormItems)
                {
                    string errmsg = "";
                    string FREEFORM_HTML = (FreeFormItem.FREEFORM_HTML == null) ? "" : FreeFormItem.FREEFORM_HTML;
                    Dbcommon.spExecute("LoadFreeForm", ref errmsg, "@article_uid", article_uid, "@asset_uid", FreeFormItem.CONTENT_ITEM_UID, "@html", FREEFORM_HTML);
                    if (errmsg.Length > 0)
                        writetolog(string.Format("ProcessFreeForm Article id: {0}  freeform uid: {1}  {2} ", article_uid, FreeFormItem.CONTENT_ITEM_UID, errmsg));
                    else
                        Numberoffreeforms += 1;
                }// foreach
            } // foreach
            return Numberoffreeforms;
        } // ProcessFreeForms


        static void UpdateRelatedContent(string article_uid, List<CONTENT_ITEM_REL> RelatedContentitems, ref int NumberOfImages)
        {
            NumberOfImages = 0;
            int NumberofPdfs = 0, Numberoffreeforms = 0;
            Predicate<CONTENT_ITEM> FindImage = (CONTENT_ITEM p) => { return p.CONTENT_TYPE_UID == (int)BusinessModel.asset.image; };
            Predicate<CONTENT_ITEM> FindPdf = (CONTENT_ITEM p) => { return p.CONTENT_TYPE_UID == (int)BusinessModel.asset.pdf; };
            Predicate<CONTENT_ITEM> FreeForm = (CONTENT_ITEM p) => { return p.CONTENT_TYPE_UID == (int)BusinessModel.asset.freeform; };

            List<CONTENT_ITEM> MyContentItems = new List<CONTENT_ITEM>();
            foreach (CONTENT_ITEM_REL RelatedContentitem in RelatedContentitems)
            {
                string uid = RelatedContentitem.RELATED_CONTENT_ITEM_UID;
                List<CONTENT_ITEM> ContentItems = BusinessModel.GetContentItems(uid);
                MyContentItems.AddRange(ContentItems);
            }
            List<CONTENT_ITEM> ImageContentItems = MyContentItems.FindAll(FindImage);

            if (ImageContentItems.Count > 0)
                NumberOfImages = ProcessImages(article_uid, ImageContentItems);

            List<CONTENT_ITEM> PdfContentItems = MyContentItems.FindAll(FindPdf);
            if (PdfContentItems.Count > 0)
                NumberofPdfs = ProcessPdfs(article_uid, PdfContentItems);
            List<CONTENT_ITEM> FreeformContentItems = MyContentItems.FindAll(FreeForm);
            if (FreeformContentItems.Count > 0)
                Numberoffreeforms = ProcessFreeForms(article_uid, FreeformContentItems);


        } //UpdateRelatedContent




        static string GetArticleCategory(string siteid, string article_uid, string grouptype, string defaultname)
        {
            List<CONTENT_GROUP> MyContentGroups = new List<CONTENT_GROUP>();
            Predicate<CONTENT_GROUP> findcategory = (CONTENT_GROUP p) => { return p.CONTENT_GROUP_TYPE_UCODE.Equals(grouptype); };
            List<CONTENT_ITEM_CONTENT_GROUP> ContentItemContentGroups = BusinessModel.GetContentItemContentGroups(article_uid, siteid);
            foreach (CONTENT_ITEM_CONTENT_GROUP ContentItemContentGroup in ContentItemContentGroups)
            {
                List<CONTENT_GROUP> ContentGroup = BusinessModel.GetContentGroup(ContentItemContentGroup.CONTENT_GROUP_UID);
                MyContentGroups.AddRange(ContentGroup);
            }
            List<CONTENT_GROUP> categoryItems = MyContentGroups.FindAll(findcategory);
            string groupname = "", groupid = "", groupids = "";
            List<String> groupid_list = new List<string>();

            foreach (CONTENT_GROUP cg in categoryItems)
            {
                groupid = cg.CONTENT_GROUP_UID.Trim();
                groupname = (cg.GROUP_NAME == null) ? defaultname : cg.GROUP_NAME.Trim();
                groupid_list.Add(groupid);
                if (grouptype.Equals("CATEGORY") && defaultname.Equals("NEWS"))
                    break;

            }

            groupids += String.Join(",", groupid_list);

            if (grouptype.Equals("DISPLAYGROUP"))
                return groupids;
            if (grouptype.Equals("CATEGORY") && defaultname.Equals("0"))
                return groupids;

            if (string.IsNullOrEmpty(groupname))
                groupname = defaultname;
            return groupname;
        }


        static string scrub(string source)
        {
            Regex rgx = new Regex(@"[^\u0009\u000a\u000d\u0020-\uD7FF\uE000-\uFFFD]");
            return rgx.Replace(source, "");
        }


        static string GetOriginalcontentSiteId(string uid)
        {
            string siteuid = "";
            List<CONTENT_ITEM> contentItems = BusinessModel.GetContentItemsAll(uid);
            if (contentItems.Count == 1)
                siteuid = contentItems[0].SITE_UID;
            return siteuid;

        }

        static List<BusinessModel.CONTENT> GetRelatedArticles(string uid, string siteid)
        {
            List<BusinessModel.CONTENT> content = new List<BusinessModel.CONTENT>();
            try
            {
                content = BusinessModel.GetRelatedArticle(uid, siteid);
            }
            catch (System.Exception e)
            {
            }
            return content;
        }

        static void UpdateArticle(string siteid, string article_uid, CONTENT_ITEM ContentItemRow, int NumberOfImages)
        {
            string origsite = "";
            string errmsg = "";
            DateTime? StartDate = (ContentItemRow.FIRST_PUBLICATION_TIMESTAMP == null) ? ContentItemRow.LAUNCHED_DATE : ContentItemRow.FIRST_PUBLICATION_TIMESTAMP;
            DateTime? EndDate = (ContentItemRow.END_DATE == null) ? DateTime.Now.AddYears(100) : ContentItemRow.END_DATE;
            string keyword = (ContentItemRow.KEYWORD == null) ? "" : ContentItemRow.KEYWORD;
            string seodescription = scrub(ContentItemRow.SEO_DESCRIPTIVE_TEXT == null ? "" : ContentItemRow.SEO_DESCRIPTIVE_TEXT);
            string heading = scrub(ContentItemRow.CONTENT_TITLE == null ? "" : ContentItemRow.CONTENT_TITLE);
            string category = GetArticleCategory(siteid, article_uid, "CATEGORY", "NEWS");
            string categorygroups = GetArticleCategory(siteid, article_uid, "CATEGORY", "0");
            string displaygroups = GetArticleCategory(siteid, article_uid, "DISPLAYGROUP", "0");
            string[] ngps_groups;
            if (string.IsNullOrEmpty(displaygroups)) {
                ngps_groups = new string[] { categorygroups.Trim() };
            }
            else
            {
                ngps_groups = new string[] { categorygroups.Trim(), displaygroups.Trim() };
            }

            // string vanity_url = GetSectionItem()
            List<BusinessModel.CONTENT> content = GetRelatedArticles(ContentItemRow.CONTENT_ITEM_UID, siteid);
            string sRelatedArticles = "";
            BusinessModel.CONTENT lastitem = null;
            if (content.Count > 0)
                lastitem = content[content.Count - 1];
            string comma = ",";
            foreach (BusinessModel.CONTENT c in content)
            {
                if (c.Equals(lastitem))
                    comma = "";
                sRelatedArticles += string.Format("{0}{1}", c.uid, comma);
            }
            string origContentItemUid = (ContentItemRow.ORIG_CONTENT_ITEM_UID == null) ? "" : ContentItemRow.ORIG_CONTENT_ITEM_UID;
            string SectionAnchorUID = (ContentItemRow.SECTION_ANCHOR_UID == null) ? "0" : ContentItemRow.SECTION_ANCHOR_UID;

            if (origContentItemUid.Length > 0)
                origsite = GetOriginalcontentSiteId(origContentItemUid);  // shared content save original Site UID

            List<ARTICLE> Article = BusinessModel.GetArticle(article_uid);
            if (Article.Count != 1)
            {
                writetolog(string.Format("Content id: {0} rows returned: {1}", article_uid, Article.Count));
                return;
            }
            ARTICLE ArticleRow = Article[0];
            string summary = scrub((ArticleRow.ABSTRACT == null) ? "" : ArticleRow.ABSTRACT);
            string byline = scrub((ArticleRow.BYLINE == null) ? "" : ArticleRow.BYLINE);
            string body = scrub((ArticleRow.BODY == null) ? "" : ArticleRow.BODY);
            string subtitle = scrub((ArticleRow.SUBTITLE == null) ? "" : ArticleRow.SUBTITLE);
            string email = scrub((ArticleRow.EMAIL_OF_AUTHOR == null) ? "" : ArticleRow.EMAIL_OF_AUTHOR);
            string section_anchor_uid = scrub((ContentItemRow.SECTION_ANCHOR_UID == null) ? "" : ContentItemRow.SECTION_ANCHOR_UID);

            //Get section, the hard way
            string vanity_url = string.Empty;
            List<SECTION_ITEM> Section = BusinessModel.GetSection(section_anchor_uid);
            if (Section.Count != 1)
            {
                //writetolog(string.Format("Section id: {0} rows returned: {1}", section_anchor_uid, Section.Count));
                vanity_url = "/news";
                //return;
            }
            else
            {
                SECTION_ITEM SectionRow = Section[0];
                vanity_url = scrub((SectionRow.VANITY_URL == null) ? "/news" : SectionRow.VANITY_URL);
            }
            //Get main URL, the hard way
            List<SITE_URL> urls = BusinessModel.GetSiteURL(siteid);
            if (urls.Count != 1)
            {
                writetolog(string.Format("Site id: {0} rows returned: {1}", siteid, urls.Count));
                return;
            }
            SITE_URL url = urls[0];
            string site_url = scrub((url.URL == null) ? "www." + email.Split('@')[1].Trim() : url.URL.Replace("qa2live", "www"));

            Dbcommon.spExecute("LoadArticle", ref errmsg, "@siteid", siteid, "@article_uid", article_uid, "@category", category, "@displaygroup", String.Join(",", ngps_groups), "@anchor", SectionAnchorUID, "@startdate", StartDate, "@enddate", EndDate, "@heading", heading, "@body", body, "@byline", byline, "@subtitle", subtitle, "@summary", summary, "@RelatedArticles", sRelatedArticles, "@seodescription", seodescription, "@keyword", keyword, "@imagecount", NumberOfImages, "@origsite", origsite, "@email", email, "@vanity_url", vanity_url, "@site_url", site_url);
            if (errmsg.Length > 0)
            {
                writetolog(string.Format("Failed Article id: {0} LoadArticle {1} ", article_uid, errmsg));
                failedArticles += 1;
            }
        }



        static void Main(string[] args)
        {
            int articleCount = 0;
            bool show_help = false;
            List<string> extra;
            List<string> siteCodes = new List<string>();
            List<string> Logfiles = new List<string>();
            List<string> startDates = new List<string>();
            List<string> endDates = new List<string>();
            var p = new OptionSet()
            {
                { "c|sitecode=", " (required) the source Site Code.",  v=> siteCodes.Add(v) },
                { "s|start=", " (required) the start Date.",  v=> startDates.Add(v) },
                { "e|end=", " (required) the End Date.",  v=> endDates.Add(v) },
                { "l|log=", " (required) log filename.",  v=> Logfiles.Add(v) },
                { "h|help",  " Show this message and exit", v => show_help = v != null },
            };
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("migration: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try migration --help for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);
                Console.ReadLine();
                return;
            }

            bool valid = true;
            if (!CheckArgument(siteCodes, "sitecode"))
                valid = false;
            if (!CheckArgument(Logfiles, "Log"))
                valid = false;
            if (!CheckArgument(startDates, "start"))
                valid = false;
            if (!CheckArgument(endDates, "end"))
                valid = false;
            if (!valid)
            {
                wait();
                return;
            }

            string siteid = siteCodes[0];
            DateTime startdate = DateTime.ParseExact(startDates[0], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime stopDate = DateTime.ParseExact(endDates[0], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            LogFileName = Logfiles[0];

            Console.WriteLine("Query NGPS DB for Site: " + siteid + ", StartDate: " + startdate.ToString("MM/dd/yyyy") + ", StopDate: " + stopDate.ToString("MM/dd/yyyy"));
            List<CONTENT_ITEM> ContentItemRows = BusinessModel.GetContentItems(siteid, (int)BusinessModel.asset.article, startdate, stopDate);
            Console.WriteLine("Total Articles: {0} ", ContentItemRows.Count);
            int total = ContentItemRows.Count;
            float throttle = 0;
            DateTime startwatch2 = DateTime.Now;
            int cnt = 0;

            Console.WriteLine("Process reuslts");
            foreach (CONTENT_ITEM ContentItemRow in ContentItemRows)
            {
                string article_uid = ContentItemRow.CONTENT_ITEM_UID;
                int NumberOfImages = 0;
                bool articleExists = BusinessRule.BusinessRule.DoesArticleExist(article_uid);
                if (articleExists)
                {
                    Console.Write("x");
                    // Console.WriteLine(string.Format("Article: {0} exists already skip...", article_uid));
                }
                else
                {
                    //   Console.WriteLine("Start Get Related Content Articles: {0} ", article_uid);          
                    Console.Write("+");

                    List<CONTENT_ITEM_REL> RelatedContentitems = BusinessModel.GetRelatedContentitems(article_uid);
                    if (RelatedContentitems.Count > 0)
                    {
                        UpdateRelatedContent(article_uid, RelatedContentitems, ref NumberOfImages);
                    } // RelatedContentitems
                      //   Console.WriteLine("Finished Get Related Content Articles: {0} ", article_uid);

                    UpdateArticle(siteid, article_uid, ContentItemRow, NumberOfImages);
                }
                articleCount += 1;      

                if (articleCount < 100 && articleCount % 10 == 0)
                {
                    Console.WriteLine();
                    TimeSpan t2 = DateTime.Now.Subtract(startwatch2);
                    int remaining = total - articleCount;
                    float avg = (float)t2.Seconds / articleCount;
                    throttle += avg;
                    cnt += 1;
                    avg = throttle / (float)cnt;
                    float totaverage = (float)remaining * avg; // Number of seconds remaining
                    TimeSpan t3 = TimeSpan.FromSeconds(totaverage);
                    Console.WriteLine(string.Format("{0}/{1} Remaining Time Est: {2}", articleCount, total, t3));
                }
                if ((articleCount % 100) == 0)
                {
                    Console.WriteLine();
                    TimeSpan t2 = DateTime.Now.Subtract(startwatch2);
                    int remaining = total - articleCount;
                    float avg = (float)t2.Seconds / 100;
                    throttle += avg;
                    cnt += 1;
                    avg = throttle / (float)cnt;
                    float totaverage = (float)remaining * avg; // Number of seconds remaining
                    TimeSpan t3 = TimeSpan.FromSeconds(totaverage);
                    Console.WriteLine(string.Format("{0}/{1} Remaining Time Est: {2}", articleCount, total, t3));
                    startwatch2 = DateTime.Now;
                }

            } // foreach
            Console.WriteLine(DateTime.Now.ToString());
        }// main

    }
}
