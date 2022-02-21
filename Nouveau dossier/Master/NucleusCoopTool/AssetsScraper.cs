using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop.Generic
{
    
    class AssetsScraper  //Download game covers & screenshots from igdb through the hub api.
    {      
        public void SaveCovers(string urls, string name)
        {
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, $@"gui\covers\" + name + ".jpeg")))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] data = webClient.DownloadData(urls);

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            using (Image newImage = Image.FromStream(mem))
                            {
                                newImage.Save(Path.Combine(Application.StartupPath, $@"gui\covers\" + name + ".jpeg"), ImageFormat.Jpeg);
                            }
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            { }
        }

        private int max;
        public void SaveScreenshots(string json, string gameName)
        {
            try
            {
                dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(json);

                if (jsonData.screenshots.Count < 5)// <= if there is less than 5 screenshots available in the igdb's database
                {
                    max = jsonData.screenshots.Count;
                }
                else 
                {
                    max = 5;
                }

                for (int i = 0; i < max; i++)//jsonData.screenshots.Count; i++) <= we don't want to download all screenshots available in the igdb's database
                {
                    if (!File.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameName + "\\" + i + "_" + gameName + ".jpeg")))
                    {
                        string url = "https:" + jsonData.screenshots[i].url;
                        string newurl = url.Replace("t_thumb", "t_original");

                        using (WebClient webClient = new WebClient())
                        {
                            byte[] data = webClient.DownloadData(newurl);
                            using (MemoryStream mem = new MemoryStream(data))
                            {
                                using (Image newImage = Image.FromStream(mem))
                                {
                                    if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameName)))

                                    {
                                        Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameName)));
                                    }

                                    newImage.Save(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameName + "\\" + i + "_" + gameName + ".jpeg"), ImageFormat.Jpeg);

                                    if (i == max) // jsonData.screenshots.Count)<= reset when "max" value is reach
                                    {
                                        i = 0;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                    }

                }
            }
            catch (Exception)
            { }
        }
    }
}
