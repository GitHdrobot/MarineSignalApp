using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

//使用Log4jnet库 将日志写入到MemoryApender中

namespace JerryMouse
{
    class LogHelper
    {
        //日志容器
        System.Windows.Forms.ListBox logContainer;
        #region 日志相关定义
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool logWatching = true;
        private log4net.Appender.MemoryAppender logger;
        private Thread logWatcher;
        #endregion

        #region 日志初始化配置
        public void InitLog()
        {

            //配置log appender
            //this.Closing += new CancelEventHandler(Log_Closing);
            logger = new log4net.Appender.MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(logger);
            logWatcher = new Thread(new ThreadStart(LogWatcher));
            logWatcher.Start();

        }
        #endregion


        #region 日志相关处理函数
        void Log_Closing(object sender, CancelEventArgs e)
        {
            logWatching = false;
            logWatcher.Join();

        }
        delegate void AppendLogToCtrl(LoggingEvent logevent);
        void AppendLog(LoggingEvent logevent)
        {
            if (logContainer.InvokeRequired)
            {
                AppendLogToCtrl add = new AppendLogToCtrl(AppendLog);
                logContainer.Invoke(add, new object[] { logevent });
            }
            else
            {
                string _log = "[" + logevent.Level + "]  " + logevent.TimeStamp.ToLocalTime() + ">>  " + logevent.RenderedMessage;
                logContainer.Items.Add(_log);
            }
        }

        private void LogWatcher()
        {
            while (logWatching)
            {
                LoggingEvent[] events = logger.GetEvents();
                if (events != null && events.Length > 0)
                {
                    logger.Clear();
                    foreach (LoggingEvent ev in events)
                    {
                        AppendLog(ev);
                    }
                }
                Thread.Sleep(500);
            }
        }
        #endregion
    }
}
