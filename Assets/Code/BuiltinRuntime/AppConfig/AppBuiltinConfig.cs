namespace UGHGame.BuiltinRuntime
{
    /// <summary>
    /// app配置
    /// </summary>
    public partial class AppBuiltinConfig
    {
        /// <summary>
        /// 热更入口类
        /// </summary>
        public static string HotfixEntryClass
        {
            get
            {
                return "UGHGame.HotfixLogic.HotfixEntry";
            }
        }
        /// <summary>
        /// 热更start方法
        /// </summary>
        public static string HotfixStartFuntion
        {
            get
            {
                return "Start";
            }
        }
        /// <summary>
        /// 热更update方法
        /// </summary>
        public static string HotfixUpdate
        {
            get
            {
                return "Update";
            }
        }
        /// <summary>
        /// 热更Shutdown方法
        /// </summary>
        public  static string HotfixShutdown
        {
            get
            {
                return "Shutdown";
            }
        }
        /// <summary>
        /// 热更程序集
        /// </summary>
        public static string HotfixAssembliy
        {
            get
            {
                return "UGHGame.HotfixLogic";
            }
        }

        /// <summary>
        /// 热更配置文件资产名
        /// </summary>
        public static string AppHotfixConfig
        {
            get
            {
                return "AppHotfixConfig";
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        public class Setting
        {
            public const string Language = "Setting.Language";
        }
    }
}
