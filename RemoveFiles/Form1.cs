using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RemoveFiles
{
    public partial class Form1 : Form
    {
        private string extensionToDelete;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            extensionToDelete = textBox1.Text;

            if (string.IsNullOrWhiteSpace(extensionToDelete))
            {
                MessageBox.Show("Please input a valid extention.");
                return;
            }

            using (var fileBrowser = new FolderBrowserDialog())
            {
                DialogResult result = fileBrowser.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileBrowser.SelectedPath))
                {
                    this.UseWaitCursor = true;

                    DeleteFilesFromDirectory(fileBrowser.SelectedPath);

                    this.UseWaitCursor = false;

                    MessageBox.Show("Done!");
                    return;
                }
            }

            MessageBox.Show("Please select a valid path.");
        }

        private void DeleteFilesFromDirectory(string directory)
        {
            var subDirectories = Directory.GetDirectories(directory);
            var files = Directory.GetFiles(directory);

            Task<bool> t = Task.Run(() =>
            {
                // Delete files
                foreach (var file in files)
                {
                    var extention = file.Split('.').LastOrDefault();

                    if (extention.ToLower() == extensionToDelete.ToLower()) File.Delete(file);
                }

                return true;
            });

            // Do nothing until the task is completed
            while (t.IsCompleted == false) ;
            t.Dispose();

            Task<bool> d = Task.Run(() =>
            {
                // Send deletion into the subdirectories
                foreach (var _dir in subDirectories)
                    DeleteFilesFromDirectory(_dir);

                return true;
            });

            // Do nothing until the task is completed
            while (d.IsCompleted == false) ;
            d.Dispose();
        }
    }
}