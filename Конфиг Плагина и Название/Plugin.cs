using Resto.Front.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNG_plugin
{
    class Plugin
    {
        public const string Name = @"TNG_plugin";
        public const string Version = "1.4";
        public static Config Params
        {
            get
            {
                return Config.Instance;
            }
        }



        public static void Log_Info(string mess,object arg)
        {
            PluginContext.Log.InfoFormat(mess, arg);
        }
        public static void Log_Warn(string mess, object arg)
        {
            var text = string.Format(mess, arg);
            PluginContext.Log.Warn(text);
        }
        public static void Log_Error(string mess, object arg)
        {
            var text = string.Format(mess, arg);
            PluginContext.Log.Error(text);
        }
        public static void Log_Mess_Info(string mess, object arg)
        {
            var text = string.Format(mess, arg);
            PluginContext.Log.Info(text);
            PluginContext.Operations.AddNotificationMessage(text, Name, TimeSpan.FromSeconds(10));
        }
        public static void Log_Mess_Warn(string mess, object arg)
        {
            var text = string.Format(mess, arg);
            PluginContext.Log.Warn(text);
            PluginContext.Operations.AddWarningMessage(text, Name, TimeSpan.FromSeconds(10));
        }
        public static void Log_Mess_Error(string mess, object arg)
        {
            var text = string.Format(mess, arg);
            PluginContext.Log.Error(text);
            PluginContext.Operations.AddErrorMessage(text, Name, TimeSpan.FromSeconds(10));
        }


        public static void Log_Info(string mess, bool show = true)
        {
            if (show)
            Log_Info(mess, null);
        }
        public static void Log_Warn(string mess, bool show = true)
        {
            if (show)
            {
                var text = string.Format(mess);
                PluginContext.Log.Warn(text);
            }
        }
        public static void Log_Error(string mess, bool show = true)
        {
            if (show)
            {
                var text = string.Format(mess);
                PluginContext.Log.Error(text);
            }
        }
        public static void Log_Mess_Info(string mess, bool show = true)
        {
            if (show)
            {
                var text = string.Format(mess);
                PluginContext.Log.Info(text);
                PluginContext.Operations.AddNotificationMessage(text, Name, TimeSpan.FromSeconds(10));
            }
        }
        public static void Log_Mess_Warn(string mess, bool show = true)
        {
            if (show)
            {
                var text = string.Format(mess);
                PluginContext.Log.Warn(text);
                PluginContext.Operations.AddWarningMessage(text, Name, TimeSpan.FromSeconds(10));
            }
        }
        public static void Log_Mess_Error(string mess, bool show = true)
        {
            if (show)
            {
                var text = string.Format(mess);
                PluginContext.Log.Error(text);
                PluginContext.Operations.AddErrorMessage(text, Name, TimeSpan.FromSeconds(10));
            }
        }

    }
}
