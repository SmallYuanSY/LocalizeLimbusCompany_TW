﻿using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalize
{
    public static class LLC_UpdateChecker
    {
        public static ConfigEntry<bool> AutoUpdate = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "AutoUpdate", false, "是否自动检查并下载更新 ( true | false )");
        public static ConfigEntry<URI> UpdateURI = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "UpdateURI", URI.GitHub, "自动更新所使用URI ( GitHub:默认 | Mirror_OneDrive:镜像,更新可能有延迟,但下载速度更快 )");
        public static void StartAutoUpdate()
        {
            if (AutoUpdate.Value)
            {
                LCB_LLCMod.LogWarning($"Check Mod Update From {UpdateURI.Value}");
                Action ModUpdate = CheckModUpdate;
                new Thread(ModUpdate).Start();
            }
        }
        static void CheckModUpdate()
        {
            string release_uri = "https://api.github.com/repos/SmallYuanSY/LocalizeLimbusCompany_TW/releases/latest";
            UnityWebRequest www = UnityWebRequest.Get(release_uri);
            www.timeout = 4;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            if (www.result != UnityWebRequest.Result.Success)
                LCB_LLCMod.LogWarning($"Can't access {UpdateURI.Value}!!!" + www.error);
            else
            {
                var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
                string latestReleaseTag = latest["tag_name"].Value;
                if (Version.Parse(LCB_LLCMod.VERSION) < Version.Parse(latestReleaseTag.Remove(0, 1)))
                {
                    string updatelog = "LimbusLocalize_BIE_" + latestReleaseTag;
                    Updatelog += updatelog + ".7z ";
                    string download_uri = "https://github.com/SmallYuanSY/LocalizeLimbusCompany_TW/releases/download/{latestReleaseTag}/{updatelog}.7z";
                    var dirs = download_uri.Split('/');
                    string filename = LCB_LLCMod.GamePath + "/" + dirs[^1];
                    if (!File.Exists(filename))
                        DownloadFileAsync(download_uri, filename);
                    UpdateCall = UpdateDel;
                }
                LCB_LLCMod.LogWarning("Check Chinese Font Asset Update");
                Action FontAssetUpdate = CheckChineseFontAssetUpdate;
                new Thread(FontAssetUpdate).Start();
            }
        }
        static void CheckChineseFontAssetUpdate()
        {
            string release_uri = "https://api.github.com/repos/SmallYuanSY/LLC_ChineseFontAsset/releases/latest";
            UnityWebRequest www = UnityWebRequest.Get(release_uri);
            string FilePath = LCB_LLCMod.ModPath + "/tmpchinesefont";
            var LastWriteTime = File.Exists(FilePath) ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(FilePath).LastWriteTime, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd")) : 0;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
            int latestReleaseTag = int.Parse(latest["tag_name"].Value);
            if (LastWriteTime < latestReleaseTag)
            {
                string updatelog = "tmpchinesefont_BIE_" + latestReleaseTag;
                Updatelog += updatelog + ".7z ";
                string download = "https://github.com/SmallYuanSY/LLC_ChineseFontAsset/releases/download/{latestReleaseTag}/{updatelog}.7z";
                var dirs = download.Split('/');
                string filename = LCB_LLCMod.GamePath + "/" + dirs[^1];
                if (!File.Exists(filename))
                    DownloadFileAsync(download, filename);
                UpdateCall = UpdateDel;
            }
        }
        static void UpdateDel()
        {
            LCB_LLCMod.OpenGamePath();
            Application.Quit();
        }
        static void DownloadFileAsync(string uri, string filePath)
        {
            try
            {
                LCB_LLCMod.LogWarning("Download " + uri + " To " + filePath);
                using HttpClient client = new();
                using HttpResponseMessage response = client.GetAsync(uri).GetAwaiter().GetResult();
                using HttpContent content = response.Content;
                using FileStream fileStream = new(filePath, FileMode.Create);
                content.CopyToAsync(fileStream).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException httpException && httpException.StatusCode == HttpStatusCode.NotFound)
                    LCB_LLCMod.LogWarning($"{uri} 404 NotFound,No Resource");
                else
                    LCB_LLCMod.LogWarning($"{uri} Error!!!" + ex.ToString());
            }
        }
        public static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://smallyuansy.github.io/LocalizeLimbusCompany_TW/LatestUpdateTime.txt");
            www.timeout = 1;
            www.SendWebRequest();
            string FilePath = LCB_LLCMod.ModPath + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result == UnityWebRequest.Result.Success && LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get("https://SmallYuanSY.github.io/LocalizeLimbusCompany_TW/Readme.json");
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                File.WriteAllText(FilePath, www2.downloadHandler.text);
                LLC_ReadmeManager.InitReadmeList();
            }
        }
        public static string Updatelog;
        public static Action UpdateCall;
        public enum URI
        {
            GitHub,
            Mirror_OneDrive
        }
    }
}