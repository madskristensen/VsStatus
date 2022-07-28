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
            try
            {
                using (HttpClient client = new())
                {
                    string json = await client.GetStringAsync(_apiUrl);
                    StatusMessage status = JsonConvert.DeserializeObject<StatusMessage>(json);

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    await EnsureUIAsync();
                    SetStatus(status);
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        private static async Task EnsureUIAsync()
        {
            if (_panel == null)
            {
                _panel = FindChild(Application.Current.MainWindow, "StatusBarPanel") as Panel;

                if (_panel == null)
                {
                    // The start window is being displayed and the status bar hasn't been rendered
                    await Task.Delay(5000);
                    await EnsureUIAsync();
                    return;
                }

                _icon = new();
                _icon.PreviewMouseLeftButtonUp += (s, e) => { _icon.ContextMenu.IsOpen = true; };
                _icon.HorizontalAlignment = HorizontalAlignment.Left;
                _icon.MaxWidth = 16;

                _icon.ContextMenu = new ContextMenu();
                MenuItem refresh = new() { Header = "Refresh" };
                refresh.Click += (s, e) => { _icon.Moniker = KnownMonikers.FTPConnection; UpdateStatusAsync().FireAndForget(); };
                _icon.ContextMenu.Items.Add(refresh);

                MenuItem open = new() { Header = "Open Status Page" };
                open.Click += (s, e) => { Process.Start(_statusUrl); };
                _icon.ContextMenu.Items.Add(open);
                
                _panel.Children.Insert(5, _icon);
            }            
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
            _icon.ToolTip = $"Status: {statusShort}";
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
