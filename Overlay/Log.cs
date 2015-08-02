using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlay {
    public static class Log {
        static string _path = @"C:\FFXIV";
        static string _file = "log.txt";


        static void EnsureFileExists() {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            if (!File.Exists(string.Format(@"{0}\{1}", _path, _file)))
                File.Create(string.Format(@"{0}\{1}", _path, _file));
        }

        public static void Write(string format, params object[] merge) {
            EnsureFileExists();

            using (StreamWriter sw = File.AppendText(string.Format(@"{0}\{1}", _path, _file))) {
                sw.WriteLine(string.Format(format, merge));
            }
        }


    }
}
