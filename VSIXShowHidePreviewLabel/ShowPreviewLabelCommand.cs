using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using VSIXShowHidePreviewLabel.Helpers;
using Task = System.Threading.Tasks.Task;

namespace VSIXShowHidePreviewLabel
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ShowPreviewLabelCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("18cf747f-0eed-47f8-8f03-f980be371805");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowPreviewLabelCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ShowPreviewLabelCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            menuItem.Checked = IsInfoBadgeButtonVisible();
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ShowPreviewLabelCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ShowPreviewLabelCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            // Wait until the MainWindow and the InfoBadgeButton is loaded
            while (GetInfoBadgeButton() == null)
            {
                await Task.Delay(1000);
            }

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowPreviewLabelCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var infoBadgeButton = GetInfoBadgeButton();

            if (infoBadgeButton != null)
            {
                infoBadgeButton.Visibility = infoBadgeButton.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                var command = sender as MenuCommand;
                command.Checked = infoBadgeButton.Visibility == Visibility.Visible;
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                      this.package,
                      "The InfoBadgeButton was not found in this Visual Studio instance",
                      "Info",
                      OLEMSGICON.OLEMSGICON_INFO,
                      OLEMSGBUTTON.OLEMSGBUTTON_OK,
                      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static Button GetInfoBadgeButton()
        {
            // Note: The Preview label in the top right corner of Visual Studio is
            //       actually a WPF Button with the name InfoBadgeButton.
            //       This method grabs that Button and returns it

            const string NameOfInfoBadgeButton = "InfoBadgeButton";

            Window visualStudioMainWindow = WpfWindowHelper.GetMainWindowOfCurrentProcess();

            Button previewButton = visualStudioMainWindow.GetChildElementByName<Button>(NameOfInfoBadgeButton);

            return previewButton;
        }

        private bool IsInfoBadgeButtonVisible()
        {
            return GetInfoBadgeButton()?.Visibility == Visibility.Visible;
        }
    }
}
