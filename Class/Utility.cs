// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Class
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using LeagueSharp.Loader.Data;

    using Application = System.Windows.Application;

    public static class ListExtensions
    {
        private static readonly Random Rng = new Random();

        public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var item in collection)
            {
                oc.Add(item);
            }
        }

        public static void ShuffleRandom<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class WebExtensions
    {
        public static string WebDecode(this string s)
        {
            return HttpUtility.HtmlDecode(HttpUtility.UrlDecode(s));
        }
    }

    public class Utility
    {
        private static readonly Random Random = new Random();

        public static void ClearDirectory(string directory)
        {
            try
            {
                var dir = new DirectoryInfo(directory);
                foreach (var fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Attributes = FileAttributes.Normal;
                        fi.Delete();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                foreach (var di in dir.GetDirectories())
                {
                    try
                    {
                        di.Attributes = FileAttributes.Normal;
                        ClearDirectory(di.FullName);
                        di.Delete();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch
            {
            }
        }

        public static void CopyDirectory(
            string sourceDirName, 
            string destDirName, 
            bool copySubDirs = false, 
            bool overrideFiles = false)
        {
            try
            {
                // Get the subdirectories for the specified directory.
                var dir = new DirectoryInfo(sourceDirName);
                dir.Attributes = FileAttributes.Directory;
                var dirs = dir.GetDirectories();

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: " + sourceDirName);
                }

                // If the destination directory doesn't exist, create it. 
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var temppath = Path.Combine(destDirName, file.Name);
                    file.Attributes = FileAttributes.Normal;
                    file.CopyTo(temppath, overrideFiles);
                }

                // If copying subdirectories, copy them and their contents to new location. 
                if (copySubDirs)
                {
                    foreach (var subdir in dirs)
                    {
                        var temppath = Path.Combine(destDirName, subdir.Name);
                        CopyDirectory(subdir.FullName, temppath, true, overrideFiles);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public static LeagueSharpAssembly CreateEmptyAssembly(string assemblyName)
        {
            try
            {
                var appconfig = ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.App.config");
                var assemblyInfocs = ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.AssemblyInfo.cs");
                var defaultProjectcsproj = ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.DefaultProject.csproj");
                var programcs = ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.Program.cs");

                var targetPath = Path.Combine(
                    Directories.LocalRepositoriesDirectory, 
                    assemblyName + Environment.TickCount.GetHashCode().ToString("X"));
                Directory.CreateDirectory(targetPath);

                programcs = programcs.Replace("{ProjectName}", assemblyName);
                assemblyInfocs = assemblyInfocs.Replace("{ProjectName}", assemblyName);
                defaultProjectcsproj = defaultProjectcsproj.Replace("{ProjectName}", assemblyName);
                defaultProjectcsproj = defaultProjectcsproj.Replace("{SystemDirectory}", Directories.CoreDirectory);

                File.WriteAllText(Path.Combine(targetPath, "App.config"), appconfig);
                File.WriteAllText(Path.Combine(targetPath, "AssemblyInfo.cs"), assemblyInfocs);
                File.WriteAllText(Path.Combine(targetPath, assemblyName + ".csproj"), defaultProjectcsproj);
                File.WriteAllText(Path.Combine(targetPath, "Program.cs"), programcs);

                return new LeagueSharpAssembly(assemblyName, Path.Combine(targetPath, assemblyName + ".csproj"), string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static void CreateFileFromResource(string path, string resource, bool overwrite = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (!overwrite && File.Exists(path))
            {
                return;
            }

            var data = ReadResource(resource);
            File.WriteAllBytes(path, data);
        }

        public static string GetLatestLeagueOfLegendsExePath(string lastKnownPath)
        {
            try
            {
                // CN
                if (lastKnownPath.EndsWith("Game\\League of Legends.exe"))
                {
                    return lastKnownPath;
                }

                var dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(lastKnownPath), "..\\..\\"));
                if (Directory.Exists(dir))
                {
                    var versionPaths = Directory.GetDirectories(dir);
                    var greatestVersionString = string.Empty;
                    long greatestVersion = 0;

                    foreach (var versionPath in versionPaths)
                    {
                        Version version;
                        var isVersion = Version.TryParse(Path.GetFileName(versionPath), out version);
                        if (isVersion)
                        {
                            var test = version.Build * Math.Pow(600, 4) + version.Major * Math.Pow(600, 3)
                                       + version.Minor * Math.Pow(600, 2) + version.Revision * Math.Pow(600, 1);
                            if (test > greatestVersion)
                            {
                                greatestVersion = (long)test;
                                greatestVersionString = Path.GetFileName(versionPath);
                            }
                        }
                    }

                    if (greatestVersion != 0)
                    {
                        var exe = Directory.GetFiles(
                            Path.Combine(dir, greatestVersionString), 
                            "League of Legends.exe", 
                            SearchOption.AllDirectories);
                        return exe.Length > 0 ? exe[0] : null;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static string GetMultiLanguageText(string key)
        {
            return Application.Current.FindResource(key).ToString();
        }

        public static string GetUniqueFile(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            var fileExt = Path.GetExtension(file.Extension);

            if (file.Name == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(fileExt));
            }

            var len = Random.Next(Math.Min(4, fileName.Length), fileName.Length);
            var newFile = GetUniqueKey(len) + fileExt;

            Log(LogStatus.Debug, $"{file.Name} -> {newFile}");

            return newFile;
        }

        public static string GetUniqueFile(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return GetUniqueFile(new FileInfo(file));
        }

        public static string GetUniqueKey(int maxSize)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            var data = new byte[1];

            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            var result = new StringBuilder(maxSize);

            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        public static void Log(LogStatus status, string message, [CallerMemberName] string source = null)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(
                () => Logs.MainLog.Items.Add(new LogItem { Status = status.ToString(), Source = source, Message = message }));
        }

        public static string MakeValidFileName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidRegStr, "_");
        }

        public static void MapClassToXmlFile(Type type, object obj, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public static object MapXmlFileToClass(Type type, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                return serializer.Deserialize(reader);
            }
        }

        public static string Md5Checksum(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    return "-1";
                }

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
                    }
                }
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        /// <summary>
        ///     Returns the md5 hash from a string.
        /// </summary>
        public static string Md5Hash(string s)
        {
            var sb = new StringBuilder();
            HashAlgorithm algorithm = MD5.Create();
            var h = algorithm.ComputeHash(Encoding.Default.GetBytes(s));

            foreach (var b in h)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static bool OverwriteFile(string file, string path, bool copy = false)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                try
                {
                    if (copy)
                    {
                        File.Copy(file, path);
                    }
                    else
                    {
                        File.Move(file, path);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    throw;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [SecuritySafeCritical]
        public static byte[] ReadResource(string file, Assembly assembly = null)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            var resourceFile = assembly.GetManifestResourceNames().FirstOrDefault(f => f.EndsWith(file));
            if (resourceFile == null)
            {
                // Log.Warn($"Not found {assembly.GetName().Name} - {file}");
                throw new ArgumentNullException(nameof(resourceFile));
            }

            // Log.Debug($"Copy {resourceFile} -> Memory");
            using (var ms = new MemoryStream())
            {
                assembly.GetManifestResourceStream(resourceFile)?.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string ReadResourceString(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            return string.Empty;
        }

        public static bool RenameFileIfExists(string file, string path)
        {
            try
            {
                var counter = 1;
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fileExtension = Path.GetExtension(file);
                var newPath = path;
                var pathDirectory = Path.GetDirectoryName(path);
                if (pathDirectory != null)
                {
                    if (!Directory.Exists(pathDirectory))
                    {
                        Directory.CreateDirectory(pathDirectory);
                    }

                    while (File.Exists(newPath))
                    {
                        var tmpFileName = string.Format("{0} ({1})", fileName, counter++);
                        newPath = Path.Combine(pathDirectory, tmpFileName + fileExtension);
                    }

                    File.Move(file, newPath);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static byte[] ReplaceFilling(string file, string searchFileName, string replaceFileName, Encoding encoding = null)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (searchFileName == null)
            {
                throw new ArgumentNullException(nameof(searchFileName));
            }

            if (replaceFileName == null)
            {
                throw new ArgumentNullException(nameof(replaceFileName));
            }

            return ReplaceFilling(
                File.ReadAllBytes(file), 
                Encoding.ASCII.GetBytes(searchFileName), 
                Encoding.ASCII.GetBytes(replaceFileName));
        }

        public static byte[] ReplaceFilling(byte[] content, byte[] search, byte[] replacement)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            if (search.Length == 0)
            {
                return content;
            }

            var result = new List<byte>();

            int i;

            for (i = 0; i <= content.Length - search.Length; i++)
            {
                var foundMatch = true;
                for (var j = 0; j < search.Length; j++)
                {
                    if (content[i + j] != search[j])
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    result.AddRange(replacement);
                    for (var k = 0; k < search.Length - replacement.Length; k++)
                    {
                        result.Add(0x00);
                    }

                    i += search.Length - 1;
                }
                else
                {
                    result.Add(content[i]);
                }
            }

            for (; i < content.Length; i++)
            {
                result.Add(content[i]);
            }

            return result.ToArray();
        }

        public static int VersionToInt(Version version)
        {
            return version.Major * 10000000 + version.Minor * 10000 + version.Build * 100 + version.Revision;
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        }
    }
}