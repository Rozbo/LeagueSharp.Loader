// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedMemory.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Sandbox.Shared
{
    using System;
    using System.IO.MemoryMappedFiles;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    [SecuritySafeCritical]
    public class SharedMemory<T> : IDisposable
        where T : struct
    {
        private readonly string fileName;

        private readonly string mutexName;

        private readonly string name;

        private readonly int size;

        private MemoryMappedViewAccessor accessor;

        private bool locked;

        private MemoryMappedFile mmf;

        private Mutex mutex;

        public SharedMemory(string name, int size = 0, bool autoOpen = true)
        {
            this.name = name;
            this.fileName = $"Local\\{name}";
            this.mutexName = $"{name}_LOCK";
            this.size = size == 0 ? Marshal.SizeOf(typeof(T)) : size;

            if (autoOpen)
            {
                this.Open();
            }
        }

        public T Data
        {
            [SecuritySafeCritical]
            get
            {
                T dataStruct;
                var dataSize = Marshal.SizeOf(typeof(T));
                var data = new byte[dataSize];
                var p = Marshal.AllocHGlobal(dataSize);

                try
                {
                    // Read from memory mapped file.
                    this.accessor.ReadArray(0, data, 0, data.Length);

                    // Copy from byte array to unmanaged memory.
                    Marshal.Copy(data, 0, p, dataSize);

                    // Copy unmanaged memory to struct.
                    dataStruct = (T)Marshal.PtrToStructure(p, typeof(T));
                }
                finally
                {
                    Marshal.FreeHGlobal(p);
                }

                return dataStruct;
            }

            [SecuritySafeCritical]
            set
            {
                this.mutex.WaitOne();
                var dataSize = Marshal.SizeOf(typeof(T));
                var data = new byte[dataSize];
                var ptr = Marshal.AllocHGlobal(dataSize);

                try
                {
                    // Copy struct to unmanaged memory.
                    Marshal.StructureToPtr(value, ptr, true);

                    // Copy from unmanaged memory to byte array.
                    Marshal.Copy(ptr, data, 0, dataSize);

                    // Write to memory mapped file.
                    this.accessor.WriteArray<byte>(0, data, 0, data.Length);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                    this.mutex.ReleaseMutex();
                }
            }
        }

        [SecuritySafeCritical]
        public void Close()
        {
            Utility.Log(LogStatus.Debug, this.fileName);
            this.accessor.Dispose();
            this.mmf.Dispose();
            this.mutex.Close();
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Close();
        }

        [SecuritySafeCritical]
        public bool Open()
        {
            try
            {
                Utility.Log(LogStatus.Debug, $"{this.fileName} - {this.mutexName} - {this.size} bytes");
                this.mmf = MemoryMappedFile.CreateOrOpen(this.name, this.size);
                this.accessor = this.mmf.CreateViewAccessor(0, this.size, MemoryMappedFileAccess.ReadWrite);
                this.mutex = new Mutex(true, this.mutexName, out this.locked);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}