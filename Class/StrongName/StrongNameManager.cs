// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StrongNameManager.cs" company="PlaySharp">
//   Copyright (c) PlaySharp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace PlaySharp.Toolkit.StrongName
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Text;

    using Mono.Security.Cryptography;
    using Mono.Xml;

    /* RUNTIME
	 *				yes
	 *	in_gac ---------------------------------\
	 *		|				|
	 *		| no				\/
	 *		|			return true
	 * CLASS LIBRARY|
	 *		|
	 * 		|
	 *		|				
	 *	bool StrongNameManager.MustVerify
	 *		|
	 *		|
	 *		\/		not found	
	 *		Token --------------------------\
	 *		|				|
	 *		| present ?			|
	 *		|				|
	 *		\/		not found	|
	 *	Assembly Name --------------------------|
	 *		|				|
	 *		| present ?			|
	 *		| or "*"			|
	 *		\/		not found	|
	 *		User ---------------------------|
	 *		|				|
	 *		| present ?			|
	 *		| or "*"			|
	 *		\/				\/
	 *	return false			return true
	 *	SKIP VERIFICATION		VERIFY ASSEMBLY
	 */
    internal class StrongNameManager
    {
        private static Hashtable mappings;

        private static Hashtable tokens;

        static StrongNameManager()
        {
        }

        public static byte[] GetMappedPublicKey(byte[] token)
        {
            if ((mappings == null) || (token == null))
            {
                return null;
            }

            var t = CryptoConvert.ToHex(token);
            var pk = (string)mappings[t];
            if (pk == null)
            {
                return null;
            }

            return CryptoConvert.FromHex(pk);
        }

        // note: more than one configuration file can be loaded at the 
        // same time (e.g. user specific and machine specific config).
        public static void LoadConfig(string filename)
        {
            if (File.Exists(filename))
            {
                var sp = new SecurityParser();
                using (var sr = new StreamReader(filename))
                {
                    var xml = sr.ReadToEnd();
                    sp.LoadXml(xml);
                }

                var root = sp.ToXml();
                if ((root != null) && (root.Tag == "configuration"))
                {
                    var strongnames = root.SearchForChildByTag("strongNames");
                    if ((strongnames != null) && (strongnames.Children.Count > 0))
                    {
                        var mapping = strongnames.SearchForChildByTag("pubTokenMapping");
                        if ((mapping != null) && (mapping.Children.Count > 0))
                        {
                            LoadMapping(mapping);
                        }

                        var settings = strongnames.SearchForChildByTag("verificationSettings");
                        if ((settings != null) && (settings.Children.Count > 0))
                        {
                            LoadVerificationSettings(settings);
                        }
                    }
                }
            }
        }

        // it is possible to skip verification for assemblies 
        // or a strongname public key using the "sn" tool.
        // note: only the runtime checks if the assembly is loaded 
        // from the GAC to skip verification
        public static bool MustVerify(AssemblyName an)
        {
            if ((an == null) || (tokens == null))
            {
                return true;
            }

            var token = CryptoConvert.ToHex(an.GetPublicKeyToken());
            var el = (Element)tokens[token];
            if (el != null)
            {
                // look for this specific assembly first
                var users = el.GetUsers(an.Name);
                if (users == null)
                {
                    // nothing for the specific assembly
                    // so look for "*" assembly
                    users = el.GetUsers("*");
                }

                if (users != null)
                {
                    // applicable to any user ?
                    if (users == "*")
                    {
                        return false;
                    }

                    // applicable to the current user ?
                    return users.IndexOf(Environment.UserName) < 0;
                }
            }

            // we must check verify the strongname on the assembly
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Public Key Token\tAssemblies\t\tUsers");
            sb.Append(Environment.NewLine);
            if (tokens == null)
            {
                sb.Append("none");
                return sb.ToString();
            }

            foreach (DictionaryEntry token in tokens)
            {
                sb.Append((string)token.Key);
                var t = (Element)token.Value;
                var first = true;
                foreach (DictionaryEntry assembly in t.assemblies)
                {
                    if (first)
                    {
                        sb.Append("\t");
                        first = false;
                    }
                    else
                    {
                        sb.Append("\t\t\t");
                    }

                    sb.Append((string)assembly.Key);
                    sb.Append("\t");
                    var users = (string)assembly.Value;
                    if (users == "*")
                    {
                        users = "All users";
                    }

                    sb.Append(users);
                    sb.Append(Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        private static void LoadMapping(SecurityElement mapping)
        {
            if (mappings == null)
            {
                mappings = new Hashtable();
            }

            lock (mappings.SyncRoot)
            {
                foreach (SecurityElement item in mapping.Children)
                {
                    if (item.Tag != "map")
                    {
                        continue;
                    }

                    var token = item.Attribute("Token");
                    if ((token == null) || (token.Length != 16))
                    {
                        continue; // invalid entry
                    }

                    token = token.ToUpper(CultureInfo.InvariantCulture);

                    var publicKey = item.Attribute("PublicKey");
                    if (publicKey == null)
                    {
                        continue; // invalid entry
                    }

                    // watch for duplicate entries
                    if (mappings[token] == null)
                    {
                        mappings.Add(token, publicKey);
                    }
                    else
                    {
                        // replace existing mapping
                        mappings[token] = publicKey;
                    }
                }
            }
        }

        private static void LoadVerificationSettings(SecurityElement settings)
        {
            if (tokens == null)
            {
                tokens = new Hashtable();
            }

            lock (tokens.SyncRoot)
            {
                foreach (SecurityElement item in settings.Children)
                {
                    if (item.Tag != "skip")
                    {
                        continue;
                    }

                    var token = item.Attribute("Token");
                    if (token == null)
                    {
                        continue; // bad entry
                    }

                    token = token.ToUpper(CultureInfo.InvariantCulture);

                    var assembly = item.Attribute("Assembly");
                    if (assembly == null)
                    {
                        assembly = "*";
                    }

                    var users = item.Attribute("Users");
                    if (users == null)
                    {
                        users = "*";
                    }

                    var el = (Element)tokens[token];
                    if (el == null)
                    {
                        // new token
                        el = new Element(assembly, users);
                        tokens.Add(token, el);
                        continue;
                    }

                    // existing token
                    var a = (string)el.assemblies[assembly];
                    if (a == null)
                    {
                        // new assembly
                        el.assemblies.Add(assembly, users);
                        continue;
                    }

                    // existing assembly
                    if (users == "*")
                    {
                        // all users (drop current users)
                        el.assemblies[assembly] = "*";
                        continue;
                    }

                    // new users, add to existing
                    var existing = (string)el.assemblies[assembly];
                    var newusers = string.Concat(existing, ",", users);
                    el.assemblies[assembly] = newusers;
                }
            }
        }

        private class Element
        {
            internal Hashtable assemblies;

            public Element()
            {
                this.assemblies = new Hashtable();
            }

            public Element(string assembly, string users)
                : this()
            {
                this.assemblies.Add(assembly, users);
            }

            public string GetUsers(string assembly)
            {
                return (string)this.assemblies[assembly];
            }
        }
    }
}