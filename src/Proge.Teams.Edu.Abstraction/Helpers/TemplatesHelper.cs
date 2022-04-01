using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction.Helpers
{
    public static class TemplatesHelper
    {
        public static async Task<string> ProcessTemplateAsync(string path, IDictionary<string, string> t)
        {
            string tmpl = await ReadTemplateAsync(path);
            string result = Execute(tmpl, t);
            return result;
        }

        public static async Task<string> ReadTemplateAsync(string path)
        {
            using (var reader = File.OpenText(path))
            {
                string fileText = await reader.ReadToEndAsync();
                return fileText;
            }
        }

        public static string Execute(string tmpl, IDictionary<string, string> t)
        {
            foreach (string k in t.Keys)
            {
                tmpl = tmpl.Replace($"{{{{{k.ToLower()}}}}}", t[k]);
            }
            return tmpl;
        }

        public static string Execute<C>(string tmpl, C c) where C : class
        {
            IDictionary<string, string> t = ClassToDictionary(string.Empty, c);
            return Execute(tmpl, t);
        }

        public readonly static IEnumerable<Type> sysTypes = 0.GetType().Assembly.GetTypes().Where(t => t.IsPrimitive && t.IsPublic);
        public static IDictionary<string, string> ClassToDictionary<C>(string prefix, C c) where C : class
        {
            PropertyInfo[] fields = c.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<KeyValuePair<string, string>> kvs = new List<KeyValuePair<string, string>>();

            foreach (var field in fields)
            {
                var fieldType = field.GetType();
                var v = field.GetValue(c);
                if (v == null)
                    continue;
                var n = ((MemberInfo)field).Name;

                bool isPrimitive = v is string || sysTypes.Any(t => v.GetType().FullName == t.FullName);
                string key = $"{prefix}{n}";
                if (isPrimitive)
                {
                    kvs.Add(new KeyValuePair<string, string>(key, v.ToString()));
                }
                else if (fieldType.IsArray)
                {
                    throw new NotSupportedException("Arrays are not supported");
                }
                else
                {
                    IDictionary<string, string> d = ClassToDictionary($"{key}.", v);
                    kvs.AddRange(d.ToList());
                }
            }

            IDictionary<string, string> dict = kvs.ToDictionary(e => e.Key, e => e.Value);
            return dict;
        }
    }
}