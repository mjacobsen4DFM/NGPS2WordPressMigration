using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessRule;
using System.Data;
using DataAuthentication;
using com.DFM.FeedHub.WordPressClient.Models;

namespace Migration2WP
{
    class Preload
    {

        public static List<NGPS_DisplayGroup2WP_SectionMap> GetDisplayGroups(String siteid)
        {
            List<NGPS_DisplayGroup2WP_SectionMap> NGPS_DisplayGroupList = new List<NGPS_DisplayGroup2WP_SectionMap>();

            DataSet dsCategories = default(DataSet);
            //WP Category = NGPS Section
            dsCategories = DataAuthentication.DataAuthentication.GetSectionMap(siteid.ToString());

            NGPS_DisplayGroup2WP_SectionMap displayGroupMap = new NGPS_DisplayGroup2WP_SectionMap();

            foreach (DataRow row in dsCategories.Tables[0].Rows)
            {
                displayGroupMap = new NGPS_DisplayGroup2WP_SectionMap();
                displayGroupMap.site_uid = Int16.Parse(row["site_uid"].ToString());
                displayGroupMap.group_id = Int32.Parse(row["group_id"].ToString());
                displayGroupMap.group_name = row["group_name"].ToString();
                displayGroupMap.wp_section_list = row["wp_section_list"].ToString().Replace(", ", ",").Split(',').ToList();
                displayGroupMap.wp_tag_slug_list = row["wp_tag_slug_list"].ToString().Replace(", ", ",").Split(',').ToList();
                displayGroupMap.wp_location_list = row["wp_location_list"].ToString().Replace(", ", ",").Split(',').ToList();
                if ((!NGPS_DisplayGroupList.Contains(displayGroupMap)))
                    NGPS_DisplayGroupList.Add(displayGroupMap);
            }
            return NGPS_DisplayGroupList;
        }


        public static List<Term> GetTerms(String siteid, String destination_siteid, String taxonomy)
        {
            List<Term> terms = new List<Term>();

            DataSet dsCategories = default(DataSet);
            //WP Category = NGPS Section
            dsCategories = DataAuthentication.DataAuthentication.GetWPTerms(siteid, destination_siteid, taxonomy);

            Term term = new Term();

            foreach (DataRow row in dsCategories.Tables[0].Rows)
            {
                term = new Term();
                term.id = Int16.Parse(row["id"].ToString());
                term.count = Int32.Parse(row["count"].ToString());
                term.name = row["name"].ToString();
                term.description = row["description"].ToString();
                term.slug = row["slug"].ToString();
                term.taxonomy = row["taxonomy"].ToString();
                term.parent = Int16.Parse(row["parent"].ToString());
                if ((!terms.Contains(term)))
                    terms.Add(term);
            }
            return terms;
        }

    }
}
