using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LimbusLocalize
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LCB_LLCMod : BasePlugin
    {
        public static ConfigFile LLC_Settings;
        public static string ModPath;
        public static string GamePath;
        public const string GUID = "Com.Bright.LocalizeLimbusCompany";
        public const string NAME = "LimbusLocalizeMod";
        public const string VERSION = "0.6.5";
        public const string AUTHOR = "Bright&SmallYuan";
        public const string LLCLink = "https://github.com/SmallYuanSY/LocalizeLimbusCompany_TW";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static void OpenLLCURL() => Application.OpenURL(LLCLink);
        public static void OpenGamePath() => Application.OpenURL(GamePath);
        public override void Load()
        {
            LLC_Settings = Config;
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LLC_Manager.FatalErrorlog += log + "\n"; LogError(log); LLC_Manager.FatalErrorAction = action; LLC_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            LLC_UpdateChecker.StartAutoUpdate();
            try
            {
                HarmonyLib.Harmony harmony = new(NAME);
                if (LLC_Chinese_Setting.IsUseChinese.Value)
                {
                    LLC_Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/CN"));
                    harmony.PatchAll(typeof(LCB_Chinese_Font));
                    harmony.PatchAll(typeof(LLC_ReadmeManager));
                    harmony.PatchAll(typeof(LLC_LoadingManager));
                    harmony.PatchAll(typeof(LLC_SpriteUI));
                }
                harmony.PatchAll(typeof(LLC_Manager));
                harmony.PatchAll(typeof(LLC_Chinese_Setting));
                if (!LCB_Chinese_Font.AddChineseFont(ModPath + "/tmpchinesefont"))
                    LogFatalError("You Not Have Chinese Font, Please Read GitHub Readme To Download\n你沒有下載中文字體,請閱讀GitHub的Readme下載", OpenLLCURL);
            }
            catch (Exception e)
            {
                LogFatalError("Mod Has Unknown Fatal Error!!!\n模组部分功能出現致命錯誤,即將打開GitHub,請根據Issues流程反饋", () => { CopyLog(); OpenGamePath(); OpenLLCURL(); });
                LogError(e.ToString());
            }
        }
        public static void CopyLog()
        {
            File.Copy(GamePath + "/BepInEx/LogOutput.log", GamePath + "/Latest(框架日誌).log", true);
            File.Copy(Application.consoleLogPath, GamePath + "/Player(遊戲日誌).log", true);
        }
    }
}
