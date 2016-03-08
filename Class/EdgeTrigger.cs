namespace PlaySharp.Toolkit.Helper
{
    using System;
    using System.Security;

    [SecuritySafeCritical]
    public class EdgeTrigger
    {
        public event EventHandler Falling;
        public event EventHandler Fallen;
        public event EventHandler Rising;
        public event EventHandler Risen;
        private bool value;

        public bool Value
        {
            [SecuritySafeCritical]
            get
            {
                return this.value;
            }

            [SecuritySafeCritical]
            set
            {
                if (this.value == value)
                {
                    return;
                }

                if (value == true)
                {
                    this.Rising?.Invoke(this, EventArgs.Empty);
                    this.value = true;
                    this.Risen?.Invoke(this, EventArgs.Empty);
                }

                if (value == false)
                {
                    this.Falling?.Invoke(this, EventArgs.Empty);
                    this.value = false;
                    this.Fallen?.Invoke(this, EventArgs.Empty);
                }

                this.value = value;
            }
        }
    }
}