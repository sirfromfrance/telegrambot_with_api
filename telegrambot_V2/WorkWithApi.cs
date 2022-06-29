using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using telegram_bot_for_Valera;

namespace telegrambot_V2
{
    internal class WorkWithApi
    {
         public  WordItem GetFromApi(string messageText)
        {
            string[] word_id = messageText.Split(" ", 2);
            string WEBSERVICE_URL = "https://localhost:44326/api/ItemContoller/";
            var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL + word_id[0]);
            webRequest.Method = "GET";
            webRequest.Timeout = 12000;
            try
            {
                using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var jsonResponse = sr.ReadToEnd();
                        var item = JsonConvert.DeserializeObject<WordItem>(jsonResponse);
                        return item;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("not today");
                return null;
            }
        }

        public void ToCollection (string messageText, string name_of_collection)
        {
            string[] word_id = messageText.Split(" ", 2);
            string WEBSERVICE_URL = $"https://localhost:44326/api/ItemContoller/toCollection?word={messageText}&name_of_collection={name_of_collection}";
            var webRequest = WebRequest.Create(WEBSERVICE_URL);
            webRequest.Method = "POST";
            webRequest.Timeout = 12000;
            WebResponse response = webRequest.GetResponse();
        }


        public List<string> GetCollection (string name_of_collection)
        {
            List<string> collection = new List<string>();
            string WEBSERVICE_URL = $"https://localhost:44326/api/ItemContoller/getCollection?name_of_collection={name_of_collection}";
            var webRequest = WebRequest.Create(WEBSERVICE_URL);
            webRequest.Method = "GET";
            webRequest.Timeout = 12000;
            try
            {
                using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var jsonResponse = sr.ReadToEnd();
                        var item = JsonConvert.DeserializeObject<List<Word>>(jsonResponse);
                        foreach (var tmp in item)
                        {
                            collection.Add(tmp.word);
                        }
                        return collection;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("not today");
                return null;
            }



        }
    }
}
            