using System;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office.CustomUI;
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
            Regex pattern = CreateFilterPattern(filters),
                  pattern_compet = CreateFilterPattern(compets.Select(x => $"({x.Name})").ToArray());

            List<Question> _questions = new List<Question>();

            Table? question_table = FindTableByTitle(filters);
            var par_1 = question_table.PreviousSibling<Paragraph>();
            var par_2 = par_1?.PreviousSibling<Paragraph>();

            while (question_table is not null) 
            {
                var competetion_match = pattern_compet.Match(GetTitleRow(question_table).InnerText);
                
                if (!competetion_match.Success)
                    break;

                if (pattern.Matches($"{par_1.InnerText} {par_2.InnerText}").Count == filters.Length)
                {
                    var questions_cells = question_table.Elements<TableRow>().Skip(1).SelectMany(n => n.Elements<TableCell>());

                    foreach (var item in questions_cells)
                    {
                        int question_num = 1;
                        foreach (var item1 in item.Elements<Paragraph>())
                        {
                            string question_description = item1.InnerText;

                            if (question_description.Length > 0)
                            {
                                Question question = new Question(question_num, competetion_match.Value, question_description);
                                _questions.Add(question);

                                question_num++;
                            }
                        }

                    }

                }
                else
                    break;

                question_table = question_table.NextSibling<Table>();
                FindPrevTitle(in question_table, out par_1, out par_2);
                /*question_table = question_table.NextSibling<Table>();

                par_2 = question_table.PreviousSibling<Paragraph>();
                par_1 = par_2.PreviousSibling<Paragraph>();

                while (par_1.InnerText == "" || par_2.InnerText == "")
                {
                    par_2 = par_2.PreviousSibling<Paragraph>();
                    par_1 = par_2.PreviousSibling<Paragraph>();
                }*/
            }

            return _questions;

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
            Regex regex = CreateFilterPattern(filters);

            var bles = main_part?.Document?.Body?.Elements<Paragraph>().ElementAt(0);
            var now_par = bles?.NextSibling<Paragraph>();

            while (now_par is not null)
            {
                string title = "";
                now_par = now_par?.NextSibling<Paragraph>();

                if (now_par is not null)
                {
                    var next_paragraph = now_par?.NextSibling<Paragraph>();

                    title = now_par?.InnerText.Trim() + " " + next_paragraph?.InnerText.Trim();
                    var matches = regex.Matches(title);

                    if (matches.Count == filters.Length)
                        return now_par?.NextSibling<Table>();
                }
            }

            return null;
        }

        private bool FindTableByColumn(string column)
        {
            IEnumerable<Paragraph> paragraphs;
            return false;
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
