 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace OWC
{
  public class rest
    {

        enum asset
        {
            binary = 101,
            text = 102
        }


        public bool ImageExists(string address)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
            request.Method = "HEAD";
            request.Timeout = 1000 * 10;
            bool exists;
            try
            {
                request.GetResponse();
                exists = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                exists = false;
            }
            return exists;
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = (60 * 1000) * 5 ;
                return w;
            }
        }


        public byte[] getFile(Uri address,ref string message)
        {
            byte[] data= null;
            try
            {
                MyWebClient wc = new MyWebClient();
                
                data = wc.DownloadData(address);
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return data;
        }

        public byte[] getFile(Uri address, string username, string password,ref string message)
        {
            byte[] data = null;
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                data = wc.DownloadData(address);
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return data;
        }

        

        public string postImage(Uri address, string username, string password, ref string message, byte[] binaryimage )
        {
            string location = "";
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                byte[] bret = wc.UploadData(address, "POST", binaryimage); 
                string sret = System.Text.Encoding.ASCII.GetString(bret);
                location = wc.ResponseHeaders["Location"].ToString();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return location;
        } // postImage


        public string executeDelete(string address, string username, string password, ref string message)
        {
            string statuscode = "";
            StringBuilder mymessage = new StringBuilder();
            try
            {
                WebRequest request = WebRequest.Create(address);
                request.Credentials = new System.Net.NetworkCredential(username, password);
                request.Method = "DELETE";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                statuscode = response.StatusCode.ToString();
            }
            catch (WebException ex)
            {
                mymessage.Append(ex.Status.ToString());
                if (ex.Response != null)
                {
                    // can use ex.Response.Status, .StatusDescription
                    if (ex.Response.ContentLength != 0)
                    {
                        using (var stream = ex.Response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                mymessage.Append( reader.ReadToEnd());
                            }
                        }
                    }
                }  // ex.Response    
            }
            message = mymessage.ToString();
            return statuscode;
        }


        public string executeCMD(string cmd, string address, string username, string password, string strData, ref string message)
        {
            string location = address;
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                byte[] bret = wc.UploadData(new Uri(address), cmd, System.Text.Encoding.ASCII.GetBytes(strData));
                if (cmd.Equals("POST"))
                {
                    string sret = System.Text.Encoding.ASCII.GetString(bret);
                    location = wc.ResponseHeaders["Location"].ToString();
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return location;
        } // postStory




        public string postPDF(string address, string username, string password, byte[] Data, ref string message)
        {
            string location = address;
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                byte[] bret = wc.UploadData(new Uri(address), "POST", Data);
                string sret = System.Text.Encoding.ASCII.GetString(bret);
                location = wc.ResponseHeaders["Location"].ToString();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return location;
        } // postStory




        public string putStory(string  address, string username, string password, string strData, ref string message)
        {
            string location = address;
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                byte[] bret = wc.UploadData(new Uri(address),"PUT", System.Text.Encoding.ASCII.GetBytes(strData));
  //                string sret = System.Text.Encoding.ASCII.GetString(bret);
  //              location = wc.ResponseHeaders["Location"].ToString();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return location;
        } // postStory


        public string postStory(Uri address, string username, string password, string strData ,ref string message)
        {
            string location = "";
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = new System.Net.NetworkCredential(username, password);
                byte[] bret = wc.UploadData(address, "POST", System.Text.Encoding.ASCII.GetBytes(strData));
                string sret = System.Text.Encoding.ASCII.GetString(bret);
                location = wc.ResponseHeaders["Location"].ToString();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return location;
        } // postStory



        //public string pushfile(Uri address, string username, string password, string FilePath, int mytype, string mainImagePath ,ref string message)
        //{
        //    string location = "";
        //    try
        //    {
        //        WebClient wc = new WebClient();
        //        wc.Credentials = new System.Net.NetworkCredential("pbruni", "m5trUbRE");

        //        if (mytype == (int)asset.binary)
        //        {
        //            FileStream fs = new FileStream(FilePath, FileMode.Open);
        //            BinaryReader br = new BinaryReader(fs);
        //            byte[] binaryfile = br.ReadBytes((int)br.BaseStream.Length);    // ReadBytes(br.BaseStream.Length());
        //            br.Close();
        //            byte[] bret = wc.UploadData(address, "POST", binaryfile); //System.Text.Encoding.ASCII.GetBytes( strData));
        //            string sret = System.Text.Encoding.ASCII.GetString(bret);
        //        }
        //        else
        //        {
        //            StreamReader sr = new StreamReader(FilePath);
        //            string strData = sr.ReadToEnd();
        //            sr.Close();
        //            strData = strData.Replace("#urimain#", mainImagePath);
        //            byte[] bret = wc.UploadData(address, "POST", System.Text.Encoding.ASCII.GetBytes(strData));
        //            string sret = System.Text.Encoding.ASCII.GetString(bret);
        //        }

        //        location = wc.ResponseHeaders["Location"].ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        message = e.Message;
        //    }
        //    return location;
        //}

    }
}
