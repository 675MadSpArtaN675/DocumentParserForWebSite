using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsParserLib
{
    public class MainPartNotFound : Exception
    {
        public MainPartNotFound(string message) : base(message) { }
        public MainPartNotFound() : base("Основная часть файла не найдена") { }
    }
}
