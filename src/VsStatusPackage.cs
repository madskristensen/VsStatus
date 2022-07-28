﻿global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;

namespace VsStatus
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.VsStatusString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VsStatusPackage : ToolkitPackage
    {
        private const double _interval = 30 * 60 * 1000; // 30 minutes

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();

            StatusBarInjector.UpdateStatusAsync().FireAndForget();

            System.Timers.Timer timer = new(_interval)
            {
                AutoReset = true,
                Enabled = true,
            };

            timer.Elapsed += (s, e) =>
            {
                StatusBarInjector.UpdateStatusAsync().FireAndForget();
            };
        }
    }
}