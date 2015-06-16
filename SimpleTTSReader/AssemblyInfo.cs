#region

using System;
using System.Deployment.Application;
using System.Reflection;
using System.Text;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class AssemblyInfo
    {
        public static string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            var attribute = (T) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof (T));
            return value.Invoke(attribute);
        }

        public static string GetVersionString()
        {
            var obj = ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.
                    CurrentVersion
                : Assembly.GetExecutingAssembly().GetName().Version;
            return $"{obj.Major}.{obj.Minor}.{obj.Build}";
        }

        public static string GetVersionString(Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public static Version GetVersion()
        {
            var obj = ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.
                    CurrentVersion
                : Assembly.GetExecutingAssembly().GetName().Version;
            return obj;
        }

        public static string GetCopyright()
        {
            return GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright);
        }

        public static string GetTitle()
        {
            return GetAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title);
        }

        public static string GetAboutDescription()
        {
            var example = new StringBuilder();
            example.AppendLine($"Simple TTS Reader (v{GetVersionString()}).");
            example.AppendLine();
            example.AppendLine($"Issues: {Resources.GitHubIssues}");
            example.AppendLine($"Donations: {Resources.DonateLink}");
            example.AppendLine();
            example.AppendLine(
                $"Icons are made by Google (https://www.google.com) from Flaticon (http://www.flaticon.com) and are licensed under CC BY 3.0 (https://creativecommons.org/licenses/by/3.0/)");
            return example.ToString();
        }
    }
}