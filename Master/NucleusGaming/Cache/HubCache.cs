using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nucleus.Gaming.Cache
{
    public static class HubCache
    {
        private static string cacheFile = Path.Combine(Application.StartupPath, $"webview\\cache\\hubcache");
        private static string thumbnailFolder = Path.Combine(Application.StartupPath, $"webview\\cache\\thumbnails\\");
        private const string api = "https://hub.splitscreen.me/api/v1/";

        private static JObject cacheObject;
        private static JArray handlersArray;
        public static int CachedHandlersCount => handlersArray.Count;
        private static JArray searchHandlers = new JArray();

        private static List<Bitmap> thumbnails = new List<Bitmap>();

        public static JArray InitCache()
        {
            if (!File.Exists(cacheFile) || CheckCacheUpdate())
            {
                return CreateCache();
            }

            string cache = File.ReadAllText(cacheFile);
            cacheObject = JsonConvert.DeserializeObject(cache) as JObject;
            handlersArray = cacheObject["Handlers"] as JArray;

            return handlersArray;
        }

        private static JArray CreateCache()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(api + "allhandlers");
            request.UserAgent = "request";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())

            using (StreamReader reader = new StreamReader(stream))
            {
                using (FileStream cachestream = new FileStream(cacheFile, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(cachestream))
                    {
                        JObject cacheObj = JObject.Parse(reader.ReadToEnd());

                        string jsonCache = JsonConvert.SerializeObject(cacheObj, Formatting.Indented);
                        writer.Write(jsonCache);
                        stream.Flush();

                        cacheObject = cacheObj;
                        handlersArray = cacheObject["Handlers"] as JArray;
                        return handlersArray;
                    }
                }
            }
        }

        private static bool CheckCacheUpdate()
        {
            if (!File.Exists(cacheFile))
            {
                return false;
            }

            DateTime lastWritten = File.GetLastWriteTime(cacheFile);
            DateTime now = DateTime.Now;

            if (now.Month == lastWritten.Month)
            {
                if (now.Day >= lastWritten.Day + 7)//chech every 7 days or more
                {
                    return true;
                }
            }

            if (now.Month > lastWritten.Month)
            {
                return true;
            }

            if (now.Month < lastWritten.Month)//new year since last update
            {
                if ((now.Day >= 7 && now.Month == 1) || now.Month > 1)
                {
                    return true;
                }
            }

            return false;
        }

        public static Handler SearchById(string handlerId)
        {
            if (cacheObject == null)
            {
                InitCache();
            }

            for (int i = 0; i < handlersArray.Count; i++)
            {
                string id = handlersArray[i]["_id"].ToString();

                if (id.Equals(handlerId, StringComparison.OrdinalIgnoreCase))
                {
                    Handler handler = new Handler
                    {
                        Id = handlersArray[i]["_id"].ToString(),
                        Owner = handlersArray[i]["owner"].ToString(),
                        OwnerName = handlersArray[i]["ownerName"].ToString(),
                        Description = handlersArray[i]["description"].ToString(),
                        Title = handlersArray[i]["title"].ToString(),
                        GameName = handlersArray[i]["gameName"].ToString(),
                        GameDescription = handlersArray[i]["gameDescription"].ToString(),
                        GameCover = handlersArray[i]["gameCover"].ToString(),
                        GameId = handlersArray[i]["gameId"].ToString(),
                        GameUrl = handlersArray[i]["gameUrl"].ToString(),
                        CreatedAt = handlersArray[i]["createdAt"].ToString(),
                        UpdatedAt = handlersArray[i]["updatedAt"].ToString(),
                        Stars = handlersArray[i]["stars"].ToString(),
                        DownloadCount = handlersArray[i]["downloadCount"].ToString(),
                        Verified = handlersArray[i]["verified"].ToString(),
                        Private = handlersArray[i]["private"].ToString(),
                        CommentCount = handlersArray[i]["commentCount"].ToString(),
                        CurrentVersion = handlersArray[i]["currentVersion"].ToString(),
                        CurrentPackage = handlersArray[i]["currentPackage"].ToString(),
                    };

                    return handler;
                }
            }

            return SearchByIdOnline(handlerId);
        }

        private static Handler SearchByIdOnline(string handlerId)
        {
            string uri = api + "handler/" + handlerId;

            string hubResp = Get(uri);

            if (hubResp == null || hubResp == "{}")
            {
                return null;
            }

            JObject serverDatas = JsonConvert.DeserializeObject(hubResp) as JObject;
            searchHandlers = serverDatas["Handlers"] as JArray;

            if (searchHandlers.Count() > 0)
            {
                Handler handler = null;

                for (int i = 0; i < searchHandlers.Count(); i++)
                {
                    if (handlersArray.All(h => h["_id"] != searchHandlers[i]))//Update cache from hub resp because those handlers were not in cache
                    {
                        cacheObject["Handlers"][0].AddBeforeSelf(searchHandlers[i]);

                        handler = new Handler
                        {
                            Id = searchHandlers[i]["_id"].ToString(),
                            Owner = searchHandlers[i]["owner"].ToString(),
                            OwnerName = searchHandlers[i]["ownerName"].ToString(),
                            Description = searchHandlers[i]["description"].ToString(),
                            Title = searchHandlers[i]["title"].ToString(),
                            GameName = searchHandlers[i]["gameName"].ToString(),
                            GameDescription = searchHandlers[i]["gameDescription"].ToString(),
                            GameCover = searchHandlers[i]["gameCover"].ToString(),
                            GameId = searchHandlers[i]["gameId"].ToString(),
                            GameUrl = searchHandlers[i]["gameUrl"].ToString(),
                            CreatedAt = searchHandlers[i]["createdAt"].ToString(),
                            UpdatedAt = searchHandlers[i]["updatedAt"].ToString(),
                            Stars = searchHandlers[i]["stars"].ToString(),
                            DownloadCount = searchHandlers[i]["downloadCount"].ToString(),
                            Verified = searchHandlers[i]["verified"].ToString(),
                            Private = searchHandlers[i]["private"].ToString(),
                            CommentCount = searchHandlers[i]["commentCount"].ToString(),
                            CurrentVersion = searchHandlers[i]["currentVersion"].ToString(),
                            CurrentPackage = searchHandlers[i]["currentPackage"].ToString(),
                        };
                    }
                }

                using (FileStream stream = new FileStream(cacheFile, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(cacheObject, Formatting.Indented);
                        writer.Write(json);
                        stream.Flush();
                    }
                }

                return handler;
            }

            return null;
        }

        public static List<Handler> GetAllHandlers()
        {
            if (cacheObject == null)
            {
                InitCache();
            }

            List<Handler> handlers = new List<Handler>();

            for (int i = 0; i < handlersArray.Count; i++)
            {
                Handler handler = new Handler
                {
                    Id = handlersArray[i]["_id"].ToString(),
                    Owner = handlersArray[i]["owner"].ToString(),
                    OwnerName = handlersArray[i]["ownerName"].ToString(),
                    Description = handlersArray[i]["description"].ToString(),
                    Title = handlersArray[i]["title"].ToString(),
                    GameName = handlersArray[i]["gameName"].ToString(),
                    GameDescription = handlersArray[i]["gameDescription"].ToString(),
                    GameCover = handlersArray[i]["gameCover"].ToString(),
                    GameId = handlersArray[i]["gameId"].ToString(),
                    GameUrl = handlersArray[i]["gameUrl"].ToString(),
                    CreatedAt = handlersArray[i]["createdAt"].ToString(),
                    UpdatedAt = handlersArray[i]["updatedAt"].ToString(),
                    Stars = handlersArray[i]["stars"].ToString(),
                    DownloadCount = handlersArray[i]["downloadCount"].ToString(),
                    Verified = handlersArray[i]["verified"].ToString(),
                    Private = handlersArray[i]["private"].ToString(),
                    CommentCount = handlersArray[i]["commentCount"].ToString(),
                    CurrentVersion = handlersArray[i]["currentVersion"].ToString(),
                    CurrentPackage = handlersArray[i]["currentPackage"].ToString(),
                };

                handlers.Add(handler);
            }

            return handlers;
        }

        public static string GetScreenshotsUri(string id)
        {
            if (id == null) { return null; }
            if (id == "{}" || id == "") { return null; }
            string resp = Get($@"https://hub.splitscreen.me/api/v1/screenshots/{id}");
            return resp;
        }

        public static JArray SearchByName(string searchParam)
        {
            if (cacheObject == null)
            {
                InitCache();
            }

            searchHandlers?.Clear();

            for (int i = 0; i < handlersArray.Count; i++)
            {
                string gameName = handlersArray[i]["gameName"].ToString();

                if (gameName.StartsWith(searchParam, StringComparison.OrdinalIgnoreCase))
                {
                    searchHandlers.Add(handlersArray[i]);

                    if (i < handlersArray.Count - 1)
                    {
                        i++;
                    }
                }
            }

            if (searchHandlers.Count > 0)
            {
                return searchHandlers;
            }

            searchParam = Uri.EscapeDataString(searchParam);
            return SearchByNameOnline(searchParam);
        }

        private static JArray SearchByNameOnline(string searchParam)//No results found in the cache
        {
            string uri = api + "handlers/" + searchParam;

            string hubResp = Get(uri);

            if (hubResp == null || hubResp == "{}")
            {
                return null;
            }

            JObject serverDatas = JsonConvert.DeserializeObject(hubResp) as JObject;
            searchHandlers = serverDatas["Handlers"] as JArray;

            List<string> newFound = new List<string>();

            if (searchHandlers.Count() > 0)
            {
                for (int i = 0; i < searchHandlers.Count(); i++)
                {
                    if (handlersArray.All(h => h["gameName"] != searchHandlers[i]))//Update cache from hub resp because those handlers were not in cache
                    {
                        cacheObject["Handlers"][0].AddBeforeSelf(searchHandlers[i]);
                    }
                }

                using (FileStream stream = new FileStream(cacheFile, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(cacheObject, Formatting.Indented);
                        writer.Write(json);
                        stream.Flush();
                    }
                }

                return searchHandlers;
            }

            return null;
        }

        public static Bitmap ThumbnailsCache(Handler handler)
        {
            try
            {
                Regex pattern = new Regex("[\\/:*?\"<>|]");

                string fileName = pattern.Replace(handler.Id.ToLower(), "");

                if (!File.Exists(thumbnailFolder + fileName + ".jpeg"))
                {
                    string _cover = $@"https://images.igdb.com/igdb/image/upload/t_micro/{handler.GameCover}.jpg";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_cover);
                    request.UserAgent = "request";
                    WebResponse resp = request.GetResponse();
                    Stream respStream = resp.GetResponseStream();

                    if (!Directory.Exists(thumbnailFolder))
                    {
                        Directory.CreateDirectory(thumbnailFolder);
                    }

                    using (Image newImage = Image.FromStream(respStream))
                    {
                        newImage.Save(thumbnailFolder + fileName + ".jpeg", ImageFormat.Jpeg);
                    }

                    respStream.Dispose();

                    return new Bitmap(thumbnailFolder + fileName + ".jpeg");
                }
                else
                {
                    return new Bitmap(thumbnailFolder + fileName + ".jpeg");
                }
            }
            catch (Exception)
            {
                return ImageCache.GetImage(Globals.ThemeFolder + "no_cover.png");
            }
        }

        public static string Get(string uri)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "request";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
