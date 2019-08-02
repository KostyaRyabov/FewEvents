using System;
using System.Threading;

using System.IO;
using System.Net;
using Newtonsoft.Json;

using System.Text.RegularExpressions;


namespace FewEvents
{
    public class BD
    {
        public SemaphoreSlim rec_signal = new SemaphoreSlim(0, 1),
            city_signal = new SemaphoreSlim(0, 1),
            locale_signal = new SemaphoreSlim(0, 1);
        public int page = 0,
            id = 1,
            total = 0;
        public short len = 0,
            currantFrame = 0;
        public string city = "null",
            error_message = "",
            path,
            category = "",
            tag = "";
        public bool record_state = true,
            city_state = true, 
            locale_state = true, 
            PorE = true;

        private Thread thread;

        private LOCALE loc = null;
        public Record record = null;
        private GEO geo;
        
        private string requestStr;




        public BD()
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                string result;

                if (File.Exists(path + "\\FewEvents\\geo.json"))
                {
                    currantFrame = 3;

                    using (StreamReader reader = new StreamReader(path + "\\FewEvents\\geo.json"))
                    {
                        result = reader.ReadToEnd();
                    }

                    if (result.Length > 30)
                    {
                        geo = JsonConvert.DeserializeObject<GEO>(result);
                        city = geo.location.data.city;
                    }
                    else
                    {
                        geo = null;
                        city = "";
                    }
                }
                else
                {
                    currantFrame = 1;

                    GetCity();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("currantFrame = " + currantFrame);
        }                           //инициализация - первый запуск? -> определение геолокации(getCity())

        public void GetRecord()                                                                             ////////////////////////////////////////////////////
        {
            if (record_state)
            {
                thread = new Thread(FunkRecord);
                thread.Start();
            }
        }           //обращение к таблице с запросом
        public void GetCity()
        {
            if (city_state)
            {
                thread = new Thread(FunkCity);
                thread.Start();
            }
        }
        public void GetLocale()
        {
            Console.WriteLine("-------" + locale_state);

            if (locale_state)
            {
                thread = new Thread(FunkLocales);
                thread.Start();
            }
        }
        public void Restore()
        {
            page = 0;
        }
        public void PrevPage()
        {
            page--;
        }
        public void NextPage()
        {
            page++;
        }

        private void FunkRecord()
        {
            record_state = false;
            
            if (PorE) requestStr = "https://all.culture.ru/api/2.2/places?locales=" + id + ((category.Length > 0)?"&categories="+category:"") + ((tag.Length > 0) ? "&tags=" + tag : "") + "&limit=8&offset=" + page* 8 + "&status=accepted&sort=-createDate";
            else requestStr = "https://all.culture.ru/api/2.2/events?locales=" + id + ((category.Length > 0) ? "&categories=" + category : "") + ((tag.Length > 0) ? "&tags=" + tag : "") + "&limit=8&offset=" + page*8 + "&status=accepted&start=" + (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + "&sort=-createDate";

            Console.WriteLine(requestStr);

            HttpWebRequest request = HttpWebRequest.CreateHttp(requestStr);
            
            request.ContentType = "application/json";
            request.Timeout = 10000;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s = reader.ReadToEnd();

                        if (!PorE) s = Regex.Replace(s, "events", "places");

                        record = JsonConvert.DeserializeObject<Record>(s);


                        len = (short)record.places.Length;
                        total = record.total;


                        Console.WriteLine("len2 " + len);
                    }
                }

                error_message = "";
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                record = null;

                len = 0;
                total = 0;

                error_message = e.ToString();
            }

            rec_signal.Release();
            record_state = true;
        }
        private void FunkLocales()
        {
            locale_state = false;
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp("https://all.culture.ru/api/2.2/locales?nameQuery=" + city + "&fields=_id,name&limit=1");

                request.ContentType = "application/json";
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();

                        loc = JsonConvert.DeserializeObject<LOCALE>(result);
                        if (loc.locales.Length > 0)
                        {
                            city = loc.locales[0].name;
                            id = loc.locales[0]._id;

                            error_message = "";
                        }
                        else
                        {
                            id = 0;
                            error_message = "город не найден";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error_message = e.ToString();
                id = 0;
            }

            locale_signal.Release();
            locale_state = true;
        }
        private void FunkCity()
        {
            city_state = false;

            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp("https://suggestions.dadata.ru/suggestions/api/4_1/rs/iplocate/address?ip=" + new WebClient().DownloadString("https://api.ipify.org"));

                request.Headers.Add("Authorization", "Token cfe33a01e55f79da22eec574e2074934b9507bd8");
                request.ContentType = "application/json";
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();

                        if (!Directory.Exists(path + "\\FewEvents"))
                            Directory.CreateDirectory(path + "\\FewEvents");

                        using (StreamWriter outputFile = new StreamWriter(path + "\\FewEvents\\geo.json")) outputFile.WriteLine(result);

                        if (result.Length > 30)
                        {
                            geo = JsonConvert.DeserializeObject<GEO>(result);
                            city = geo.location.data.city;
                        }
                        else city = "";
                    }
                }

                if (city.Equals("")) id = 1;
                else
                {
                    FunkLocales();
                    locale_signal.Wait();
                }

                error_message = "";
            }
            catch (WebException e)
            {
                city = "";

                id = 0;

                error_message = e.ToString();

                if (!Directory.Exists(path + "\\FewEvents"))
                    Directory.CreateDirectory(path + "\\FewEvents");

                using (StreamWriter outputFile = new StreamWriter(path + "\\FewEvents\\geo.json")) outputFile.WriteLine("{\"location\": null}");
            }

            Console.WriteLine("city = " + city);

            city_signal.Release();
            city_state = true;
        }
    }
}
