using MSFSPopoutPanelManager.DomainModel.DynamicLod;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.SimConnectAgent;
using MSFSPopoutPanelManager.WindowsAgent;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class DynamicLodOrchestrator : BaseOrchestrator
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
        private const long OFFSET_POINTER_CLOUDQ = 0x44;
        private const long OFFSET_POINTER_CLOUDQ_VR = 0x108;
        private const long OFFSET_POINTER_VR_MODE = 0x1C;
        private const long OFFSET_POINTER_FG_MODE = 0x4A;
        private const long OFFSET_POINTER_ANSIO_FILTER = -0x18;
        private const long OFFSET_POINTER_WATER_WAVES = 0x3C;

        private bool _isActive;
        private WindowProcess _process;
        private IntPtr _processHandle;
        private long _processModuleAddress;

        private long _addressTlod;
        private long _addressOlod;
        private long _addressTlodVr;
        private long _addressOlodVr;
        private long _addressCloudQ;
        private long _addressCloudQVr;
        private long _addressVrMode;
        private long _addressFgMode;

        private DynamicLodSetting DynamicLodSetting => AppSettingData.ApplicationSetting.DynamicLodSetting;

        private DynamicLodSimData SimData => FlightSimData.DynamicLodSimData;

        private DateTime _lastLodUpdateTime = DateTime.Now;
        private bool _isDecreasedCloudQualityActive = false;

        public DynamicLodOrchestrator(SharedStorage sharedStorage) : base(sharedStorage) {}

        public void Attach()
        {
            if (AppSettingData == null || _isActive)
                return;

            _process = WindowProcessManager.SimulatorProcess;
            _processModuleAddress = GetSimModuleAddress();
            
            if (_process == null || _processModuleAddress == IntPtr.Zero)
                return;

            _processHandle = PInvoke.OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, _process.ProcessId);

            if (_processHandle == IntPtr.Zero) 
                return;

            _addressTlod = ReadMemory<long>(_processModuleAddress + OFFSET_MODULE_BASE) + OFFSET_POINTER_MAIN;

            if (_addressTlod > 0)
            {
                _addressTlodVr = ReadMemory<long>(_addressTlod) + OFFSET_POINTER_TLOD_VR;
                _addressTlod = ReadMemory<long>(_addressTlod) + OFFSET_POINTER_TLOD;
                _addressOlod = _addressTlod + OFFSET_POINTER_OLOD;
                _addressOlodVr = _addressTlodVr + OFFSET_POINTER_OLOD;
                _addressCloudQ = _addressTlod + OFFSET_POINTER_CLOUDQ;
                _addressCloudQVr = _addressCloudQ + OFFSET_POINTER_CLOUDQ_VR;
                _addressVrMode = _addressTlod - OFFSET_POINTER_VR_MODE;
                _addressFgMode = _addressTlod - OFFSET_POINTER_FG_MODE;

                if (!MemoryBoundaryTest())
                {
                    FileLogger.WriteLog("Unable to validate memory space for Dynamic LOD", StatusMessageType.Error);
                    return;
                }
            }

            _isActive = true;

            _lastLodUpdateTime = DateTime.Now;
            _isDecreasedCloudQualityActive = false;
        }

        public void Detach()
        {
            if (DynamicLodSetting == null)
                return;

            if (DynamicLodSetting.ResetEnabled)
            {
                var isVr = ReadIsVr();
                WriteMemory(isVr ? _addressTlodVr : _addressTlod, DynamicLodSetting.ResetTlod / 100.0f);
                WriteMemory(isVr ? _addressOlodVr : _addressOlod, DynamicLodSetting.ResetOlod / 100.0f);
            }

            _isActive = false;

            Debug.WriteLine($"Reset to custom LOD: TLOD: {DynamicLodSetting.ResetTlod}, OLOD: {DynamicLodSetting.ResetOlod}");
        }

        public int ReadTlod(bool isVr = false)
        {
            return Convert.ToInt32(ReadMemory<float>(isVr ? _addressTlodVr : _addressTlod) * 100.0f);
        }

        public int ReadOlod(bool isVr = false)
        {
            return Convert.ToInt32(ReadMemory<float>(isVr ? _addressOlodVr : _addressOlod) * 100.0f);
        }

        public string ReadCloudQuality(bool isVr = false)
        {
            return ReadCloudQualitySimValue(isVr) switch
            {
                0 => "Low",
                1 => "Medium",
                2 => "High",
                3 => "Ultra",
                _ => "N/A"
            };
        }

        public int ReadCloudQualitySimValue(bool isVr = false)
        {
            return Convert.ToInt32(ReadMemory<int>(isVr ? _addressCloudQVr : _addressCloudQ));
        }

        public bool ReadIsVr()
        {
            return ReadMemory<int>(_addressVrMode) == 1;
        }

        public bool ReadIsFg(bool isVr)
        {
            if (isVr)
                return false;

            return ReadMemory<byte>(_addressFgMode) == 1;
        }
        
        public void UpdateLod(bool isVr)
        {
            if (DateTime.Now - _lastLodUpdateTime <= TimeSpan.FromSeconds(1))
                return;

            if (!FlightSimData.IsFlightStarted || !FlightSimData.IsInCockpit || (DynamicLodSetting.PauseOutsideCockpitView && FlightSimData.CameraState != CameraState.Cockpit))
                return;

            var deltaFps = SimData.Fps - DynamicLodSetting.TargetedFps;
            if (Math.Abs(deltaFps) < DynamicLodSetting.TargetedFps * DynamicLodSetting.FpsTolerance / 100.0)       // within FPS tolerance
                return;
            
            _lastLodUpdateTime = DateTime.Now;

            SetTlod(deltaFps);
            SetOlod();
        }
        
        private long GetSimModuleAddress()
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

        private void WriteMemory(long address, object value)
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

        private T ReadMemory<T>(long address) where T : struct
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

        private byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);
            var array = new byte[length];
            var pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
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

        private bool MemoryBoundaryTest()
        {
            // Boundary check a few known setting memory addresses to see if any fail which likely indicates MSFS memory map has changed
            if (ReadTlod() < 10 || ReadTlod() > 1000 || ReadTlod(true) < 10 || ReadTlod(true) > 1000
                || ReadOlod() < 10 || ReadOlod() > 1000 || ReadOlod(true) < 10 || ReadOlod(true) > 1000
                || ReadCloudQuality() == "N/A" || ReadCloudQuality(true) == "N/A"
                || ReadMemory<int>(_addressVrMode) < 0 || ReadMemory<int>(_addressVrMode) > 1
                || ReadMemory<int>(_addressTlod + OFFSET_POINTER_ANSIO_FILTER) < 1 || ReadMemory<int>(_addressTlod + OFFSET_POINTER_ANSIO_FILTER) > 16
                || !(ReadMemory<int>(_addressTlod + OFFSET_POINTER_WATER_WAVES) == 128 || ReadMemory<int>(_addressTlod + OFFSET_POINTER_WATER_WAVES) == 256 || ReadMemory<int>(_addressTlod + OFFSET_POINTER_WATER_WAVES) == 512))
            {
                return false;
            }

            return true;
        }
        
        private void SetTlod(int deltaFps, bool isVr = false)
        {
            var tlodStep = Math.Max(5, Math.Abs(deltaFps / 2));
            var newTlod = SimData.Tlod + Math.Sign(deltaFps) * tlodStep;

            if (DynamicLodSetting.TlodMinOnGround && SimData.AltAboveGround <= DynamicLodSetting.AltTlodBase)
            {
                newTlod = DynamicLodSetting.TlodMin;
            }
            else if (newTlod < DynamicLodSetting.TlodMin)
            {
                newTlod = DynamicLodSetting.TlodMin;
            }
            else if (newTlod > DynamicLodSetting.TlodMax)
            {
                newTlod = DynamicLodSetting.TlodMax;
            }

            if (ReadTlod(isVr) == newTlod)
                return;

            // Adjust cloud quality if applicable
            if (DynamicLodSetting.DecreaseCloudQuality && (!DynamicLodSetting.TlodMinOnGround && !(SimData.AltAboveGround <= DynamicLodSetting.AltTlodBase)))
            {
                switch (deltaFps)
                {
                    case < 0 when newTlod < DynamicLodSetting.CloudRecoveryTlod && !_isDecreasedCloudQualityActive:
                        _isDecreasedCloudQualityActive = true;
                        WriteMemory(isVr ? _addressCloudQVr : _addressCloudQ, ReadCloudQualitySimValue(isVr) - 1); 

                        _lastLodUpdateTime =
                            _lastLodUpdateTime.AddSeconds(2); // Add extra delay for cloud setting to take effect
                        Debug.WriteLine("New Cloud Quality written - 2.");

                        return;
                    case > 0 when newTlod >= DynamicLodSetting.CloudRecoveryTlod && _isDecreasedCloudQualityActive:
                        _isDecreasedCloudQualityActive = false;
                        WriteMemory(isVr ? _addressCloudQVr : _addressCloudQ, ReadCloudQualitySimValue(isVr) + 1); 

                        _lastLodUpdateTime = _lastLodUpdateTime.AddSeconds(2);
                        Debug.WriteLine("New Cloud Quality written - 3.");

                        return;
                }
            }

            Debug.WriteLine($"New TLOD written - {newTlod}.");
            WriteMemory(isVr ? _addressTlodVr : _addressTlod, newTlod / 100.0f);
        }

        private void SetOlod(bool isVr = false)
        {
            int newOlod;
            
            if (SimData.AltAboveGround < DynamicLodSetting.AltOlodBase)
            {
                newOlod = DynamicLodSetting.OlodBase;
            }
            else if (SimData.AltAboveGround > DynamicLodSetting.AltOlodTop)
            {
                newOlod = DynamicLodSetting.OlodTop;
            }
            else
            {
                newOlod = Convert.ToInt32(DynamicLodSetting.OlodBase - (DynamicLodSetting.OlodBase - DynamicLodSetting.OlodTop) * (SimData.AltAboveGround - DynamicLodSetting.AltOlodBase) / (DynamicLodSetting.AltOlodTop - DynamicLodSetting.AltOlodBase));
            }

            if (ReadOlod(isVr) == newOlod)
                return;

            Debug.WriteLine($"New OLOD written - {newOlod}.");
            WriteMemory(isVr ? _addressOlodVr : _addressOlod, newOlod / 100.0f);
        }
    }
}
