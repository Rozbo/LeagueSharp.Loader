// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StrongNameUtility.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace PlaySharp.Toolkit.StrongName
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    using Mono.Security;
    using Mono.Security.Cryptography;
    using Mono.Security.X509;

    [SecurityCritical]
    public static class StrongNameUtility
    {
        [SecurityCritical]
        public static bool LoadConfig(bool quiet)
        {
            var config = typeof(Environment).GetMethod("GetMachineConfigPath", BindingFlags.Static | BindingFlags.NonPublic);

            if (config != null)
            {
                var path = (string)config.Invoke(null, null);

                var exist = File.Exists(path);
                if (!quiet && !exist)
                {
                    Console.WriteLine("Couldn't find machine.config");
                }

                StrongNameManager.LoadConfig(path);
                return exist;
            }

            // default CSP
            return false;
        }

        [SecurityCritical]
        public static int SaveConfig()
        {
            // default CSP
            return 1;
        }

        [SecurityCritical]
        public static byte[] ReadFromFile(string fileName)
        {
            byte[] data = null;
            var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }
            finally
            {
                fs.Close();
            }

            return data;
        }

        [SecurityCritical]
        public static void WriteToFile(string fileName, byte[] data)
        {
            var fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
            try
            {
                fs.Write(data, 0, data.Length);
            }
            finally
            {
                fs.Close();
            }
        }

        [SecurityCritical]
        public static void WriteCSVToFile(string fileName, byte[] data, string mask)
        {
            var sw = File.CreateText(fileName);
            try
            {
                for (var i = 0; i < data.Length; i++)
                {
                    if (mask[0] == 'X')
                    {
                        sw.Write("0x");
                    }

                    sw.Write(data[i].ToString(mask));
                    sw.Write(", ");
                }
            }
            finally
            {
                sw.Close();
            }
        }

        [SecurityCritical]
        private static string ToString(byte[] data)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                if ((i % 39 == 0) && (data.Length > 39))
                {
                    sb.Append(Environment.NewLine);
                }

                sb.Append(data[i].ToString("x2"));
                if (i > 2080)
                {
                    // ensure we can display up to 16384 bits keypair
                    sb.Append(" !!! TOO LONG !!!");
                    break;
                }
            }

            return sb.ToString();
        }

        [SecurityCritical]
        public static RSA GetKey(byte[] data, string password = null)
        {
            try
            {
                // for SNK files (including the ECMA pseudo-key)
                return new StrongName(data).RSA;
            }
            catch
            {
                if (data.Length == 0 || data[0] != 0x30)
                {
                    throw;
                }

                if (password == null)
                {
                    throw new ArgumentNullException(nameof(password));
                }

                var pfx = new PKCS12(data, password);

                // works only if a single key is present
                if (pfx.Keys.Count != 1)
                {
                    throw;
                }

                var rsa = pfx.Keys[0] as RSA;
                if (rsa == null)
                {
                    throw;
                }

                return rsa;
            }
        }

        [SecurityCritical]
        public static RSA GetKeyFromFile(string filename, string password = null)
        {
            return GetKey(File.ReadAllBytes(filename), password);
        }

        [SecurityCritical]
        public static bool ReSign(string assemblyFile, string keyFile, string password = null)
        {
            // HACK: use sn.exe to sign if process is 64bit
            if (Environment.Is64BitProcess)
            {
                var snPath = Path.Combine(Path.GetTempPath(), "sn.exe");
                Utility.CreateFileFromResource(snPath, "LeagueSharp.Loader.Resources.sn.exe", true);

                if (!File.Exists(snPath))
                {
                    MessageBox.Show("sn.exe not found");
                    return false;
                }

                var p = new Process
                {
                    StartInfo =
                            new ProcessStartInfo
                            {
                                UseShellExecute = true,
                                FileName = Path.Combine(Path.GetTempPath(), "sn.exe"),
                                Arguments = $"-q -Ra \"{assemblyFile}\" \"{keyFile}\"",
                                WindowStyle = ProcessWindowStyle.Hidden
                            }
                };
                p.Start();
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    MessageBox.Show($"Could not Sign {assemblyFile}");
                    return false;
                }

                File.Delete(snPath);
                Utility.Log(LogStatus.Info, $"Assembly {assemblyFile} successfully signed.");

                return true;
            }

            return ReSign(assemblyFile, GetKeyFromFile(keyFile, password));
        }

        [SecurityCritical]
        public static bool ReSign(string assemblyFile, byte[] resourceKeyFile, string password = null)
        {
            return ReSign(assemblyFile, GetKey(resourceKeyFile, password));
        }

        [SecurityCritical]
        public static bool ReSign(string assemblyName, RSA key)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            // this doesn't load the assembly (well it unloads it ;)
            // http://weblogs.asp.net/nunitaddin/posts/9991.aspx
            AssemblyName an = null;
            try
            {
                an = AssemblyName.GetAssemblyName(assemblyName);
            }
            catch
            {
            }

            if (an == null)
            {
                MessageBox.Show($"Unable to load assembly: {assemblyName}");
                return false;
            }

            var sign = new StrongName(key);
            var token = an.GetPublicKeyToken();

            // first, try to compare using a mapped public key (e.g. ECMA)
            var same = Compare(sign.PublicKey, StrongNameManager.GetMappedPublicKey(token));
            if (!same)
            {
                // second, try to compare using the assembly public key
                same = Compare(sign.PublicKey, an.GetPublicKey());
                if (!same)
                {
                    // third (and last) chance, try to compare public key token
                    same = Compare(sign.PublicKeyToken, token);
                }
            }

            if (same)
            {
                var signed = sign.Sign(assemblyName);

                if (signed)
                {
                    Utility.Log(LogStatus.Info, $"Assembly {assemblyName} successfully signed.");
                }
                else
                {
                    MessageBox.Show($"Couldn't sign the assembly: {assemblyName}");
                }

                return signed;
            }

            MessageBox.Show($"Couldn't sign the assembly {assemblyName} with this key pair.");
            return false;
        }

        [SecurityCritical]
        public static int Verify(string assemblyName, bool forceVerification = true)
        {
            // this doesn't load the assembly (well it unloads it ;)
            // http://weblogs.asp.net/nunitaddin/posts/9991.aspx
            AssemblyName an = null;
            try
            {
                an = AssemblyName.GetAssemblyName(assemblyName);
            }
            catch
            {
            }

            if (an == null)
            {
                MessageBox.Show($"Unable to load assembly: {assemblyName}");
                return 2;
            }

            var publicKey = StrongNameManager.GetMappedPublicKey(an.GetPublicKeyToken());
            if ((publicKey == null) || (publicKey.Length < 12))
            {
                // no mapping
                publicKey = an.GetPublicKey();
                if ((publicKey == null) || (publicKey.Length < 12))
                {
                    return 2;
                }
            }

            // Note: MustVerify is based on the original token (by design). Public key
            // remapping won't affect if the assembly is verified or not.
            if (forceVerification || StrongNameManager.MustVerify(an))
            {
                var rsa = CryptoConvert.FromCapiPublicKeyBlob(publicKey, 12);
                var sn = new StrongName(rsa);
                if (sn.Verify(assemblyName))
                {
                    return 0;
                }
                else
                {
                    MessageBox.Show($"Assembly {assemblyName} is delay-signed but not strongnamed.");
                    return 1;
                }
            }
            else
            {
                MessageBox.Show($"Assembly {assemblyName} is strongnamed (verification skipped).");
                return 0;
            }
        }

        [SecurityCritical]
        public static bool Compare(byte[] value1, byte[] value2)
        {
            if ((value1 == null) || (value2 == null))
            {
                return false;
            }

            var result = value1.Length == value2.Length;
            if (result)
            {
                for (var i = 0; i < value1.Length; i++)
                {
                    if (value1[i] != value2[i])
                    {
                        return false;
                    }
                }
            }

            return result;
        }
    }
}