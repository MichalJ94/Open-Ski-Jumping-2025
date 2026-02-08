using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class AutoBackupSystem
{
    private static readonly TimeSpan MinBackupInterval =
        TimeSpan.FromMinutes(15);

    public static void BackupFile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
            return;

        string json = File.ReadAllText(sourcePath);
        if (!JsonValidation.TryDeserialize<object>(json, out _))
            return; // never back up invalid JSON

        string backupDir = Path.Combine(Application.streamingAssetsPath, "autobackup");
        Directory.CreateDirectory(backupDir);

        string fileName = Path.GetFileName(sourcePath);
        string backupPath = Path.Combine(backupDir, fileName);

        // First backup -> always create
        if (!File.Exists(backupPath))
        {
            AtomicWrite(backupPath, json);
            return;
        }

        // Throttle backups
        var lastBackupTime = File.GetLastWriteTimeUtc(backupPath);
        var timeSinceLastBackup = DateTime.UtcNow - lastBackupTime;

        if (timeSinceLastBackup < MinBackupInterval)
        {
            var minutesAgo = Mathf.FloorToInt((float)timeSinceLastBackup.TotalMinutes);
            var secondsAgo = Mathf.FloorToInt((float)timeSinceLastBackup.TotalSeconds % 60);

            Debug.Log(
                $"[AutoBackup] Skipped backup for '{Path.GetFileName(backupPath)}'. " +
                $"Last backup was {minutesAgo} min {secondsAgo} sec ago."
            );

            return;
        }

        AtomicWrite(backupPath, json);

        Debug.Log(
            $"[AutoBackup] Backup created for '{Path.GetFileName(backupPath)}'."
        );
    }

    private static void AtomicWrite(string path, string content)
    {
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        File.Replace(tmp, path, null);
    }

    // ---------------- RESTORE ----------------

    public static class AutoRestoreSystem
    {
        public static bool TryRestore<T>(string mainPath)
        {
            if (!File.Exists(mainPath))
                return TryRestoreFromBackup<T>(mainPath);

            string json = File.ReadAllText(mainPath);
            if (JsonValidation.TryDeserialize<T>(json, out _))
                return false;

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
            if (!JsonValidation.TryDeserialize<T>(backupJson, out _))
                return false;

            AtomicWrite(mainPath, backupJson);
            Debug.Log($"[AutoRestore] Restored {fileName} from autobackup");
            return true;
        }
    }

    // ---------------- JSON ----------------

    public static class JsonValidation
    {
        public static bool TryDeserialize<T>(string json, out T result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
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
