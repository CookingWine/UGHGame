using GameFramework.Resource;
using System;
using System.Data;
using UGHGame.BuiltinRuntime;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGHGame.HotfixLogic
{
    /// <summary>
    /// 热更入口
    /// </summary>
    public class HotfixEntry
    {
        /// <summary>
        /// 有限状态机
        /// </summary>
        public static FsmManager Fsm
        {
            get;
            private set;
        }

        ///<summary>
        ///流程管理器
        ///</summary>
        public static ProcedureManager Procedure
        {
            get;
            private set;
        }

        /// <summary>
        /// 计时器
        /// </summary>
        public static TimerManager Timer
        {
            get;
            private set;
        }

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager Event
        {
            get;
            private set;
        }

        /// <summary>
        /// 数据表
        /// </summary>
        public static DataTableManager DataTable
        {
            get;
            private set;
        }

        /// <summary>
        /// 应用热更配置
        /// </summary>
        public static AppHotfixConfig AppRuntimeConfig
        {
            get;
            private set;
        }

        /// <summary>
        /// 游戏设置
        /// </summary>
        public static GameSettingsManager GameSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// 加载AOT进度
        /// </summary>
        private static int m_CurrentProcess;
        /// <summary>
        /// 加载元数据完成
        /// </summary>
        private static bool m_LoadMetadataForAOTAssembliesFlage = false;
        /// <summary>
        /// 不可调用,供给HybridclrComponent使用【相当于Mono.Start】
        /// </summary>
        public static void Start( )
        {
            m_CurrentProcess = 0;
            m_LoadMetadataForAOTAssembliesFlage = false;
            LoadAppHotfixConfig( );
        }
        /// <summary>
        /// 不可调用,供给HybridclrComponent使用【相当于Mono.Update】
        /// </summary>
        /// <param name="elapseSeconds"></param>
        /// <param name="realElapseSeconds"></param>
        public static void Update(float elapseSeconds , float realElapseSeconds)
        {
            if(!m_LoadMetadataForAOTAssembliesFlage)
            {
                return;
            }
            Fsm.Update(elapseSeconds , realElapseSeconds);
            Timer.UpdateTimer(elapseSeconds , realElapseSeconds);
            Event.Update(elapseSeconds , realElapseSeconds);
        }

        /// <summary>
        /// 不可调用,供给HybridclrComponent使用
        /// </summary>
        public static void Shutdown( )
        {
            Fsm.Shutdown( );
            Timer.Shotdown( );
            Event.Shutdown( );
            DataTable.Shutdown( );
        }
        private static void LoadAppHotfixConfig( )
        {
            GameCollectionEntry.Resource.LoadAsset(AssetUtility.GetScriptableObjectAsset(AppBuiltinConfig.AppHotfixConfig) ,
             new LoadAssetCallbacks((assetName , asset , duration , userData) =>
            {
                AppRuntimeConfig = asset as AppHotfixConfig;
                if(AppRuntimeConfig == null)
                {
                    Log.Fatal("加载热更配置文件失败");
                    GameCollectionEntry.ShutdownGameFramework(ShutdownType.Quit);
                    return;
                }
                //补充AOT数据
                LoadMetadataForAOTData( );
            } , (assetName , status , errorMessage , userData) =>
            {
                Log.Debug($"Can not load aot dll '{assetName}' error message '{errorMessage}'.");
            }) , AppBuiltinConfig.AppHotfixConfig);
        }

        /// <summary>
        /// 加载AOT元数据
        /// </summary>
        private static void LoadMetadataForAOTData( )
        {
            for(int i = 0; i < AppRuntimeConfig.AotFileList.Length; i++)
            {
                GameCollectionEntry.Resource.LoadAsset(AssetUtility.GetAotMetadataAsset(AppRuntimeConfig.AotFileList[i]) , new LoadAssetCallbacks((assetName , asset , duration , userData) =>
                {
                    byte[] bytes = ( asset as TextAsset ).bytes;
                    bool state = GameCollectionEntry.Hybridclr.LoadMetadataForAOTAssembly(bytes);
                    if(state)
                    {
                        m_CurrentProcess++;
                    }
                    Log.Debug($"LoadMetadataForAOTAssembly:{userData}.Load state:{state}");
                    //等待AOT数据加载完毕后，再去初始化组件数据
                    if(m_CurrentProcess == AppRuntimeConfig.AotFileList.Length)
                    {
                        //初始化组件
                        LoadingHotSwappingComponents( );
                    }
                } , (assetName , status , errorMessage , userData) => { Log.Fatal($"Can not load aot dll '{assetName}' error message '{errorMessage}'."); }) , AppRuntimeConfig.AotFileList[i]);
            }
        }

        /// <summary>
        /// 初始化加载组件
        /// </summary>
        private static void LoadingHotSwappingComponents( )
        {
            Fsm = new FsmManager( );
            Timer = new TimerManager( );
            Event = new EventManager( );
            DataTable = new DataTableManager( );
            Procedure = new ProcedureManager( );
            GameSettings = GameSettingsManager.LoadGameSettings( );
            Procedure.Initialize(Fsm , GetHotfixGameProduce( ));
            Procedure.StartProcedure<ProcedureHotfixEntry>( );
            m_LoadMetadataForAOTAssembliesFlage = true;
        }

        /// <summary>
        /// 获取热更游戏的流程
        /// </summary>
        /// <returns></returns>
        private static ProcedureBase[] GetHotfixGameProduce( )
        {
            ProcedureBase[] procedures = new ProcedureBase[AppRuntimeConfig.HotfixProcedure.Length];
            for(int i = 0; i < AppRuntimeConfig.HotfixProcedure.Length; i++)
            {
                Type t = Type.GetType(AppRuntimeConfig.HotfixProcedure[i]);
                if(t == null)
                {
                    Log.Fatal("无法获取{0}类型" , AppRuntimeConfig.HotfixProcedure[i]);
                    continue;
                }
                procedures[i] = Activator.CreateInstance(t) as ProcedureBase;
            }
            return procedures;
        }
    }
}
