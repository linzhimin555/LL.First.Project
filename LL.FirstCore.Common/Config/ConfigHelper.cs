using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.Common.Config
{
    /// <summary>
    /// 读取配置信息帮助类
    /// </summary>
    public class ConfigHelper
    {
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// 默认配置文件夹下读取配置信息
        /// </summary>
        static ConfigHelper()
        {
            Configuration = new ConfigurationBuilder().Add(new JsonConfigurationSource() { Path = "appsettings.json", ReloadOnChange = true }).Build();
        }

        /// <summary>
        /// 是否存在给定节点key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsExistNode(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return Configuration.GetSection(key).Exists();
        }

        /// <summary>
        /// 获取节点value值
        /// </summary>
        /// <param name="key">节点key值(eg:Logging:LogLevel:Default)</param>
        /// <returns></returns>
        public static string GetDefaultJsonValue(string key)
        {
            return Configuration[key];
        }

        /// <summary>
        /// 获取节点value值
        /// </summary>
        /// <param name="sections">各层节点值</param>
        /// <returns></returns>
        public static string GetDefaultJsonValue(params string[] sections)
        {
            if (sections != null && sections.Length > 0)
            {
                return Configuration[string.Join(":", sections)];
            }

            return null;
        }

        #region 从指定文件读取配置信息
        /// <summary>
        /// 获取指定配置文件指定键的值
        /// </summary>
        /// <param name="configPath">指定的json文件路径</param>
        /// <param name="key">节点Key</param>
        /// <returns></returns>
        public static string GetJson(string configPath, string key)
        {
            if (string.IsNullOrEmpty(configPath) || string.IsNullOrEmpty(key))
                return null;

            var config = new ConfigurationBuilder().AddJsonFile(configPath).Build();
            return config.GetSection(key).Value;
        }

        /// <summary>
        /// 获取自定义配置文件配置
        /// </summary>
        /// <typeparam name="T">配置模型</typeparam>
        /// <param name="key">根节点</param>
        /// <param name="configPath">配置文件名称</param>
        /// <returns></returns>
        public T GetAppSetting<T>(string key, string configPath) where T : class, new()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configPath))
                return null;

            IConfiguration config = new ConfigurationBuilder().Add
                (new JsonConfigurationSource { Path = configPath, ReloadOnChange = true }).Build();
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<T>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>()
                .Value;
            return appconfig;
        }

        /// <summary>
        /// 获取自定义配置文件配置（异步方式）
        /// </summary>
        /// <typeparam name="T">配置模型</typeparam>
        /// <param name="key">根节点</param>
        /// <param name="configPath">配置文件名称</param>
        /// <returns></returns>
        public async Task<T> GetAppSettingAsync<T>(string key, string configPath) where T : class, new()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configPath))
                return null;

            IConfiguration config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource
                {
                    Path = configPath,
                    ReloadOnChange = true
                }).Build();
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<T>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>()
                .Value;

            return await Task.Run(() => appconfig);
        }

        /// <summary>
        /// 获取自定义配置文件配置
        /// </summary>
        /// <typeparam name="T">配置模型</typeparam>
        /// <param name="key">根节点</param>
        /// <param name="configPath">配置文件名称</param>
        /// <returns></returns>
        public List<T> GetListAppSettings<T>(string key, string configPath) where T : class, new()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configPath))
                return null;

            IConfiguration config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = configPath, ReloadOnChange = true }).Build();
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<List<T>>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<List<T>>>()
                .Value;

            return appconfig;
        }

        /// <summary>
        /// 获取自定义配置文件配置（异步方式）
        /// </summary>
        /// <typeparam name="T">配置模型</typeparam>
        /// <param name="key">根节点</param>
        /// <param name="configPath">配置文件名称</param>
        /// <returns></returns>
        public async Task<List<T>> GetListAppSettingsAsync<T>(string key, string configPath) where T : class, new()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configPath))
                return null;

            IConfiguration config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = configPath, ReloadOnChange = true })
                .Build();
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<List<T>>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<List<T>>>()
                .Value;

            return await Task.Run(() => appconfig);
        }
        #endregion
    }
}
