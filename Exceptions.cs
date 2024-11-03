namespace DocsParserLib
{
    public class MainPartNotFound : Exception
    {
        public MainPartNotFound(string message) : base(message) { }
        public MainPartNotFound() : base("Основная часть файла не найдена") { }
    }

    public class DataNotLoaded : Exception
    {
        public DataNotLoaded(string message) : base(message) { }
        public DataNotLoaded() : base("Часть информации не была собрана") { }
    }
}
