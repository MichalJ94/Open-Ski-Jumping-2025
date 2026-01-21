using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using static AutoBackupSystem;

namespace OpenSkiJumping.Data
{
    public abstract class RuntimeData : ScriptableObject
    {
        public abstract bool LoadData();
        public abstract void SaveData();
        public abstract void Reset();
    }

    public class DatabaseObject<T> : RuntimeData
    {
        [FormerlySerializedAs("fileName")] [SerializeField]
        protected string path;
        
        [SerializeField] protected T data;
        [SerializeField] protected bool loaded;
        [SerializeField] protected bool prettyPrint;


        public T Data
        {
            get => data;
            protected set => data = value;
        }

        public bool Loaded
        {
            get => loaded;
            set => loaded = value;
        }

        public override void Reset()
        {
            data = default;
            loaded = false;
        }

        public override bool LoadData()
        {
            var absolutePath = Path.Combine(Application.streamingAssetsPath, path);

            // validate & auto-restore
            AutoRestoreSystem.TryRestore<T>(absolutePath);

            if (!File.Exists(absolutePath))
            {
                loaded = false;
                return false;
            }

            var json = File.ReadAllText(absolutePath);
            if (!JsonValidation.IsValidJson<T>(json, out var result))
            {
                loaded = false;
                return false;
            }

            data = result;
            loaded = true;
            return true;
        }

        private bool LoadMultipleFiles(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
            {
                return false;
            }

            var fileEntries = Directory.GetFiles(absolutePath);
            foreach (var fileName in fileEntries)
                LoadSingleFile(fileName);

            // string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            // foreach (string subdirectory in subdirectoryEntries)
            //     ProcessDirectory(subdirectory);
            return true;
        }

        private bool LoadSingleFile(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                var dataAsJson = File.ReadAllText(absolutePath);
                data = JsonConvert.DeserializeObject<T>(dataAsJson);
                loaded = true;
                return true;
            }

            loaded = false;
            return false;
        }

        public override void SaveData()
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, path);

            // Serialize first
            var json = JsonConvert.SerializeObject(
                data,
                prettyPrint ? Formatting.Indented : Formatting.None
            );

            // SAFETY: do not overwrite with empty/null
            if (string.IsNullOrEmpty(json) || data == null)
            {
                Debug.LogWarning($"Skipped saving empty data: {path}");
                return;
            }

            // Write atomically
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Replace(tempPath, filePath, null);

            // Backup AFTER successful save
            AutoBackupSystem.BackupFile(filePath);
        }
    }
}