using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartFileSelector.Wpf
{
    public class SmartFileSelectorController
    {

        public SmartFileSelectorController()
        {

        }


        public void RenameFiles(string folder, string pattern)
        {
            try
            {
                Core.FileRenamer.RenameWithPattern(folder, pattern);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}