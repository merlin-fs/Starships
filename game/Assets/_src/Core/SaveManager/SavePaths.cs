using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Game.Core.Saves
{
    public class Paths
    {
        private const string SAVE_DATA_NAME = "save";
        private const string SAVE_PATH = "saves";
        private const string SAVE_EXT = "json";

        public static string GetPath(string name = null)
        {
            name = string.IsNullOrEmpty(name) ? SAVE_DATA_NAME : name;
            name = Path.ChangeExtension(name, SAVE_EXT);
            var path = Path.Combine(Application.persistentDataPath, SAVE_PATH);
            path = Path.Combine(path, name);
            return path;
        }
    }
}
