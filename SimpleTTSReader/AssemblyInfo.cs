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

        public static string GetVersion()
        {
            var obj = ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.
                    CurrentVersion
                : Assembly.GetExecutingAssembly().GetName().Version;
            return $"{obj.Major}.{obj.Minor}.{obj.Build}";
        }

        public static string GetCopyright()
        {
            return GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright);
        }

        public static string GetWelcomeMessage()
        {
            var example = new StringBuilder();
            example.AppendLine(
                $"WARNING: This application is currently in beta and will receive frequent updates and may have issues.");
            example.AppendLine();
            example.AppendLine($"Hello {Environment.UserName}.");
            example.AppendLine($"Welcome to Simple Text to Speech Reader version {GetVersion()}.");
            example.AppendLine();
            example.AppendLine(
                $"If you want to change Text to Speech voice properties such as gender, speed, and volume, use the options on the right.");
            example.AppendLine($"To start, stop, or pause voice playback, use the buttons on the left.");
            example.AppendLine($"You can find settings, help, etc in the menu above.");
            example.AppendLine();
            example.AppendLine();
            example.AppendLine(
                $"All features in this application are available free of charge, but feel free to leave a donation at \"{Resources.DonateLink}\"");
            example.AppendLine();
            example.AppendLine(
                $"If you have any issues or requests, you can report them at \"{Resources.GitHubIssues}\".");
            return example.ToString();
        }
    }
}