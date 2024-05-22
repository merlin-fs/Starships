using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Common.Core
{
    public static class UnityObjectExtensions
    {
        private const char SEPARATOR = '/';
        
        [NotNull]
        public static string GetHierarchyPath(this GameObject gameObject, GameObject root = null)
        {
            var rootTransform = root?.transform;
            var builder = new StringBuilder(128);
            var iter = gameObject.transform;
            do
            {
                builder.Insert(0, iter.name);
                builder.Insert(0, SEPARATOR);
            } while ((iter = iter.parent) != rootTransform && iter.parent != rootTransform);
            builder.Remove(0, 1);
            return builder.ToString();
        }

        public static GameObject FindObjectFromPath(this GameObject gameObject, string path)
        {
            return gameObject.transform.Find(path)?.gameObject;
        }
    }
}