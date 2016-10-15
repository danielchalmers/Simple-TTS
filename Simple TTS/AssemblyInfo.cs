#region

using System;
using System.Deployment.Application;
using System.Reflection;
using Simple_TTS.Properties;

#endregion

namespace Simple_TTS
{
    public static class AssemblyInfo
    {
        public static Version Version { get; } = ApplicationDeployment.IsNetworkDeployed
            ? ApplicationDeployment.CurrentDeployment.CurrentVersion
            : Assembly.GetExecutingAssembly().GetName().Version;

        public static string Copyright { get; } = GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright);
        public static string Title { get; } = GetAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title);

        public static string CustomDescription { get; } = string.Format(Resources.About, Title, Version,
            Resources.Website, Copyright);

        private static string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            var attribute = (T) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof (T));
            return value.Invoke(attribute);
        }
    }
}