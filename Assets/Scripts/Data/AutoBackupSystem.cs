using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class AutoBackupSystem
{
    private const long MinSizeBytes = 1024;

    public static void BackupFile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
            return;

        var info = new FileInfo(sourcePath);
        if (info.Length < 0.2*MinSizeBytes)
            return;

        string backupDir = Path.Combine(Application.streamingAssetsPath, "autobackup");
        Directory.CreateDirectory(backupDir);

        string fileName = Path.GetFileName(sourcePath);
        string backupPath = Path.Combine(backupDir, fileName);

        File.Copy(sourcePath, backupPath, overwrite: true);
    }

    public static class AutoRestoreSystem
    {
        public static bool TryRestore<T>(string mainPath)
        {
            if (!File.Exists(mainPath))
                return TryRestoreFromBackup<T>(mainPath);

            string json = File.ReadAllText(mainPath);

            // JSON validation instead of size heuristic
            if (JsonValidation.IsValidJson<T>(json, out _))
                return false; // file is OK

            return TryRestoreFromBackup<T>(mainPath);
        }

        private static bool TryRestoreFromBackup<T>(string mainPath)
        {
            string fileName = Path.GetFileName(mainPath);
            string backupPath = Path.Combine(
                Application.streamingAssetsPath,
                "autobackup",
                fileName
            );

            if (!File.Exists(backupPath))
                return false;

            string backupJson = File.ReadAllText(backupPath);

            if (!JsonValidation.IsValidJson<T>(backupJson, out _))
                return false;

            File.WriteAllText(mainPath, backupJson);
            Debug.Log($"[AutoRestore] Restored {fileName} from autobackup");
            return true;
        }
    }

    public static class JsonValidation
    {
        public static bool IsValidJson<T>(string json, out T result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                // syntax
                JToken.Parse(json);

                // semantic
                result = JsonConvert.DeserializeObject<T>(json);
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}