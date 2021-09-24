using System;
using System.Collections.Generic;
using System.IO;
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

namespace ReplaceFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static byte[] copyBytes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void dirButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fileBrowser = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = fileBrowser.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fileBrowser.SelectedPath))
                {
                    dirTextBlock.Text = fileBrowser.SelectedPath;
                    return;
                }
            }

            MessageBox.Show("Please select a valid path.");
        }

        private void copyObjectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fileBrowser = new System.Windows.Forms.OpenFileDialog())
            {
                var result = fileBrowser.ShowDialog();

                copyObjectTextBlock.Text = fileBrowser.FileName.Split('\\').LastOrDefault();

                var file = fileBrowser.OpenFile();

                copyBytes = File.ReadAllBytes(fileBrowser.FileName);

                file.Close();
            }

            //MessageBox.Show("Please select a valid file.");
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(dirTextBlock.Text) && copyBytes != null && copyBytes.Length > 0)
            {
                this.Cursor = Cursors.Wait;

                ReplaceFilesInDirectory(dirTextBlock.Text, copyObjectTextBlock.Text, copyBytes);

                this.Cursor = Cursors.Arrow;

                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("Please select a valid path.");
            }
        }

        private void ReplaceFilesInDirectory(string directory, string copyObjectName, byte[] copyObjectBytes)
        {
            var subDirectories = Directory.GetDirectories(directory);
            var files = Directory.GetFiles(directory);

            Task<bool> t = Task.Run(() =>
            {
                // Delete files
                foreach (var file in files)
                {
                    var fileName = file.Split('\\').LastOrDefault();

                    if (fileName.ToLower() == copyObjectName.ToLower())
                    {
                        File.WriteAllBytes(file, copyObjectBytes);
                    }
                }

                return true;
            });

            // Do nothing until the task is completed
            while (t.IsCompleted == false) ;
            t.Dispose();

            Task<bool> d = Task.Run((Func<bool>)(() =>
            {
                // Send deletion into the subdirectories
                foreach (var _dir in subDirectories)
                    ReplaceFilesInDirectory(_dir, copyObjectName, copyObjectBytes);

                return true;
            }));

            // Do nothing until the task is completed
            while (d.IsCompleted == false) ;
            d.Dispose();
        }
    }
}