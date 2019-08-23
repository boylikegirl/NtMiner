﻿using NTMiner.Core.Gpus;
using NTMiner.JsonDb;
using NTMiner.MinerClient;
using NTMiner.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTMiner.Core.Profiles.Impl {
    public class GpuProfileSet : IGpuProfileSet {
        private GpuProfilesJsonDb _data = new GpuProfilesJsonDb();

        public GpuProfileSet(INTMinerRoot root) {
            VirtualRoot.Window<AddOrUpdateGpuProfileCommand>("处理添加或更新Gpu超频数据命令", LogEnum.DevConsole,
                action: message => {
                    GpuProfileData data = _data.GpuProfiles.FirstOrDefault(a => a.CoinId == message.Input.CoinId && a.Index == message.Input.Index);
                    if (data != null) {
                        data.Update(message.Input);
                        Save();
                    }
                    else {
                        data = new GpuProfileData(message.Input);
                        _data.GpuProfiles.Add(data);
                        Save();
                    }
                    VirtualRoot.Happened(new GpuProfileAddedOrUpdatedEvent(data));
                });
            VirtualRoot.Window<CoinOverClockCommand>("处理币种超频命令", LogEnum.DevConsole,
                action: message => {
                    if (message.IsJoin) {
                        CoinOverClock(root, message.CoinId);
                    }
                    else {
                        Task.Factory.StartNew(() => {
                            CoinOverClock(root, message.CoinId);
                        });
                    }
                });
        }

        public void Refresh() {
            _isInited = false;
            VirtualRoot.Happened(new GpuProfileSetRefreshedEvent());
        }

        public bool IsOverClockEnabled(Guid coinId) {
            InitOnece();
            var item = _data.CoinOverClocks.FirstOrDefault(a => a.CoinId == coinId);
            if (item == null) {
                return false;
            }
            return item.IsOverClockEnabled;
        }

        public void SetIsOverClockEnabled(Guid coinId, bool value) {
            InitOnece();
            var item = _data.CoinOverClocks.FirstOrDefault(a => a.CoinId == coinId);
            if (item == null) {
                item = new CoinOverClockData() {
                    CoinId = coinId,
                    IsOverClockEnabled = value,
                    IsOverClockGpuAll = true
                };
                _data.CoinOverClocks.Add(item);
            }
            else {
                item.IsOverClockEnabled = value;
            }
            Save();
        }

        public bool IsOverClockGpuAll(Guid coinId) {
            InitOnece();
            var item = _data.CoinOverClocks.FirstOrDefault(a => a.CoinId == coinId);
            if (item == null) {
                return true;
            }
            return item.IsOverClockGpuAll;
        }

        public void SetIsOverClockGpuAll(Guid coinId, bool value) {
            InitOnece();
            var item = _data.CoinOverClocks.FirstOrDefault(a => a.CoinId == coinId);
            if (item == null) {
                item = new CoinOverClockData() {
                    CoinId = coinId,
                    IsOverClockEnabled = false,
                    IsOverClockGpuAll = value
                };
                _data.CoinOverClocks.Add(item);
            }
            else {
                item.IsOverClockGpuAll = value;
            }
            Save();
        }

        public IGpuProfile GetGpuProfile(Guid coinId, int index) {
            InitOnece();
            GpuProfileData data = _data.GpuProfiles.FirstOrDefault(a => a.CoinId == coinId && a.Index == index);
            if (data == null) {
                return new GpuProfileData(coinId, index);
            }
            return data;
        }

        #region private methods
        private GpuData[] CreateGpus() {
            List<GpuData> list = new List<GpuData>();
            foreach (var gpu in NTMinerRoot.Instance.GpuSet) {
                list.Add(new GpuData {
                    Index = gpu.Index,
                    Name = gpu.Name,
                    CoreClockDeltaMax = gpu.CoreClockDeltaMax,
                    CoreClockDeltaMin = gpu.CoreClockDeltaMin,
                    MemoryClockDeltaMax = gpu.MemoryClockDeltaMax,
                    MemoryClockDeltaMin = gpu.MemoryClockDeltaMin,
                    CoolMax = gpu.CoolMax,
                    CoolMin = gpu.CoolMin,
                    PowerMax = gpu.PowerMax,
                    PowerMin = gpu.PowerMin,
                    TempLimitDefault = gpu.TempLimitDefault,
                    TempLimitMax = gpu.TempLimitMax,
                    TempLimitMin = gpu.TempLimitMin
                });
            }
            return list.ToArray();
        }

        private void CoinOverClock(INTMinerRoot root, Guid coinId) {
            try {
                if (IsOverClockGpuAll(coinId)) {
                    GpuProfileData overClockData = _data.GpuProfiles.FirstOrDefault(a => a.CoinId == coinId && a.Index == NTMinerRoot.GpuAllId);
                    if (overClockData != null) {
                        OverClock(root, overClockData);
                    }
                }
                else {
                    foreach (var overClockData in _data.GpuProfiles.Where(a => a.CoinId == coinId)) {
                        if (overClockData.Index != NTMinerRoot.GpuAllId) {
                            OverClock(root, overClockData);
                        }
                    }
                }
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
            }
        }

        private void OverClock(INTMinerRoot root, IGpuProfile data) {
#if DEBUG
            Write.Stopwatch.Restart();
#endif
            if (root.GpuSet.TryGetGpu(data.Index, out IGpu gpu)) {
                IOverClock overClock = root.GpuSet.OverClock;
                if (!data.IsAutoFanSpeed) {
                    overClock.SetFanSpeed(data.Index, data.Cool);
                }
                overClock.SetCoreClock(data.Index, data.CoreClockDelta, data.CoreVoltage);
                overClock.SetMemoryClock(data.Index, data.MemoryClockDelta, data.MemoryVoltage);
                overClock.SetPowerLimit(data.Index, data.PowerCapacity);
                overClock.SetTempLimit(data.Index, data.TempLimit);
                if (data.Index == NTMinerRoot.GpuAllId) {
                    Write.UserLine($"统一超频：{data.ToOverClockString()}", "OverClock", ConsoleColor.Yellow);
                }
                else {
                    Write.UserLine($"GPU{gpu.Index}超频：{data.ToOverClockString()}", "OverClock", ConsoleColor.Yellow);
                }
                overClock.RefreshGpuState(data.Index);
            }
#if DEBUG
            Write.DevWarn($"耗时{Write.Stopwatch.ElapsedMilliseconds}毫秒 {this.GetType().Name}.OverClock");
#endif
        }

        private void Save() {
            _data.Gpus = CreateGpus();
            string json = VirtualRoot.JsonSerializer.Serialize(_data);
            SpecialPath.WriteGpuProfilesJsonFile(json);
        }

        private bool _isInited = false;
        private readonly object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    string json = SpecialPath.ReadGpuProfilesJsonFile();
                    if (!string.IsNullOrEmpty(json)) {
                        // 反序列化不报异常，但如果格式不正确返回值可能为null
                        GpuProfilesJsonDb data = VirtualRoot.JsonSerializer.Deserialize<GpuProfilesJsonDb>(json);
                        if (data != null) {
                            _data = data;
                        }
                        else {
                            Save();
                        }
                    }
                    else {
                        Save();
                    }
                    _isInited = true;
                }
            }
        }
        #endregion
    }
}