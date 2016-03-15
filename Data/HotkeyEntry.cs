// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HotkeyEntry.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Data
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using LeagueSharp.Loader.Class;

    public class HotkeyEntry : INotifyPropertyChanged
    {
        private Key _hotkey;

        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public Key DefaultKey { get; set; }

        public string Description { get; set; }

        public string DisplayDescription => Utility.GetMultiLanguageText(this.Description);

        public Key Hotkey
        {
            get
            {
                return this._hotkey;
            }

            set
            {
                this._hotkey = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("HotkeyString");
            }
        }

        public byte HotkeyInt
        {
            get
            {
                if (this.Hotkey == Key.LeftShift || this.Hotkey == Key.RightShift)
                {
                    return 16;
                }

                if (this.Hotkey == Key.LeftAlt || this.Hotkey == Key.RightAlt)
                {
                    return 0x12;
                }

                if (this.Hotkey == Key.LeftCtrl || this.Hotkey == Key.RightCtrl)
                {
                    return 0x11;
                }

                return (byte)KeyInterop.VirtualKeyFromKey(this.Hotkey);
            }

            set
            {
            }
        }

        public string HotkeyString => this._hotkey.ToString();

        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}