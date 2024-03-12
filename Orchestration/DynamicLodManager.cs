using MSFSPopoutPanelManager.DomainModel.DynamicLod;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class DynamicLodManager
    {
        private const string SIMMODULE_NAME = "WwiseLibPCx64P.dll";
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const long OFFSET_MODULE_BASE =0x004B2368;
        private const long OFFSET_POINTER_MAIN = 0x3D0;
        private const long OFFSET_POINTER_TLOD_VR = 0x114;
        private const long OFFSET_POINTER_TLOD = 0xC;
        private const long OFFSET_POINTER_OLOD = 0xC;

        private static bool _isActive;
        private static WindowProcess _process;
        private static IntPtr _processHandle;
        private static long _processModuleAddress;

        private static long _addressMain;
        private static long _addressTlod;
        private static long _addressOlod;
        private static long _addressTlodVr;
        private static long _addressOlodVr;

        private static FlightSimData _flightSimData;
        private static AppSettingData _appSettingData;
        private static LinkedListNode<LodConfig> _nextTlod;
        private static LinkedListNode<LodConfig> _nextOlod;
        private static LinkedListNode<LodConfig> _currentTlod;
        private static LinkedListNode<LodConfig> _currentOlod;

        public static void Attach(FlightSimData flightSimData, AppSettingData appSettingData)
        {
            if (appSettingData == null || _isActive)
                return;

            _flightSimData = flightSimData;
            _appSettingData = appSettingData;
            _flightSimData.OnAltAboveGroundChanged -= HandleOnAltAboveGroundChanged;
            _flightSimData.OnAltAboveGroundChanged += HandleOnAltAboveGroundChanged;

            _process = WindowProcessManager.SimulatorProcess;
            _processModuleAddress = GetSimModuleAddress();
            
            if (_process == null || _processModuleAddress == IntPtr.Zero)
                return;

            _processHandle = PInvoke.OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, _process.ProcessId);

            if (_processHandle == IntPtr.Zero) 
                return;

            _addressMain = ReadMemory<long>(_processModuleAddress + OFFSET_MODULE_BASE) + OFFSET_POINTER_MAIN;
            _addressTlod = ReadMemory<long>(_addressMain) + OFFSET_POINTER_TLOD;
            _addressTlodVr = ReadMemory<long>(_addressMain) + OFFSET_POINTER_TLOD_VR;
            _addressOlod = _addressTlod + OFFSET_POINTER_OLOD;
            _addressOlodVr = _addressTlodVr + OFFSET_POINTER_OLOD;

            // Set initial LOD if already in cockpit
            InitializeData();

            _isActive = true;
        }

        public static void Detach()
        {
            if (_appSettingData == null)
                return;

            if (_appSettingData.ApplicationSetting.DynamicLodSetting.ResetEnabled)
            {
                WriteTlod(_appSettingData.ApplicationSetting.DynamicLodSetting.ResetTlod);
                WriteOlod(_appSettingData.ApplicationSetting.DynamicLodSetting.ResetOlod);
            }

            _isActive = false;

            Debug.WriteLine($"Reset to custom LOD: TLOD: {_appSettingData.ApplicationSetting.DynamicLodSetting.ResetTlod}, OLOD: {_appSettingData.ApplicationSetting.DynamicLodSetting.ResetOlod}");
        }

        private static void HandleOnAltAboveGroundChanged(object sender, EventArgs e)
        {
            if (!_flightSimData.IsFlightStarted)
                return;

            var agl = _flightSimData.PlaneAltAboveGround < 0 ? 0 : _flightSimData.PlaneAltAboveGround;

            if (_nextTlod != null && _nextTlod.Value.Agl <= agl)
            {
                _currentTlod = _nextTlod;
                _nextTlod = _currentTlod.Next;
                DynamicLodManager.WriteTlod(_currentTlod.Value.Lod);
            }
            else if (_currentTlod is { Previous: not null } && _currentTlod.Value.Agl > agl)
            {
                _currentTlod = _currentTlod.Previous;

                if (_currentTlod != null)
                {
                    _nextTlod = _currentTlod.Next;
                    DynamicLodManager.WriteTlod(_currentTlod.Value.Lod);
                }
            }

            if (_nextOlod != null && _nextOlod.Value.Agl <= agl)
            {
                _currentOlod = _nextOlod;
                _nextOlod = _currentOlod.Next;
                DynamicLodManager.WriteOlod(_currentOlod.Value.Lod);
            }
            else if (_currentOlod is { Previous: not null } && _currentOlod.Value.Agl > agl)
            {
                _currentOlod = _currentOlod.Previous;

                if (_currentOlod != null)
                {
                    _nextOlod = _currentOlod.Next;
                    DynamicLodManager.WriteOlod(_currentOlod.Value.Lod);
                }
            }

            if(_currentTlod != null && _currentOlod != null)
                Debug.WriteLine($"Altitude: {agl}, TLOD: {_currentTlod.Value.Lod}, OLOD: {_currentOlod.Value.Lod}");
        }

        private static void WriteTlod(int value, bool isVr = false)
        {
            WriteMemory(isVr ? _addressTlodVr : _addressTlod, value / 100.0f);
        }

        private static void WriteOlod(int value, bool isVr = false)
        {
            WriteMemory(isVr ? _addressOlodVr : _addressOlod, value / 100.0f);
        }

        private static long GetSimModuleAddress()
        {
            if (_process == null)
                return -1;

            foreach (ProcessModule processModule in _process.Modules)
            {
                if (processModule.ModuleName == SIMMODULE_NAME)
                    return processModule.BaseAddress;
            }

            return -1;
        }

        private static void WriteMemory(long address, object value)
        {
            try
            {
                var buffer = StructureToByteArray(value);
                PInvoke.NtWriteVirtualMemory(checked((int)_processHandle), address, buffer, buffer.Length, out _);
            }
            catch
            {
                // ignored
            }
        }

        private static T ReadMemory<T>(long address) where T : struct
        {
            try
            {
                var byteSize = Marshal.SizeOf(typeof(T));
                var buffer = new byte[byteSize];
                PInvoke.NtReadVirtualMemory(checked((int)_processHandle), address, buffer, buffer.Length, out _);
                return ByteArrayToStructure<T>(buffer);
            }
            catch
            {
                // ignored
            }

            return default(T);
        }

        private static byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);
            var array = new byte[length];
            var pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                var result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

                if(result != null)
                    return (T)result;

                return default(T);
            }
            finally
            {
                handle.Free();
            }
        }

        private static void InitializeData()
        {
            var agl = _flightSimData.PlaneAltAboveGround < 0 ? 0 : _flightSimData.PlaneAltAboveGround;

            var tlod = _appSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.FirstOrDefault(x => x.Agl > agl);
            _nextTlod = _appSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.Find(tlod);

            tlod = _appSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.LastOrDefault(x => x.Agl <= agl);
            _currentTlod = _appSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.Find(tlod) ?? _appSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.Last;

            if (_currentTlod != null)
                DynamicLodManager.WriteTlod(_currentTlod.Value.Lod);

            var olod = _appSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.FirstOrDefault(x => x.Agl > agl);
            _nextOlod = _appSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.Find(olod);

            olod = _appSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.LastOrDefault(x => x.Agl <= agl);
            _currentOlod = _appSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.Find(olod) ?? _appSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.Last;

            if (_currentOlod != null)
                DynamicLodManager.WriteOlod(_currentOlod.Value.Lod);

            if (_currentTlod != null && _currentOlod != null)
                Debug.WriteLine($"Initialize Altitude: {agl}, TLOD: {_currentTlod.Value.Lod}, OLOD: {_currentOlod.Value.Lod}");
        }

        //private static int ReadTlod(bool isVr = false)
        //{
        //    return Convert.ToInt32(ReadMemory<float>(isVr ? _addressTlodVr : _addressTlod) * 100.0f);
        //}

        //private static int ReadOlod(bool isVr = false)
        //{
        //    return Convert.ToInt32(ReadMemory<float>(isVr ? _addressOlodVr : _addressOlod) * 100.0f);
        //}
    }
}
