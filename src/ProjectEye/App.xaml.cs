﻿using ProjectEye.Core;
using ProjectEye.Core.Models.Options;
using ProjectEye.Core.Service;
using ProjectEye.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace ProjectEye
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        private readonly ServiceCollection serviceCollection;
        private System.Threading.Mutex mutex;
        public delegate void AppEventHandler();
        /// <summary>
        /// 服务初始化完成时发生
        /// </summary>
        public event AppEventHandler OnServiceInitialized;
        public App()
        {
            serviceCollection = new ServiceCollection();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //全局异常捕获
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            //重复运行确认
            ReRunCheck();

            //必须按优先级依次添加
            serviceCollection.AddInstance(this);
            //后台任务
            serviceCollection.Add<BackgroundWorkerService>();
            //数据统计
            serviceCollection.Add<StatisticService>();
            //系统资源
            serviceCollection.Add<SystemResourcesService>();
            //内存缓存
            serviceCollection.Add<CacheService>();
            //配置文件
            serviceCollection.Add<ConfigService>();
            //主题
            serviceCollection.Add<ThemeService>();
            //扩展显示器
            serviceCollection.Add<ScreenService>();
            //主要
            serviceCollection.Add<MainService>();
            //托盘
            serviceCollection.Add<TrayService>();
            //休息
            serviceCollection.Add<ResetService>();
            //声音
            serviceCollection.Add<SoundService>();
            //快捷键
            serviceCollection.Add<KeyboardShortcutsService>();
            //预提醒
            serviceCollection.Add<PreAlertService>();
            
            WindowManager.serviceCollection = serviceCollection;
            //初始化所有服务
            serviceCollection.Initialize();
            OnServiceInitialized?.Invoke();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMsg= "程序发生了不可预料的错误，已经将错误报告保存在程序运行目录Log文件夹下，请将错误内容提供给我们。";
            LogHelper.Error(e.Exception.ToString());
            MessageBox.Show(errorMsg, "错误提示，程序即将退出", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            this.Shutdown();
        }

        #region 重复运行确认
        /// <summary>
        /// 重复运行确认
        /// </summary>
        private void ReRunCheck()
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "projecteye", out ret);
            if (!ret)
            {
#if !DEBUG
                //仅允许运行一次进程
                MessageBox.Show("程序已经在运行中了", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Current.Shutdown();
               
#endif
            }
        }
        #endregion

    }
}
