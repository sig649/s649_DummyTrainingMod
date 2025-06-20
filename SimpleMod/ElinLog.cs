
using Debug = UnityEngine.Debug;
//using s649PBR.BIClass;
using System.Collections.Generic;
//using s649PBR.Main;
using s649DPM.PatchMain;
namespace s649ElinLog
{//begin namespaceMain
    public class LogTier
    {
        public const int Fatal = 2;//未使用
        public const int Error = 1;//引数不正やtrycatchで投げられる
        public const int Info = 0;//実行結果出力など
        public const int Deep = -1;//引数の詳細やnullcheckなど
        public const int Other = -2;//動作の確認
        public const int Tweet = -3;//末端のメソッドの呼び出し確認など
    }
    public class ElinLog 
    {
        public static string modmainNS { private get; set; }
        public static int Level_Log { private get; set; }
        private static List<string> stackLog = new List<string> { };
        public static void SetConfig(int lv, string ns)
        {
            Level_Log = lv;
            modmainNS = ns;
        }
        public static void LogDeepTry(bool b, string arg)
        {
            string text = b ? "Success!" : "Failed...";
            LogDeep(text + arg);
        }
        public static void LogDeepTry(bool b)
        {
            string text = b ? "Success!" : "Failed...";
            LogDeep(text);
        }
        public static void LogTweet(string s, int lv)//旧式
        {
            LogTweet(s);
        }
        public static void LogTweet(string s)
        {
            //string text = b ? "Success!" : "Failed...";
            //LogDeepTry(text);
            Log(s, LogTier.Tweet);
        }
        public static void LogOtherTry(bool b)
        {
            string text = b ? "Success!" : "Failed...";
            LogOther(text);
        }
        public static void LogOther(string s, int lv)//旧式
        {
            LogOther(s);
        }
        public static void LogOther(string s)
        {
            //string text = b ? "Success!" : "Failed...";
            //LogDeepTry(text);
            Log(s, LogTier.Other);
        }
        public static void LogDeep(string s, int lv)//旧式
        {
            LogDeep(s);
        }
        public static void LogDeep(string s)
        {
            //string text = b ? "Success!" : "Failed...";
            //LogDeepTry(text);
            Log(s, LogTier.Deep);
        }
        public static void LogInfo(string s, int lv)//旧式
        {
            LogInfo(s);
        }
        public static void LogInfo(string s)
        {
            //string text = b ? "Success!" : "Failed...";
            //LogDeepTry(text);
            Log(s, LogTier.Info);
        }
        public static void LogError(string argText)
        {
            Log(argText, LogTier.Error);
        }
        private static void Log(string arg, int lv)
        {
            //string text = b ? "Success!" : "Failed...";
            //LogDeepTry(text);
            if (Level_Log != Main.CE_LogLevel.Value) { Level_Log = Main.CE_LogLevel.Value; }
            string logHeader = "";
            switch (lv)
            {
                case >=2:
                    logHeader = "[Fatal]";
                    return;
                case 1:
                    logHeader = "[Error]";
                    break;
                case 0:
                    logHeader = "[Info ]";
                    break;
                case -1:
                    logHeader = "[Deep ]";
                    break;
                case -2:
                    logHeader = "[Other]";
                    break;
                default:
                    logHeader = "[Tweet]";
                    break;

            }
            if (lv >= Level_Log)
            {
                Debug.Log(modmainNS + logHeader + string.Join("", stackLog) + arg);
            }
            //Debug.Log(s);
        }
        internal static void LogStack(string argString)
        {
            //メソッドの先頭で呼び出し、ログ用のヘッダーを追加する
            //メソッドの終点でLogStackDumpを呼び出す必要がある
            //※混乱を招くため処理の途中に追加してはいけない
            //末端のメソッドなら呼び出す必要はない
            //ログ処理だけして中身は別メソッド処理に任せるやり方も後の混乱を呼ぶのでだめ。中継メソッドはできるだけ簡素に
            //stackLogLast = stackLog;
            //stackLog += argString;
            stackLog.Add(argString);
        }
        internal static void LogStackDump()
        {
            if (stackLog.Count > 1) { stackLog.RemoveAt(stackLog.Count - 1); }
            //stackLog = stackLogLast;
            //stackLog += argString;
        }
        
        public static void ClearLogStack()
        {   //初期化。HarmonyPatchのpreかpostで、メソッドの頭で必ず呼び出す。
            //preとpostで重複呼び出しはしない事。
            stackLog = new List<string> { };
        }
        public static string StrConv(object input)
        {
            if (input == null) { return "*NULL*"; }
            switch (input) 
            {
                case bool:
                    return ToTF((bool)input);
                case int:
                    return ((int)input).ToString();
                case string:
                    return (string)input;
                case Card:
                    return ((Card)input).NameSimple;
                case Recipe:
                    return ((Recipe)input).GetName();
                default:
                    return input?.ToString() ?? "";
            }
        }

        private static string ToTF(bool b) { return b ? "T" : "F"; }
        /*
        public static string GetStr(bool b)
        {
            return ToTF(b);
        }
        public static string GetStr(int arg)
        {
            return (arg != 0) ? arg.ToString() : "0";
        }
        public static string GetStr(string s)
        {
            return s;
        }
        public static string GetStr(Point arg)
        {
            return (arg != null) ? arg.ToString() : "-";
        }
        public static string GetStr(Trait arg)
        {
            return (arg != null) ? arg.ToString() : "-";
        }
        public static string GetStr(Card arg)
        {
            return (arg != null) ? arg.NameSimple : "-";
        }*/
    }
}//end namespaceMain