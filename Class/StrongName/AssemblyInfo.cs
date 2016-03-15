// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace PlaySharp.Toolkit.StrongName
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Text;

    [SecurityCritical]
    public class AssemblyInfo
    {
        private string _copyright;

        private string _description;

        private string _name;

        private string _title;

        private string _version;

        [SecurityCritical]
        public AssemblyInfo()
            : this(Assembly.GetExecutingAssembly())
        {
        }

        [SecurityCritical]
        public AssemblyInfo(Assembly a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            var an = a.GetName();
            this._name = an.ToString();

            var att = a.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            this._title = (att.Length > 0) ? ((AssemblyTitleAttribute)att[0]).Title : string.Empty;

            att = a.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            this._copyright = (att.Length > 0) ? ((AssemblyCopyrightAttribute)att[0]).Copyright : string.Empty;

            att = a.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            this._description = (att.Length > 0) ? ((AssemblyDescriptionAttribute)att[0]).Description : string.Empty;

            this._version = an.Version.ToString();
        }

        public string Copyright
        {
            [SecurityCritical]
            get
            {
                return this._copyright;
            }
        }

        public string Description
        {
            [SecurityCritical]
            get
            {
                return this._description;
            }
        }

        public string Name
        {
            [SecurityCritical]
            get
            {
                return this._name;
            }
        }

        public string Title
        {
            [SecurityCritical]
            get
            {
                return this._title;
            }
        }

        public string Version
        {
            [SecurityCritical]
            get
            {
                return this._version;
            }
        }

        [SecurityCritical]
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{1} - version {2}{0}{3}{0}{4}{0}", Environment.NewLine, this._title, this._version, this._description, this._copyright);
            return sb.ToString();
        }
    }
}