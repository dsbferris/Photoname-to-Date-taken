using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Fotos7000
{
    public partial class Form1 : Form
    {
        private static string[] exts = new string[] { "jpg", "gif", "png"};

        //we init this once so that if the function is repeatedly called
        //it isn't stressing the garbage man
        private static Regex r = new Regex(":");

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var filepaths = GetFiles(textBox1.Text);
            if(filepaths != null)
            {
                List<DateFile> dateFiles = GetDateFiles(filepaths);
                foreach(var df in dateFiles)
                {
                    string datename = GetDateNamePath(df.FilePath, df.DateTaken);
                    if (File.Exists(datename)) datename = NextAvailableFilename(datename);
                    File.Move(df.FilePath, datename);

                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath == null) return;
            textBox1.Text = fbd.SelectedPath;
        }

        private static string GetDateNamePath(string filepath, DateTime dateTime)
        {
            string s = dateTime.Year.ToString();
            s += (dateTime.Month < 10) ? ("0" + dateTime.Month) : (dateTime.Month.ToString());
            s += (dateTime.Day < 10) ? ("0" + dateTime.Day) : (dateTime.Day.ToString());
            s += "_";
            s += (dateTime.Hour < 10) ? ("0" + dateTime.Hour) : (dateTime.Hour.ToString());
            s += (dateTime.Minute < 10) ? ("0" + dateTime.Minute) : (dateTime.Minute.ToString());
            s += (dateTime.Second < 10) ? ("0" + dateTime.Second) : (dateTime.Second.ToString());

            FileInfo fi = new FileInfo(filepath);
            return fi.FullName.Replace(fi.Name, s) + fi.Extension;
        }

        private static List<DateFile> GetDateFiles(IEnumerable<string> files)
        {
            List<DateFile> datefiles = new List<DateFile>();
            foreach (var f in files)
            {
                foreach (var ex in exts)
                {
                    if (f.EndsWith(ex, true, CultureInfo.CurrentCulture))
                    {
                        datefiles.Add(new DateFile(f, GetDateTakenFromImage(f)));
                        break;
                    }
                }
            }
            return datefiles;
        }

        private static IEnumerable<string> GetFiles(string path)
        {
            if (path != null)
            {
                try
                {
                    return Directory.EnumerateFiles(path);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                return null;
            }
        }

        private static DateTime GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try
                {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    return DateTime.Parse(dateTaken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(path + Environment.NewLine + ex.ToString());
                    return DateTime.Now;
                }
                
            }
        }

        private static string numberPattern = "_{0}";

        public static string NextAvailableFilename(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }
    }
}
