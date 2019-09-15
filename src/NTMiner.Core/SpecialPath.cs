﻿using NTMiner.Core;
using System.IO;

namespace NTMiner {
    public static class SpecialPath {
        static SpecialPath() {
            string daemonDirFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "Daemon");
            if (!Directory.Exists(daemonDirFullName)) {
                Directory.CreateDirectory(daemonDirFullName);
            }
            DaemonFileFullName = Path.Combine(daemonDirFullName, "NTMinerDaemon.exe");
            DevConsoleFileFullName = Path.Combine(daemonDirFullName, "DevConsole.exe");

            TempDirFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "Temp");
            if (!Directory.Exists(TempDirFullName)) {
                Directory.CreateDirectory(TempDirFullName);
            }
            ServerDbFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "server.litedb");
            ServerJsonFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "server.json");

            LocalDbFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "local.litedb");
            LocalJsonFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "local.json");
            GpuProfilesJsonFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "gpuProfiles.json");
            WorkerEventDbFileFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "workerEvent.litedb");
            if (AssemblyInfo.IsLocalDir && !File.Exists(AssemblyInfo.RootLockFileFullName)) {
                if (VirtualRoot.IsMinerClient) {
                    #region 迁移
                    string sharePackagesDir = Path.Combine(AssemblyInfo.ShareDirFullName, "Packages");
                    if (Directory.Exists(sharePackagesDir)) {
                        foreach (var fileFullName in Directory.GetFiles(sharePackagesDir)) {
                            string destFileName = Path.Combine(PackagesDirFullName, Path.GetFileName(fileFullName));
                            if (!File.Exists(destFileName)) {
                                File.Copy(fileFullName, destFileName);
                            }
                        }
                    }
                    string shareServerDbFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "server.litedb");
                    if (File.Exists(shareServerDbFileFullName) && !File.Exists(ServerDbFileFullName)) {
                        File.Copy(shareServerDbFileFullName, ServerDbFileFullName);
                    }
                    string shareServerJsonFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "server.json");
                    if (File.Exists(shareServerJsonFileFullName) && !File.Exists(ServerJsonFileFullName)) {
                        File.Copy(shareServerJsonFileFullName, ServerJsonFileFullName);
                    }
                    string shareLocalDbFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "local.litedb");
                    if (File.Exists(shareLocalDbFileFullName) && !File.Exists(LocalDbFileFullName)) {
                        File.Copy(shareLocalDbFileFullName, LocalDbFileFullName);
                    }
                    string shareLocalJsonFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "local.json");
                    if (File.Exists(shareLocalJsonFileFullName) && !File.Exists(LocalJsonFileFullName)) {
                        File.Copy(shareLocalJsonFileFullName, LocalJsonFileFullName);
                    }
                    string shareGpuProfilesJsonFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "gpuProfiles.json");
                    if (File.Exists(shareGpuProfilesJsonFileFullName) && !File.Exists(GpuProfilesJsonFileFullName)) {
                        File.Copy(shareGpuProfilesJsonFileFullName, GpuProfilesJsonFileFullName);
                    }
                    string shareWorkerEventDbFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "workerEvent.litedb");
                    if (File.Exists(shareWorkerEventDbFileFullName) && !File.Exists(WorkerEventDbFileFullName)) {
                        File.Copy(shareWorkerEventDbFileFullName, WorkerEventDbFileFullName);
                    }
                    string shareUpdaterFileFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "Updater", "NTMinerUpdater.exe");
                    if (File.Exists(shareUpdaterFileFullName) && !File.Exists(UpdaterFileFullName)) {
                        File.Copy(shareUpdaterFileFullName, UpdaterFileFullName);
                    }
                    #endregion
                    File.Move(AssemblyInfo.RootConfigFileFullName, AssemblyInfo.RootLockFileFullName);
                }
            }
        }

        public static string GetIconFileFullName(ICoin coin) {
            if (coin == null || string.IsNullOrEmpty(coin.Icon)) {
                return string.Empty;
            }
            string iconFileFullName = Path.Combine(CoinIconsDirFullName, coin.Icon);
            return iconFileFullName;
        }

        public static string ReadServerJsonFile() {
            if (File.Exists(ServerJsonFileFullName)) {
                return File.ReadAllText(ServerJsonFileFullName);
            }

            return string.Empty;
        }

        public static void WriteServerJsonFile(string json) {
            File.WriteAllText(ServerJsonFileFullName, json);
        }

        public static string ReadLocalJsonFile() {
            if (File.Exists(LocalJsonFileFullName)) {
                return File.ReadAllText(LocalJsonFileFullName);
            }

            return string.Empty;
        }

        public static string ReadGpuProfilesJsonFile() {
            if (File.Exists(GpuProfilesJsonFileFullName)) {
                return File.ReadAllText(GpuProfilesJsonFileFullName);
            }

            return string.Empty;
        }

        public static void WriteGpuProfilesJsonFile(string json) {
            File.WriteAllText(GpuProfilesJsonFileFullName, json);
        }

        public static readonly string LocalDbFileFullName;
        public static readonly string LocalJsonFileFullName;
        public static readonly string GpuProfilesJsonFileFullName;

        public static readonly string WorkerEventDbFileFullName;
        public static readonly string ServerDbFileFullName;

        public static readonly string ServerJsonFileFullName;

        public static readonly string DaemonFileFullName;

        public static readonly string DevConsoleFileFullName;

        public static readonly string TempDirFullName;

        private static bool _sIsFirstCallPackageDirFullName = true;
        public static string PackagesDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "Packages");
                if (_sIsFirstCallPackageDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallPackageDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallCoinIconDirFullName = true;
        public static string CoinIconsDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.ShareDirFullName, "CoinIcons");
                if (_sIsFirstCallCoinIconDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallCoinIconDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallDownloadDirFullName = true;
        public static string DownloadDirFullName {
            get {
                string dirFullName = Path.Combine(TempDirFullName, "Download");
                if (_sIsFirstCallDownloadDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallDownloadDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallKernelsDirFullName = true;
        public static string KernelsDirFullName {
            get {
                string dirFullName = Path.Combine(TempDirFullName, "Kernels");
                if (_sIsFirstCallKernelsDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallKernelsDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallLogsDirFullName = true;
        public static string LogsDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "Logs");
                if (_sIsFirstCallLogsDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallLogsDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallToolsDirFullName = true;
        public static string ToolsDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "Tools");
                if (_sIsFirstCallToolsDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallToolsDirFullName = false;
                }

                return dirFullName;
            }
        }

        private static bool _sIsFirstCallUpdaterDirFullName = true;
        public static string UpdaterDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "Updater");
                if (_sIsFirstCallUpdaterDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallUpdaterDirFullName = false;
                }

                return dirFullName;
            }
        }

        public static string UpdaterFileFullName {
            get {
                return Path.Combine(UpdaterDirFullName, "NTMinerUpdater.exe");
            }
        }

        private static bool _sIsFirstCallServicesDirFullName = true;
        public static string ServicesDirFullName {
            get {
                string dirFullName = Path.Combine(AssemblyInfo.LocalDirFullName, "Services");
                if (_sIsFirstCallServicesDirFullName) {
                    if (!Directory.Exists(dirFullName)) {
                        Directory.CreateDirectory(dirFullName);
                    }
                    _sIsFirstCallServicesDirFullName = false;
                }

                return dirFullName;
            }
        }

        public static string ServicesFileFullName {
            get {
                return Path.Combine(ServicesDirFullName, "NTMinerServices.exe");
            }
        }
    }
}
