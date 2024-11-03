using System;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
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

            return ReadTable<Question>(filters, (question_table, _questions) =>
            {
                IEnumerable<TableRow> table_rows = question_table.Elements<TableRow>();
                IEnumerable<TableCell> questions_cells = table_rows.Skip(1).SelectMany(n => n.Elements<TableCell>());

                string title = table_rows.ElementAt(0).InnerText.Trim();
                string? title_match = GetCompetitionNameStr(title);
                Competetion? comp = GetCompetetionByName(title_match);

                ReadQuestions(_questions, questions_cells, comp ?? new Competetion());
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
                (question_table, tasks) => {
                    List<TableRow> tasks_rows = question_table.Elements<TableRow>().ToList();

                    int question_num = 1;
                    Competetion? comp = null;
                    foreach (TableRow row in tasks_rows)
                    {
                        string? title = GetCompetitionNameStr(row.InnerText);

                        if (title is not null)
                        {
                            comp = GetCompetetionByName(title);
                            question_num = 1;
                            continue;
                        }

                        tasks.Add(PracticeTaskRowParse(row, question_num, comp ?? new Competetion()));
                        question_num++;
                    }
            });

            return result_tasks;
        }

        private PracticTask PracticeTaskRowParse(TableRow row, int question_num, Competetion comp)
        {
            IEnumerable<Paragraph> answer = row.ElementAt(1).Elements<Paragraph>();
            
            string title = answer.ElementAt(0).InnerText.Trim();
            List<AnswerVariant> variants = new List<AnswerVariant>();

            int i = 0;
            foreach (var paragraph in answer.Skip(1))
            {
                if (paragraph.InnerText != "")
                {
                    string ans_description = paragraph.InnerText.Trim();
                    bool valid_var = false;

                    if (paragraph.Elements<Run>()?.ElementAt(0)?.RunProperties?.Bold is not null)
                        valid_var = true;

                    variants.Add(new AnswerVariant(i, ans_description, valid_var));
                    i++;
                }
            }

            PracticTask task = new PracticTask(question_num, comp, title, variants);
            return task;
        }

        private string? GetCompetitionNameStr(string title_text)
        {
            Regex comp_pat = new Regex(@"([А-Я]{2}-\d+)\s*");
            Match match = comp_pat.Match(title_text.Trim());

            if (match.Success && match.Value != "")
                return match.Value.Trim();

            return null;
        }

        private Competetion? GetCompetetionByName(string name)
        {
            Console.WriteLine($"A{name}A");
            try
            {
                return compets.First(n => n.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch (ArgumentNullException)
            { 
                return null; 
            }
            
        }

        private List<T> ReadTable<T>(string[] filters, Action<Table?, List<T>> read_rows)
        {
            Regex pattern = CreateFilterPattern(filters);
                  
            List<T> result = new List<T>();

            Table? question_table = FindTableByTitle(filters);
            var par_1 = question_table.PreviousSibling<Paragraph>();
            var par_2 = par_1?.PreviousSibling<Paragraph>();

            while (question_table is not null)
            {
                if (pattern.Matches($"{par_1.InnerText} {par_2.InnerText}").Count == filters.Length)
                {
                    //! List<T> question_list, Match pattern, string filters
                    read_rows(question_table, result);
                }
                else
                    break;

                question_table = question_table.NextSibling<Table>();

                if (question_table is not null)
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

        private Table? FindTableByTitle(string[] filters)
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

        private void ReadQuestions(List<Question> questions_list, IEnumerable<TableCell> questions_cells, Competetion competention)
        {
            foreach (var item in questions_cells)
            {
                int question_num = 1;
                foreach (var item1 in item.Elements<Paragraph>())
                {
                    string question_description = item1.InnerText;

                    if (question_description.Length > 0)
                    {
                        Question question = new Question(question_num, competention, question_description);
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

        private Regex CreateFilterPattern(string[] filters)
        {
            string pat = string.Join('|', filters.Select(n => $"({n})"));
            return new Regex(pat, RegexOptions.IgnoreCase);
        }
    }
}
