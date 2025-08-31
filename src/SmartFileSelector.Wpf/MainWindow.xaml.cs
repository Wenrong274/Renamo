using SmartFileSelector.Core;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFileSelector.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SmartFileSelectorController smartFileSelectorController = new SmartFileSelectorController();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            smartFileSelectorController.RenameFiles(FolderText.Text, FileNamaRuleText.Text);
        }
    }
}