using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Reflection;
using viwik.EnglishLearning.Data;

namespace EnglishLearning.Business.Manager
{
    public class DataFileManager : FileManager
    {
        private readonly static string dataFileName = "english.db3";
        private readonly static string dbVersionFileName = "DbVersion.txt";
        private static string folderName => "data";

        public static string DataFolder => Path.Combine(RootFolder, folderName);

        public static string DataFilePath
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    throw new NotImplementedException();
                }

                return Path.Combine(RootFolder, folderName, dataFileName);
            }
        }


        static DataFileManager()
        {

        }

        public static async void Init()
        {
            string dbFolder = DataFolder;

            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            string dbFilePath = DataFilePath;
            string dbVersionFilePath = Path.Combine(dbFolder, dbVersionFileName);

            bool hasDbFile = File.Exists(dbFilePath);
            bool hasDbVersionFile = File.Exists(dbVersionFilePath);

            bool dbCopied = false;
#if DEBUG
            await CopyDbFileAsync(dbFilePath);
            dbCopied = true;
#endif

            if (!hasDbFile && !dbCopied)
            {
                await CopyDbFileAsync(dbFilePath);
                dbCopied = true;
            }

            var dbVersion = GetDatabaseVersion();

            if (!hasDbVersionFile)
            {
                File.WriteAllText(dbVersionFilePath, dbVersion);
            }

            if (hasDbVersionFile)
            {
                string oldDbVersion = File.ReadAllText(dbVersionFilePath);

                if (dbVersion != oldDbVersion)
                {
                    if (!dbCopied)
                    {
                        await CopyDbFileAsync(dbFilePath);
                    }

                    File.WriteAllText(dbVersionFilePath, dbVersion);
                }
            }
        }

        private static Assembly GetDataAssembly()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(EnglishDataHook));

            return assembly;
        }

        private static AssemblyName GetDataAssemblyName()
        {
            Assembly assembly = GetDataAssembly();

            var assemblyName = assembly.GetName();

            return assemblyName;
        }

        private static string GetDatabaseVersion()
        {
            string version = GetDataAssemblyName().Version.ToString();

            return version;
        }

        private static async Task CopyDbFileAsync(string targetFilePath)
        {
            Assembly assembly = GetDataAssembly();

            var assemblyName = assembly.GetName().Name;

            using (Stream fs = assembly.GetManifestResourceStream($"{assemblyName}.{dataFileName}"))
            {
                if (fs != null)
                {
                    UserData userData = null;

                    bool needKeepUserData = false;

                    #region 保留用户数据
                    bool hasDbFile = File.Exists(targetFilePath);

                    if (hasDbFile)
                    {
                        try
                        {
                            if (await DataProcessor.HasUserDataTable(targetFilePath))
                            {
                                userData = await DataProcessor.GetUserData(targetFilePath);

                                needKeepUserData = true;

                                SqliteConnection.ClearAllPools();
                            }
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Debug.WriteLine(ex);
#endif
                            return;
                        }
                    }
                    #endregion

                    using (FileStream target = new FileStream(targetFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        StreamWriter writer = new StreamWriter(target);

                        fs.CopyTo(target);

                        writer.Flush();

                        fs.Close();
                    }

                    if (needKeepUserData)
                    {
                        try
                        {
                            if (userData != null)
                            {
                                await DbExecuter.KeepUserData(userData, targetFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Debug.WriteLine(ex);
#endif
                        }

                    }
                }
            }
        }
    }
}
