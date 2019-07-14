using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerrisPDT
{
    public class DateFile
    {
        public string FilePath { get; set; }
        public DateTime DateTaken { get; set; }

        public DateFile(string filepath, DateTime datetaken)
        {
            FilePath = filepath;
            DateTaken = datetaken;
        }
    }
}
