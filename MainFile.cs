using System;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocsParserLib
{
    public class DocParser
    {
        private WordprocessingDocument _wordDoc;
        private MainDocumentPart? main_part;

        private IEnumerable<Table> tables;
        private List<Competetion> compets;

        public MainDocumentPart? MainPartOfDoc { 
            get {
                return main_part;
            }
        }

        public DocParser(string filename)
        {
            _wordDoc = WordprocessingDocument.Open(filename, false);
            main_part = _wordDoc.MainDocumentPart;

            if (main_part is null || main_part.Document.Body is null)
                throw new MainPartNotFound();

            tables = main_part.Document.Body.Elements<Table>();
            compets = new List<Competetion>();
        }

        ~DocParser()
        {
            _wordDoc.Dispose();
        }

        public void Parse()
        {
            
        }

        public List<Competetion> GetCompetetions()
        {
            if (compets.Count > 0)
                return compets;

            string[] filters_lines = { "ПЕРЕЧЕНЬ", "ПЛАНИРУЕМЫХ", "РЕЗУЛЬТАТОВ"};
            Table table = FindTableByTitle(filters_lines);

            IEnumerable<TableRow> competetion_table_rows = table.Elements<TableRow>();

            int rows_count = competetion_table_rows.Count();
            IEnumerable<TableRow> rowsWithoutTitle = competetion_table_rows.Skip(1);

            for (int i = 1; i < rows_count / 2 + 1; i++)
            {
                var row = rowsWithoutTitle.Take(3);
                rowsWithoutTitle = rowsWithoutTitle.Except(row);

                Competetion? competetion = CreateCompetetion(row);

                if (competetion is not null)
                    compets.Add((Competetion)competetion);
            }

            return compets;
        }

        public List<Question> GetQuestions()
        {
            string[] filters = { "результатов", "компетенций", "вопросы" };

            return ReadTable<Question>(filters, (competetion_match, question_table, _questions) =>
            {
                IEnumerable<TableCell> questions_cells = question_table.Elements<TableRow>().Skip(1).SelectMany(n => n.Elements<TableCell>());

                ReadQuestions(_questions, questions_cells, competetion_match);
            });

        }

        // Action<
        // Table? - откуда считываем строки
        // List<Question> - куда сохранять результат
        // Match? - результат сравнения шаблона заголовка нужной таблицы
        // (Для какой компетенции предназначено)
        // >

        // Предупреждение: Данная функция сделана для чтения вопросов и практических заданий
        
        public List<PracticTask> GetPracticTasks()
        {
            string[] filters = ["Практические", "задания", "результатов"];

            var result_tasks = ReadTable<PracticTask>(filters, 
                (competetion_match, question_table, tasks) => {
                    List<TableRow> tasks_rows = question_table.Elements<TableRow>().Skip(1).ToList();

                    foreach (TableRow row in tasks_rows)
                    {
                        Console.WriteLine(row.InnerText);
                        tasks.Add(new PracticTask(0, 0, row.InnerText));
                    }
            });




            return result_tasks;
        }

        private List<T> ReadTable<T>(string[] filters, Action<Match?, Table?, List<T>> read_rows)
        {
            Regex pattern = CreateFilterPattern(filters),
                  pattern_compet = CreateFilterPattern(compets.Select(x => $"{x.Name}").ToArray());

            List<T> result = new List<T>();

            Table? question_table = FindTableByTitle(filters);
            var par_1 = question_table.PreviousSibling<Paragraph>();
            var par_2 = par_1?.PreviousSibling<Paragraph>();

            while (question_table is not null)
            {
                Match? competetion_match = pattern_compet.Match(GetTitleRow(question_table).InnerText);

                if (!competetion_match.Success)
                    break;

                if (pattern.Matches($"{par_1.InnerText} {par_2.InnerText}").Count == filters.Length)
                {
                    //! List<T> question_list, Match pattern, string filters
                    read_rows(competetion_match, question_table, result);
                }
                else
                    break;

                question_table = question_table.NextSibling<Table>();
                FindPrevTitle(in question_table, out par_1, out par_2);
            }

            return result;
        }

        private Competetion? CreateCompetetion(IEnumerable<TableRow> row)
        {
            Competetion result = new Competetion();
            EvalulationMaterial eval_mat = new EvalulationMaterial();

            IEnumerable<TableCell[]> row_elements = row.Select((elem) => elem.Elements<TableCell>().ToArray());

            int iter = 0;
            foreach (TableCell[] item in row_elements)
            {
                foreach (TableCell cell in item)
                {
                    string text = cell.Elements<Paragraph>().Select(n => n.InnerText).Aggregate((x, y) => $"{x}\n{y}");

                    if (text != "")
                    {
                        switch (iter)
                        {
                            case 0:
                                result.Number = int.Parse(text.Trim());
                                break;

                            case 1:
                                result.Name = text.Trim();
                                break;
                            case 2:
                                string[] sp_text = text.Split('\n');

                                eval_mat.Name = sp_text[0].Trim();
                                eval_mat.Description = sp_text[1].Trim();
                                break;
                            case 3:
                                eval_mat.EM_Type = text.Trim();
                                break;
                            default:
                                eval_mat.EvalulationIndicator = text.Trim();
                                result.EvalulationMaterial.Add(eval_mat);
                                break;
                        }

                        iter++;
                    }

                }

                iter = 2;
            }

            if (result.Name == "")
                return null;

            return result;
        }

        private void FindPrevTitle(in Table? question_table, out Paragraph? par_1, out Paragraph? par_2)
        {
            par_2 = question_table.PreviousSibling<Paragraph>();
            par_1 = par_2.PreviousSibling<Paragraph>();

            while (par_1.InnerText == "" || par_2.InnerText == "")
            {
                par_2 = par_2.PreviousSibling<Paragraph>();
                par_1 = par_2.PreviousSibling<Paragraph>();
            }
        }

        public Table? FindTableByTitle(string[] filters)
        {
            if (main_part.Document.Body is null) return null;

            var paragraphs = main_part.Document.Body.Elements<Paragraph>().ToArray();
            Regex title_pattern = CreateFilterPattern(filters);

            int i = 0;
            foreach (var paragraph in paragraphs)
            {
                string par_text = UnionRuns(paragraph);

                if (title_pattern.Matches(par_text).Count() == filters.Length)
                    return paragraph.NextSibling<Table>();

                Paragraph? next_par = paragraph.NextSibling<Paragraph>();
                string par_text_2 = UnionRuns(next_par);

                par_text += " " + par_text_2;

                if (title_pattern.Matches(par_text).Count() == filters.Length)
                    return next_par?.NextSibling<Table>();
            }

            return null;
        }

        private void ReadQuestions(List<Question> questions_list, IEnumerable<TableCell> questions_cells, Match? competetion_match)
        {
            foreach (var item in questions_cells)
            {
                int question_num = 1;
                foreach (var item1 in item.Elements<Paragraph>())
                {
                    string question_description = item1.InnerText;

                    if (question_description.Length > 0)
                    {
                        Question question = new Question(question_num, competetion_match.Value, question_description);
                        questions_list.Add(question);

                        question_num++;
                    }
                }

            }
        }

        private string UnionRuns(Paragraph? paragraph)
        {
            if (paragraph == null) return "";

            var runs = paragraph.Elements<Run>();
            var run_texts = runs.Select(n => n.InnerText);

            string par_text = "";
            if (run_texts.Count() > 0)
                par_text = run_texts.Aggregate((x, y) => $"{x} {y}");

            return par_text;
        }

        private TableRow GetTitleRow(Table table)
        {
            return table.Elements<TableRow>().ElementAt(0);
        }

        public Regex CreateFilterPattern(string[] filters)
        {
            string pat = string.Join('|', filters.Select(n => $"({n})"));
            return new Regex(pat, RegexOptions.IgnoreCase);
        }
    }
}
