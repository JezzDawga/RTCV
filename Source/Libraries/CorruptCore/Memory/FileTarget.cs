namespace RTCV.CorruptCore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public enum FileTargetStatus
    {
        UNBACKED = 0,
        BACKED = 1,
        DIRTY = 2,

    }

    public enum FileTargetLocation
    {
        REAL,
        WORKING,
        BACKUP,
        WORKINGFOLDER,
        BACKUPFOLDER
    }

    [Serializable()]
    public class FileTarget
    {

        public string FilePath { get; set; } = "";
        public string BaseDir { get; set; } = "";
        public long PaddingHeader { get; set; } = 0;
        public long PaddingFooter { get; set; } = 0;
        public bool IsMain { get; set; } = false;

        public string OriginalChecksum { get; set; } = null;
        public long OriginalSize { get; set; } = -1;
        public bool isDirty { get; set; } = false;

        public FileTarget(string filePath, string baseDir)
        {
            FilePath = filePath;

            if (!string.IsNullOrWhiteSpace(baseDir))
                BaseDir = baseDir;
        }

        public string getUniqueId()
        {
            string CreateMd5HashString(byte[] input)
            {
                var hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(input);
                return string.Join("", hashBytes.Select(b => b.ToString("X")));
            }

            string basepart = "";
            if (!string.IsNullOrWhiteSpace(BaseDir))
                basepart = CreateMd5HashString(System.Text.Encoding.UTF8.GetBytes(BaseDir));

            string filepart = CreateMd5HashString(System.Text.Encoding.UTF8.GetBytes(FilePath));

            return $"{basepart}$${filepart}";

        }

        public string RealFilePath => BaseDir + FilePath;
        public string WorkingFilePath => Path.Combine(Vault.vaultWorkingPath, getUniqueId(), new FileInfo(RealFilePath).Name);
        public string BackupFilePath => Path.Combine(Vault.vaultBackupsPath, getUniqueId(), new FileInfo(RealFilePath).Name);

        public bool SetBaseDir(string baseDir)
        {
            if (baseDir == null)
                return true;

            if (!FilePath.Contains(baseDir))
                return false;

            BaseDir = baseDir;
            FilePath = FilePath.Replace(baseDir, "");
            return true;
        }

        public string GetPathFromLocation(FileTargetLocation location)
        {
            switch (location)
            {
                case FileTargetLocation.BACKUP:
                    return BackupFilePath;
                case FileTargetLocation.BACKUPFOLDER:
                    return new FileInfo(BackupFilePath).DirectoryName;
                case FileTargetLocation.WORKING:
                    return WorkingFilePath;
                case FileTargetLocation.WORKINGFOLDER:
                    return new FileInfo(WorkingFilePath).DirectoryName;
                default:
                case FileTargetLocation.REAL:
                    return RealFilePath;
            }
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
