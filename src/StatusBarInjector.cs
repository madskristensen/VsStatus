using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Newtonsoft.Json;

namespace VsStatus
{
    public class StatusBarInjector
    {
        private const string _statusUrl = "https://status.visualstudio.microsoft.com/";
        private const string _apiUrl = "https://status.visualstudio.microsoft.com/api/status";
        private static Panel _panel;
        private static CrispImage _icon;

        public static async Task UpdateStatusAsync()
        {
            using (HttpClient client = new())
            {
                string json = await client.GetStringAsync(_apiUrl);
                StatusMessage status = JsonConvert.DeserializeObject<StatusMessage>(json);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                SetIcon(status);
            }
        }

        private static void SetIcon(StatusMessage status)
        {
            if (_panel == null)
            {
                _icon = new();
                _icon.PreviewMouseLeftButtonUp += (s, e) => { Process.Start(_statusUrl); };
                _icon.HorizontalAlignment = HorizontalAlignment.Right;

                FrameworkElement statusBar = FindChild(Application.Current.MainWindow, "StatusBarContainer") as FrameworkElement;
                _panel = statusBar?.Parent as Panel;
                _panel.Children.Add(_icon);
            }

            SetStatus(status);
        }

        private static void SetStatus(StatusMessage status)
        {
            Severity severity = status.services.Max(s => s.severity);

            string statusShort = "OK";
            State state = status.services.Max(s => s.state);

            if (state != State.Resolved)
            {
                statusShort = state.ToString();
            }

            _icon.Moniker = GetImageMoniker(severity);
            _icon.ToolTip = $"Status: {statusShort}\r\n\r\nClick to open the Visual Studio Service Status page.";
        }

        private static ImageMoniker GetImageMoniker(Severity severity)
        {
            return severity switch
            {
                Severity.Unhealthy => KnownMonikers.CloudError,
                Severity.Degraded => KnownMonikers.CloudWarning,
                Severity.Advisory => KnownMonikers.CloudStopped,
                _ => KnownMonikers.CloudOK,
            };
        }

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }
            }

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                child = FindChild(child, childName);

                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
