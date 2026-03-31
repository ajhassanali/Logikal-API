using Ofcas.Lk.Api.Client.Demo.Models;
using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class EnvironmentSettings
    {
        private const string FileName = "user.config";

        private static readonly string _userConfigPath;

        static EnvironmentSettings()
        {
            var userConfigPath = ConfigurationManager.AppSettings["UserConfigPath"];
            var localApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(userConfigPath))
                userConfigPath = localApplicationDataPath;
            else
                userConfigPath = userConfigPath.Replace(@"%localappdata%", localApplicationDataPath);

            _userConfigPath = Path.Combine(userConfigPath, FileName);
        }

        public static EnvironmentModel Load()
        {
            if (!File.Exists(_userConfigPath))
            {
                return new EnvironmentModel();
            }

            var xmlSerializer = new XmlSerializer(typeof(EnvironmentModel));
            using (var streamReader = new StreamReader(_userConfigPath))
            {
                return (EnvironmentModel)xmlSerializer.Deserialize(streamReader);
            }
        }

        public static void Save(EnvironmentModel environmentModel)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_userConfigPath));

            var xmlSerializer = new XmlSerializer(typeof(EnvironmentModel));
            using (var streamWriter = new StreamWriter(_userConfigPath))
            {
                xmlSerializer.Serialize(streamWriter, environmentModel);
            }
        }

        public static void Clear()
        {
            if (File.Exists(_userConfigPath))
                File.Delete(_userConfigPath);
        }
    }
}