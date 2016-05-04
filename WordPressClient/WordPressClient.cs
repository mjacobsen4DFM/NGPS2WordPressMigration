using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace com.DFM.FeedHub.WordPressClient
{
    public class WordPressClient
    {
        public String host;
        public String redisType;
        public String redisKey;

        public WordPressClient(String host, String redisType, String redisKey)
        {
            this.host = host;
            this.redisType = redisType;
            this.redisKey = redisKey;
        }

        public Dictionary<string, object> getAll(string brokerEndpoint, string remoteEndpoint)
        {
            Dictionary<string, object> dictResponse = new Dictionary<string, object>();
            List<String> all = new List<String>();
            Int16 pageCount = 0;
            string pageArg = (remoteEndpoint.Contains("?")) ? "&page=" : "?page=";
            try
            {
                Dictionary<string, object> setMap = new Dictionary<string, object>();
                setMap.Add("body", "nothing");

                while ((String)setMap["body"] != "[]")
                {
                    pageCount += 1;
                    setMap = new Dictionary<string, object>();
                    setMap = get(brokerEndpoint, remoteEndpoint + pageArg + pageCount);
                    if (WebClient.isOK((Int32)setMap["code"]))
                    {
                        if ((String)setMap["body"] != "[]")
                        {
                            {
                                String json = (String)setMap["body"];
                                JArray jarray = JArray.Parse(json);

                                foreach (JObject jobject in jarray.Children<JObject>())
                                {
                                    string jsonElement = JsonConvert.SerializeObject(jobject);
                                    all.Add(jsonElement);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception((String)setMap["error"]);
                    }

                }

                String sAll = String.Format("[{0}]", string.Join<string>(",", all));
                dictResponse.Add("code", setMap["code"]);
                dictResponse.Add("result", setMap["result"]);
                dictResponse.Add("body", sAll);

                // Get the response
                //Console.WriteLine("Code {0}, Result {1}", dictResponse["code"], dictResponse["result"]);
                //Console.WriteLine();
                Console.Write(".");
                return dictResponse;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> dictError = new Dictionary<string, object>();
                dictError.Add("code", 600);
                dictError.Add("error", ex.Message);
                Console.Write("!");
                //Console.WriteLine("\nThe following Exception was raised : {0}", ex.Message);
                //Console.WriteLine(ex.ToString());
                return dictError;
            }
        }

        public Dictionary<string, object> get(string brokerEndpoint, string remoteEndpoint)
        {
            try
            {
                // Create a request 
                WebRequest request = WebRequest.Create(this.host + brokerEndpoint);

                //Set the Method property of the request to POST
                request.Method = "GET";

                // Set the ContentType property of the WebRequest
                request.ContentType = "application/json";

                // Set the Redis values in header
                request.Headers.Add("RedisType", this.redisType);
                request.Headers.Add("RedisKey", this.redisKey);
                request.Headers.Add("remoteEndpoint", remoteEndpoint);

                // Get the response
                Dictionary<string, object> dictResponse = ReadResponse(request);
                //Console.WriteLine("Code {0}, Result {1}", dictResponse["code"], dictResponse["result"]);
                //Console.WriteLine();
                Console.Write(".");
                return dictResponse;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> dictError = new Dictionary<string, object>();
                dictError.Add("code", 600);
                dictError.Add("error", ex.Message);
                Console.Write("!");
                //Console.WriteLine("\nThe following Exception was raised : {0}", ex.Message);
                //Console.WriteLine(ex.ToString());
                return dictError;
            }
        }


        public Dictionary<string, object> post(string brokerEndpoint, string remoteEndpoint)
        {
            return post(brokerEndpoint, remoteEndpoint, "{}");
        }

        public Dictionary<string, object> post(string brokerEndpoint, string remoteEndpoint, string json)
        {
            try
            {
                // Create a request 
                WebRequest request = WebRequest.Create(this.host + brokerEndpoint);

                //Set the Method property of the request to POST
                request.Method = "POST";

                // Set the ContentType property of the WebRequest
                request.ContentType = "application/json";

                // Set the Redis values in header
                request.Headers.Add("RedisType", this.redisType);
                request.Headers.Add("RedisKey", this.redisKey);
                request.Headers.Add("remoteEndpoint", remoteEndpoint);

                //Create POST data and convert it to a byte array
                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                // Set the ContentLength property of the WebRequest
                request.ContentLength = byteArray.Length;

                // Write the data to the request stream
                using (BinaryWriter Writer = new BinaryWriter(request.GetRequestStream()))
                {
                    Writer.Write(byteArray);
                }

                // Get the response
                Dictionary<string, object> dictResponse = ReadResponse(request);
                //Console.WriteLine("Code {0}, Result {1}", dictResponse["code"], dictResponse["result"]);
                //Console.WriteLine();
                Console.Write(".");
                return dictResponse;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> dictError = new Dictionary<string, object>();
                dictError.Add("code", 600);
                dictError.Add("error", ex.Message);
                //Console.WriteLine("\nThe following Exception was raised : {0}", ex.Message);
                //Console.WriteLine(ex.ToString());
                return dictError;
            }
        }

        public Dictionary<string, object> delete(string brokerEndpoint, string remoteEndpoint)
        {
            try
            {
                // Create a request 
                WebRequest request = WebRequest.Create(this.host + brokerEndpoint);

                //Set the Method property of the request to POST
                request.Method = "DELETE";

                // Set the ContentType property of the WebRequest
                request.ContentType = "application/json";

                // Set the Redis values in header
                request.Headers.Add("RedisType", this.redisType);
                request.Headers.Add("RedisKey", this.redisKey);
                request.Headers.Add("remoteEndpoint", remoteEndpoint + "?force=true");

                // Get the response
                Dictionary<string, object> dictResponse = ReadResponse(request);
                //Console.WriteLine("Code {0}, Result {1}", dictResponse["code"], dictResponse["result"]);
                //Console.WriteLine();
                Console.Write(".");
                return dictResponse;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> dictError = new Dictionary<string, object>();
                dictError.Add("code", 600);
                dictError.Add("error", ex.Message);
                //Console.WriteLine("\nThe following Exception was raised : {0}", ex.Message);
                //Console.WriteLine(ex.ToString());
                return dictError;
            }
        }

        internal Dictionary<string, object> ReadResponse(WebRequest req)
        {
            Dictionary<string, object> dictWR = new Dictionary<string, object>();
            try
            {
                WebResponse response;
                using (response = req.GetResponse())
                {
                    HttpStatusCode code = ((HttpWebResponse)response).StatusCode;
                    string result = ((HttpWebResponse)response).StatusDescription;
                    string id = ((System.Net.HttpWebResponse)response).Headers["id"];
                    string location = ((System.Net.HttpWebResponse)response).Headers["location"];
                    string body = "";
                    using (StreamReader SReader = new StreamReader(((System.Net.HttpWebResponse)response).GetResponseStream()))
                    {
                        body = SReader.ReadToEnd();
                    }
                    dictWR.Add("code", (int)code);
                    dictWR.Add("result", result);
                    dictWR.Add("id", id);
                    dictWR.Add("location", location);
                    dictWR.Add("body", body);
                }
            }
            catch (WebException ex)
            {
                HttpStatusCode code = ((System.Net.HttpWebResponse)ex.Response).StatusCode;
                string result = ((System.Net.HttpWebResponse)ex.Response).StatusDescription;
                string error = ((System.Net.HttpWebResponse)ex.Response).Headers["error"];
                string body = "";
                using (StreamReader SReader = new StreamReader(((System.Net.HttpWebResponse)ex.Response).GetResponseStream()))
                {
                    body = SReader.ReadToEnd();
                }
                dictWR.Add("code", (int)code);
                dictWR.Add("result", (result != null) ? result : "");
                dictWR.Add("error", (error != null) ? error : "");
                dictWR.Add("body", body);
                //Console.WriteLine("\r\nWebException Raised. The following error occured : {0}", ex.Status);
            }
            catch (Exception ex)
            {
                dictWR.Add("code", 600);
                dictWR.Add("error", ex.Message);
                //Console.WriteLine("\nThe following Exception was raised : {0}", ex.Message);
            }
            return dictWR;
        }
    }
}
