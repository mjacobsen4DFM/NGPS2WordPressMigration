using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Collections;
using NDesk.Options;
using BusinessRule;
using System.Text.RegularExpressions;

namespace Migration
{
    class Program
    {


        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: migration [OPTIONS]+ ");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        } // Show Help

        static void wait()
        {
            Console.WriteLine("Press enter to exit");
            Console.Read();
        }


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


        static void Main(string[] args)
        {
            bool show_help = false;
            bool images_only = false;
            bool articles_only = false;
            bool imageUpdate = false;
            bool forcearticlesExcludeImages = false;
            bool filter = false;
            List<string> extra;
            List<string> siteCodes = new List<string>();
            List<string> destCodes = new List<string>();
            List<string> startDates = new List<string>();
            List<string> endDates = new List<string>();
            var p = new OptionSet()
            {
                { "c|sitecode=", " (required) the source Site Code.",  v=> siteCodes.Add(v) },
                { "d|destcode=", " (required) the Destination Pub Code.",  v=> destCodes.Add(v) },
                { "s|start=", " (required) the start Date.",  v=> startDates.Add(v) },
                { "e|end=", " (required) the End Date.",  v=> endDates.Add(v) },
                { "i|images", " (Optional) migrate Images Only",  v=> images_only = v != null },
                { "f|forceArticlesOnly", " (Optional) migrate Articles do not include Images",  v=> forcearticlesExcludeImages = v != null },
                { "w|filter=", " (Optional) migrate Articles based on filter key app.config",  v=> filter = v != null },
                { "a|articles", " (Optional) migrate articles Only ( MUST MIGRATE IMAGES FIRST )",  v=> articles_only = v != null },
                { "u|update Articles with Images", " (Optional) updates existing articles with images",  v=> imageUpdate = v != null },

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
            if (!CheckArgument(destCodes, "destcode"))
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
            Int32 wait_time = Convert.ToInt32(ConfigurationManager.AppSettings["wait_time"].ToString());
            wait_time *= 1000;

            string siteid = siteCodes[0];
            string destination_siteid = destCodes[0];
            string taxpubid = BusinessRule.BusinessRule.GetWordpressPubid(siteid, destination_siteid);
            if (taxpubid.Length == 0)
            {
                Console.WriteLine("Please assign a [taxonomyPubId] in table: [cms_pubMap] for siteid: {0}", siteid);
                wait();
                return;

            }
            DateTime startdate = DateTime.ParseExact(startDates[0], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime stopdate = DateTime.ParseExact(endDates[0], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DataRow cmsData = BusinessRule.BusinessRule.GetCMSData(Convert.ToInt32(siteid), destination_siteid);
            //string hostUrl = "http://dfm-redesign-dev.medianewsgroup.com";
            string documents = ConfigurationManager.AppSettings["documents"].ToString();
            string temp = ConfigurationManager.AppSettings["temp"].ToString();
            string SkipExists = ConfigurationManager.AppSettings["SkipExists"].ToString();
            string imageCheckSQL = ConfigurationManager.AppSettings["imageCheckSQL"].ToString();
            string missingArticlesSQL = ConfigurationManager.AppSettings["missingArticlesSQL"].ToString();
            string articlesMissingImagesSQL = ConfigurationManager.AppSettings["articlesMissingImagesSQL"].ToString();

            //FixEntries()
            //fixImagesMin(siteid, destination_siteid, startdate, stopdate, hostUrl, username, pwd);
            //return;

            //string sqlstatement = String.Format("select article_uid from article where siteid = {2} and article_uid in(26365014)", startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid);
            //string sqlstatement = String.Format("select article_uid from article where startdate >= '{0}' and startdate <= '{1}' and siteid = {2}", startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
            //sqlstatement += String.Format(" and article.article_uid not in(select article_uid from saxo_article where siteid = {2} and destination_siteid = '{3}')", startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
            //sqlstatement += String.Format(" and article.article_uid not in(select article_uid from success_wp where siteid = {2} and destination_siteid = '{3}')", startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
            //String sqlFormat = System.IO.File.ReadAllText(@"C:\Users\Mick\Documents\SQL Server Management Studio\Big Join for Articles Missing Images.sql");
            //String sqlFormat = System.IO.File.ReadAllText(@"C:\Users\Mick\Documents\SQL Server Management Studio\Simple Join for Missing Articles - Final check.sql");
            String sqlFormat = System.IO.File.ReadAllText(missingArticlesSQL);
            String sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
            if (filter)
            {
                string filterString = ConfigurationManager.AppSettings["filter"].ToString();
                sqlstatement = String.Format("{0} {1} ", sqlstatement, filterString);

            }

            ArrayList holdrules = BusinessRule.BusinessRule.htmlholdrules();

            if (images_only || imageUpdate)
                sqlstatement = String.Format("{0} and imagecount > 0 order by startdate", sqlstatement);
            else
                sqlstatement = String.Format("{0} order by startdate", sqlstatement);

            string looplog = String.Format("C:\\temp\\TrackLoops_{0}_{1}_{2}_{3}.txt", siteid, startdate.Month, startdate.Day, startdate.Year);
            int loopcount = 0;

            Int32 defaultAuthorTermID = 370; // 2130 wpd; //370 wpm

            Console.WriteLine(sqlstatement);
            //Intial Migration
            BusinessRule.BusinessRule br = new BusinessRule.BusinessRule();
            //while (BusinessRule.BusinessRule.BusGetDataset(sqlstatement).Tables[0].Rows.Count > 0)
            //{
            if (br.AllStop())
            {
                Console.WriteLine("All Stop! {0}", DateTime.Now.ToString());
                return;
            }
            loopcount += 1;
            System.IO.File.WriteAllText(looplog, "Iteration " + loopcount + " at " + DateTime.Now.ToString());
            DataSet ds = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
            BusinessRule.BusinessRule._NGPS_DisplayGroupList = Migration2WP.Preload.GetDisplayGroups(siteid);
            BusinessRule.BusinessRule._wpCategories = Migration2WP.Preload.GetTerms(siteid, destination_siteid, "category");
            BusinessRule.BusinessRule._wpTags = Migration2WP.Preload.GetTerms(siteid, destination_siteid, "post_tag");
            BusinessRule.BusinessRule._wpLocation = Migration2WP.Preload.GetTerms(siteid, destination_siteid, "location");
            BusinessRule.BusinessRule._wpAuthTerms = Migration2WP.Preload.GetTerms(siteid, destination_siteid, "author");

            migrateArticles(taxpubid, siteid, startdate, stopdate, destination_siteid, documents, temp, cmsData, defaultAuthorTermID, holdrules, imageCheckSQL, ds);
            //} //while

            ////Fix Migration (migrate missing images)
            //sqlFormat = System.IO.File.ReadAllText(articlesMissingImagesSQL);
            //sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
            //Console.WriteLine(sqlstatement);
            ////while (BusinessRule.BusinessRule.BusGetDataset(sqlstatement).Tables[0].Rows.Count > 0)
            ////{
            //    if (br.AllStop())
            //    {
            //        Console.WriteLine("All Stop! {0}", DateTime.Now.ToString());
            //        return;
            //    }
            //    loopcount += 1;
            //    System.IO.File.WriteAllText(looplog, "Iteration " + loopcount + " at " + DateTime.Now.ToString());
            //    DataSet ds = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
            //    migrateArticles(taxpubid, siteid, startdate, stopdate, destination_siteid, documents, temp, cmsData, defaultAuthorTermID, holdrules, imageCheckSQL, ds);
            ////}


            if (br.AllStop())
            {
                Console.WriteLine("All Stop! {0}", DateTime.Now.ToString());
            }
            else
            {
                Console.WriteLine("End: {0}", DateTime.Now.ToString());
            }
            //wait();
            return;
        } // Main


        static void migrateArticles(string taxpubid, string siteid, DateTime startdate, DateTime stopdate, string destination_siteid, string documents, string temp, DataRow cmsData, int defaultAuthorTermID, ArrayList holdrules, string imageCheckSQL, DataSet ds)
        {
            int total = ds.Tables[0].Rows.Count;
            Console.WriteLine("Total: {0}", total);
            int articleCount = 0;
            bool status = true;
            DateTime startwatch2 = DateTime.Now;

            Console.WriteLine("Start: {0}", startwatch2.ToString());
            //BusinessRule.BusinessRule.DeleteWPPosts(taxpubid, hostUrl, username, pwd);
            string log = String.Format("C:\\temp\\Migrate2WP_{0}_{1}_{2}.txt", siteid, startdate.Month, startdate.Year);
            int articleTrack = 0;
            BusinessRule.BusinessRule br = new BusinessRule.BusinessRule();
            //if -i - a not used then migrate image , galleries , and articles  (-u switch will update images for articles that were sent)
            foreach (DataRow articleRow in ds.Tables[0].Rows)
            {
                articleCount += 1;
                articleTrack = articleCount - 1;
                string article_uid = articleRow["article_uid"].ToString();
                try
                {
                    if (br.AllStop()) { return; }
                    DateTime s1 = DateTime.Now;
                    status = br.MigrateArticle(startdate.ToString("yyyyMMdd"), stopdate.ToString("yyyyMMdd"), documents, temp, cmsData, article_uid, "N", holdrules, imageCheckSQL);
                    TimeSpan s2 = DateTime.Now.Subtract(s1);
                    if (articleCount <= 110000) { Console.WriteLine("\t\t{0}", s2); }
                    if (articleCount == 1)
                    {
                        startwatch2 = DateTime.Now;
                        //Console.Write(".");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    System.IO.File.AppendAllText(log, String.Format("{0}{1}", ex.Message, System.Environment.NewLine));
                }

                if (articleTrack < 100 && articleTrack != 0 && articleTrack % 10 == 0)
                {
                    Console.WriteLine();
                    TimeSpan t2 = DateTime.Now.Subtract(startwatch2);
                    int remaining = total - articleTrack;
                    float seconds = ((float)t2.Minutes * 60) + (float)t2.Seconds;
                    float avg = seconds / articleTrack;
                    float totaverage = (float)remaining * avg; // Number of seconds remaining
                    TimeSpan t3 = TimeSpan.FromSeconds(totaverage);
                    Console.WriteLine(String.Format("{0}/{1} Remaining Est: {2}, Time: {3}, Seconds: {4}, Average: {5}", articleTrack, total, t3, DateTime.Now.ToString(), seconds, avg));
                }
                if (articleTrack != 0 && (articleTrack % 100) == 0)
                {
                    Console.WriteLine(String.Empty);
                    TimeSpan t2 = DateTime.Now.Subtract(startwatch2);
                    int remaining = total - articleTrack;
                    float seconds = ((float)t2.Hours * 3600) + ((float)t2.Minutes * 60) + (float)t2.Seconds;
                    float avg = seconds / articleTrack;
                    float totaverage = (float)remaining * avg; // Number of seconds remaining
                    TimeSpan t3 = TimeSpan.FromSeconds(totaverage);
                    Console.WriteLine(String.Format("{0}/{1} Remaining Est: {2}, Time: {3}, Seconds: {4}, Average: {5}", articleTrack, total, t3, DateTime.Now.ToString(), seconds, avg));
                    //startwatch2 = DateTime.Now;
                }
            } // foreach
        }


        //static void fixImagesMin(string siteid, string destination_siteid, DateTime startdate, DateTime stopdate, string hostUrl, string username, string pwd)
        //{
        //    int articleCount = 0;
        //    BusinessRule.BusinessRule br = new BusinessRule.BusinessRule();
        //    Console.WriteLine(String.Empty);
        //    Console.WriteLine("Fixing Images");
        //    //string sqlFormat = "select article.article_uid, storyurl as post_id, startdate, body, summary, category  from article, saxo_article where article.article_uid = '26830377' and article.article_uid = saxo_article.article_uid and article.siteid = saxo_article.siteid and startdate >= '{0}' and startdate <= '{1}' and article.siteid = {2} and destination_siteid = '{3}'";
        //    string sqlFormat = System.IO.File.ReadAllText("C:\\Users\\Mick\\Documents\\SQL Server Management Studio\\Articles and images - missing-min.sql");
        //    string sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
        //    Console.WriteLine(sqlstatement);
        //    DataSet ds;
        //    DataSet ds2;
        //    string article_uid = String.Empty;
        //    string pubdate = String.Empty;
        //    Int32 post_id = 0;
        //    string img_id = String.Empty;
        //    Post post = new Post();
        //    Boolean bFound = false;
        //    string log = String.Format("C:\\temp\\fixImages_{0}_{1}_{2}.txt", siteid, startdate.Month, startdate.Year);

        //    //instantiate WP client
        //    WordPressSiteConfig config = new WordPressSiteConfig
        //    {
        //        BaseUrl = hostUrl,
        //        Username = username,
        //        Password = pwd
        //    };
        //    WordPressClient _wpClient = new WordPressClient(config);

        //    try
        //    {
        //        ds = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
        //    }
        //    catch (Exception)
        //    {
        //        throw; ;
        //    }

        //    int total = ds.Tables[0].Rows.Count;

        //    Console.WriteLine("Fetched {0} rows", total);

        //    foreach (DataRow articleRow in ds.Tables[0].Rows)
        //    {
        //        bFound = false;
        //        article_uid = articleRow["article_uid"].ToString();
        //        pubdate = articleRow["startdate"].ToString();
        //        post_id = Convert.ToInt32(articleRow["post_id"].ToString());
        //        articleCount += 1;
        //        try
        //        {
        //            sqlFormat = "select image.[asset_uid],[imagepath],[position],[width],[height],[media_type],[caption],[filesize] from image, saxo_image where image.asset_uid = saxo_image.asset_uid and image.asset_uid in (select asset_uid from asset where asset_type = {1} and  article_uid = '{0}') and saxo_image.destination_siteid = '{2}'";
        //            sqlstatement = String.Format(sqlFormat, article_uid, (Int16)(BusinessRule.BusinessRule.asset.image), destination_siteid);
        //            ds2 = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
        //            //Console.Write(">");
        //            //if (ds2.Tables[0].Rows.Count == 0) { continue; }
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("S");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + System.Environment.NewLine);
        //            continue;
        //        }

        //        try
        //        {
        //            post = _wpClient.GetPost(post_id);
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("@");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + System.Environment.NewLine);
        //            continue;
        //        }

        //        try
        //        {
        //            Console.Write("A");
        //            System.IO.File.AppendAllText(log, string.Format("Article: {0}, post: {1}{2}", article_uid, post_id, System.Environment.NewLine));
        //            MediaFilter mf = new MediaFilter();
        //            mf.Number = 100;
        //            mf.ParentId = Convert.ToString(post_id);
        //            MediaItem[] mediaItems = _wpClient.GetMediaItems(mf);
        //            Console.Write("i{0}", ds2.Tables[0].Rows.Count);
        //            Console.Write("m{0}", mediaItems.Length);
        //            string author = post.Author;
        //            for (int i = 0; i <= mediaItems.Length - 1; i++)
        //            {

        //                MediaItem img = mediaItems[i];
        //                foreach (DataRow imageRow in ds2.Tables[0].Rows)
        //                {
        //                    string asset_uid = imageRow["asset_uid"].ToString();
        //                    string imageSource = imageRow["imagepath"].ToString();
        //                    string[] imageUrlParts = imageSource.Split('/');
        //                    string imageName = imageUrlParts[imageUrlParts.Length - 1];
        //                    string excerpt = imageRow["caption"].ToString();
        //                    String cleanImageName = CleanString(imageName);
        //                    if (img.Title.Contains(cleanImageName))
        //                    {
        //                        try
        //                        {

        //                            WordPressExtension.WordPressExtension.AddExcerpt(article_uid, img.Id, excerpt, pubdate, post.Terms, hostUrl, username, pwd, destination_siteid, log);
        //                            BusinessRule.BusinessRule.UpdateImageInfo(asset_uid, destination_siteid, img.Id);
        //                            bFound = true;
        //                            //Console.Write("^");
        //                        }
        //                        catch (Exception)
        //                        {

        //                            Console.Write("%");
        //                            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //                        }
        //                    }
        //                }
        //            }

        //            if (bFound)
        //            {
        //                string galleryShortcode = "[gallery]";
        //                string content = articleRow["body"].ToString();
        //                post.MediaItems = mediaItems;
        //                post.FeaturedImageId = mediaItems[0].Id;
        //                if (mediaItems.Length > 1) { post.Content = galleryShortcode + content; }
        //                post.PublishDateTime = Convert.ToDateTime(pubdate);
        //                try
        //                {
        //                    _wpClient.EditPost(post);
        //                }
        //                catch (Exception)
        //                {
        //                    Console.Write("#");
        //                    System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("*");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //        }

        //        if ((articleCount % 100) == 0)
        //        {
        //            Console.WriteLine(String.Empty);
        //            Console.WriteLine(String.Format("{0}/{1} ", articleCount, total));
        //        }
        //    }
        //}

        //static void fixImages(string siteid, string destination_siteid, DateTime startdate, DateTime stopdate, string hostUrl, string username, string pwd)
        //{
        //    int articleCount = 0;
        //    BusinessRule.BusinessRule br = new BusinessRule.BusinessRule();
        //    Console.WriteLine(String.Empty);
        //    Console.WriteLine("Fixing Images");
        //    //string sqlFormat = "select article.article_uid, storyurl as post_id, startdate, body, summary, category  from article, saxo_article where article.article_uid = '26830377' and article.article_uid = saxo_article.article_uid and article.siteid = saxo_article.siteid and startdate >= '{0}' and startdate <= '{1}' and article.siteid = {2} and destination_siteid = '{3}'";
        //    string sqlFormat = "select article.article_uid, storyurl as post_id, startdate, body, summary, category  from article, saxo_article where article.article_uid = saxo_article.article_uid and article.siteid = saxo_article.siteid and startdate >= '{0}' and startdate <= '{1}' and article.siteid = {2} and destination_siteid = '{3}'";
        //    string sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
        //    Console.WriteLine(sqlstatement);
        //    DataSet ds;
        //    DataSet ds2;
        //    string article_uid = String.Empty;
        //    string pubdate = String.Empty;
        //    Int32 post_id = 0;
        //    string img_id = String.Empty;
        //    Post post = new Post();
        //    Boolean bFound = false;
        //    string log = String.Format("C:\\temp\\fixImages_{0}_{1}_{2}.txt", siteid, startdate.Month, startdate.Year);

        //    //instantiate WP client
        //    WordPressSiteConfig config = new WordPressSiteConfig
        //    {
        //        BaseUrl = hostUrl,
        //        Username = username,
        //        Password = pwd
        //    };
        //    WordPressClient _wpClient = new WordPressClient(config);

        //    try
        //    {
        //        ds = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
        //    }
        //    catch (Exception)
        //    {
        //        throw; ;
        //    }

        //    int total = ds.Tables[0].Rows.Count;


        //    foreach (DataRow articleRow in ds.Tables[0].Rows)
        //    {
        //        bFound = false;
        //        article_uid = articleRow["article_uid"].ToString();
        //        pubdate = articleRow["startdate"].ToString();
        //        post_id = Convert.ToInt32(articleRow["post_id"].ToString());
        //        articleCount += 1;
        //        try
        //        {
        //            sqlFormat = String.Format("select image.[asset_uid],[imagepath],[position],[width],[height],[media_type],[caption],[filesize] from image, saxo_image where image.asset_uid = saxo_image.asset_uid and image.asset_uid in (select asset_uid from asset where asset_type = {1} and  article_uid = '{0}') and saxo_image.destination_siteid = '{2}'", article_uid, (Int16)(BusinessRule.BusinessRule.asset.image), destination_siteid);
        //            sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
        //            ds2 = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);
        //            Console.Write(">");
        //            if (ds2.Tables[0].Rows.Count == 0) { continue; }
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("S");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + System.Environment.NewLine);
        //            continue;
        //        }

        //        try
        //        {
        //            post = _wpClient.GetPost(post_id);
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("@");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + System.Environment.NewLine);
        //            continue;
        //        }

        //        try
        //        {
        //            MediaFilter mf = new MediaFilter();
        //            mf.Number = 100;
        //            mf.ParentId = Convert.ToString(post_id);
        //            MediaItem[] mediaItems = _wpClient.GetMediaItems(mf);
        //            string author = post.Author;
        //            for (int i = 0; i <= mediaItems.Length - 1; i++)
        //            {

        //                MediaItem img = mediaItems[i];
        //                foreach (DataRow imageRow in ds2.Tables[0].Rows)
        //                {
        //                    string asset_uid = imageRow["asset_uid"].ToString();
        //                    string imageSource = imageRow["imagepath"].ToString();
        //                    string[] imageUrlParts = imageSource.Split('/');
        //                    string imageName = imageUrlParts[imageUrlParts.Length - 1];
        //                    string excerpt = imageRow["caption"].ToString();
        //                    if (img.Title.Contains(imageName.Replace(' ', '-')) && String.IsNullOrEmpty(img.Caption))
        //                    {
        //                        try
        //                        {

        //                            WordPressExtension.WordPressExtension.AddExcerpt(article_uid, img.Id, excerpt, pubdate, post.Terms, hostUrl, username, pwd, destination_siteid, log);
        //                            BusinessRule.BusinessRule.UpdateImageInfo(asset_uid, destination_siteid, img.Id);
        //                            bFound = true;
        //                            //Console.Write("^");
        //                        }
        //                        catch (Exception)
        //                        {

        //                            Console.Write("%");
        //                            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //                        }
        //                    }
        //                }
        //            }

        //            if (bFound)
        //            {
        //                string galleryShortcode = "[gallery]";
        //                string content = articleRow["body"].ToString();
        //                post.MediaItems = mediaItems;
        //                post.FeaturedImageId = mediaItems[0].Id;
        //                if (mediaItems.Length > 1) { post.Content = galleryShortcode + content; }
        //                post.PublishDateTime = Convert.ToDateTime(pubdate);
        //                try
        //                {
        //                    _wpClient.EditPost(post);
        //                }
        //                catch (Exception)
        //                {
        //                    Console.Write("#");
        //                    System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            Console.Write("*");
        //            System.IO.File.AppendAllText(log.Replace("fixImages", "replay"), "article_uid, " + article_uid + ", Post.id, " + post.Id.ToString() + System.Environment.NewLine);
        //        }

        //        if ((articleCount % 100) == 0)
        //        {
        //            Console.WriteLine(String.Empty);
        //            Console.WriteLine(String.Format("{0}/{1} ", articleCount, total));
        //        }
        //    }
        //}

        //static void fixEntries()
        //{
        //    //articleCount = 0;
        //    //Console.WriteLine(String.Empty);
        //    //Console.WriteLine("Fixing Articles");
        //    //string sqlFormat = "select article.article_uid, storyurl as post_id, startdate, summary, category  from article, saxo_article where article.article_uid = saxo_article.article_uid and article.siteid = saxo_article.siteid and startdate >= '{0}' and startdate <= '{1}' and article.siteid = {2} and destination_siteid = '{3}'";
        //    //sqlstatement = String.Format(sqlFormat, startdate.ToShortDateString(), stopdate.ToShortDateString(), siteid, destination_siteid);
        //    //Console.WriteLine(sqlstatement);
        //    //ds = BusinessRule.BusinessRule.BusGetDataset(sqlstatement);

        //    //total = ds.Tables[0].Rows.Count;

        //    //foreach (DataRow articleRow in ds.Tables[0].Rows)
        //    //{
        //    //    articleCount += 1;
        //    //    string article_uid = articleRow["article_uid"].ToString();
        //    //    string post_id = articleRow["post_id"].ToString();
        //    //    string pubdate = articleRow["startdate"].ToString();
        //    //    string excerpt = articleRow["summary"].ToString();
        //    //    string fieldName = "category";
        //    //    string custom_key = "original_category";
        //    //    string custom_value = articleRow[fieldName].ToString();
        //    //    string custom_key2 = "original_id";
        //    //    string custom_value2 = article_uid;
        //    //    string log = String.Format("C:\\temp\\fix_{0}_{1}_{2}.txt", siteid, startdate.Month, startdate.Year);
        //    //    WordPressExtension.WordPressExtension.AddExcerpt(article_uid, post_id, excerpt, hostUrl, username, pwd, destination_siteid, log);
        //    //    WordPressExtension.WordPressExtension.AddCustomField(article_uid, post_id, custom_key, custom_value, "*", ">", hostUrl, username, pwd, destination_siteid, log);
        //    //    WordPressExtension.WordPressExtension.AddCustomField(article_uid, post_id, custom_key2, custom_value2, "&", "-", hostUrl, username, pwd, destination_siteid, log);
        //    //    WordPressExtension.WordPressExtension.FixPubdate(article_uid, post_id, pubdate, hostUrl, username, pwd, destination_siteid, log);
        //    //    if ((articleCount % 100) == 0)
        //    //    {
        //    //        Console.WriteLine(String.Empty);
        //    //        Console.WriteLine(String.Format("{0}/{1} ", articleCount, total));
        //    //    }
        //    //}
        //}

        static string CleanString(string input)
        {
            // Replace invalid characters with empty strings.
            String clean;
            try
            {
                clean = Regex.Replace(input, @"[\s]", "-", RegexOptions.None);
                clean = Regex.Replace(clean, @"[^a-zA-Z0-9-\._]", "", RegexOptions.None);
                clean = Regex.Replace(clean, @"\-{2,}", "-", RegexOptions.None);
                while (Regex.Match(clean, @"\.([a-zA-Z]{2,5}[0-9]*)\.").Success)
                {
                    clean = Regex.Replace(clean, @"\.([a-zA-Z]{2,5}[0-9]*)\.", ".$1_.", RegexOptions.None);
                }
                return clean;
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return input;
            }
        }
    } // Program
} // End Namespace Migration
