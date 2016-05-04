using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.DFM.FeedHub.WordPressClient
{
    public class WebClient : System.Net.WebClient
    {
        public System.Net.WebClient New()
        {
            return new System.Net.WebClient();
        }
        public static Boolean isOK(Int32 code)
        {
            if (code >= 200 && code < 300)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean isBad(Int32 code)
        {
            return !isOK(code);
        }
    }
}
