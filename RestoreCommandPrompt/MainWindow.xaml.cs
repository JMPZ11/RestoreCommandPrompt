using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestoreCommandPrompt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateStatus();


        }

        void UpdateStatus()
        {
            chkFolderStatus.IsChecked = RestoreTool.CheckFolderStatus();
            chkFolderBackStatus.IsChecked = RestoreTool.CheckFolderBackgroundStatus();
            chkExplorerStatus.IsChecked = RestoreTool.CheckExplorerRibbonStatus();

            if (chkFolderStatus.IsChecked.GetValueOrDefault())
            {
                btnFolderContextEnable.IsEnabled = false;
                btnFolderContextDisable.IsEnabled = true;
            }
            else
            {
                btnFolderContextEnable.IsEnabled = true;
                btnFolderContextDisable.IsEnabled = false;
            }

            if (chkFolderBackStatus.IsChecked.GetValueOrDefault())
            {
                btnFolderBackEnable.IsEnabled = false;
                btnFolderBackDisable.IsEnabled = true;
            }
            else
            {
                btnFolderBackEnable.IsEnabled = true;
                btnFolderBackDisable.IsEnabled = false;
            }

           

            if (chkExplorerStatus.IsChecked.GetValueOrDefault())
            {
                btnExplorerEnable.IsEnabled = false;
                btnExplorerDisable.IsEnabled = true;
            }
            else
            {
                btnExplorerEnable.IsEnabled = true;
                btnExplorerDisable.IsEnabled = false;
            }


        }

        private void btnFolderContextEnable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.EnableFolder();
            UpdateStatus();
        }

        private void btnFolderContextDisable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.DisableFolder();
            UpdateStatus();
        }

        private void btnFolderBackEnable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.EnableFolderBackground();
            UpdateStatus();
        }

        private void btnFolderBackDisable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.DisableFolderBackground();
            UpdateStatus();
        }

        private void btnExplorerEnable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.ReplacePowershellWithCmd();
            UpdateStatus();
        }

        private void btnExplorerDisable_Click(object sender, RoutedEventArgs e)
        {
            RestoreTool.RestorePowershellRibbon();
            UpdateStatus();
        }

    }
}
