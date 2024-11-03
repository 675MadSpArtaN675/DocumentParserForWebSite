using DocumentFormat.OpenXml.Office2013.Excel;
using System;

namespace DocsParserLib
{
    public struct Question
    {
        public int Number;
        public Competetion Competention;
        public string Description;

        public Question(int _num, Competetion _comp_name, string _descr)
        { 
            Number = _num;
            Competention = _comp_name;
            Description = _descr;
        }

        public override string ToString()
        {
            return $"Номер: {Number}\nНазвание компетенции: {Competention.Name}\nОписание вопроса: {Description}\n";
        }
    }

    public struct AnswerVariant
    {
        public int Number;
        public string Description;
        public bool ValidAnswer;

        public char AnswerLetter { 
            get
            {
                return Letters[Number];
            } 
        }

        public int AnswerNormalNumber
        {
            get
            {
                return Number + 1;
            }
        }

        private string Letters = "АБВГДЕЁЖЗИЙКЛМНОПРСТ";

        public AnswerVariant(int _num, string _desc, bool valid)
        {
            Number = _num;
            Description = _desc;
            ValidAnswer = valid;
        }

        public override string ToString()
        {
            return $"[{Number}/{AnswerNormalNumber}/{AnswerLetter}]:\nDescription: {Description}\nValid: {ValidAnswer}";
        }
    }

    public struct PracticTask
    {
        public int Number;
        public Competetion Competetion;
        public string Description;

        public List<AnswerVariant> answerVariants { get; private set; }

        public PracticTask(int _numb, Competetion _competetion, string _descr, List<AnswerVariant> answers)
        {
            Number = _numb;
            Competetion = _competetion;
            Description = _descr;
            answerVariants = answers;
        }

        public PracticTask(int _numb, Competetion _competetion, string _descr) : this(_numb, _competetion, _descr, new List<AnswerVariant>()) { }

        public override string ToString()
        {
            string answers = "";
            foreach (var variant in answerVariants)
                answers += variant.ToString() + "\n\n";
                
            return $"{Number} {Competetion.Name} {Description}\n{answers}";
        }
    }
}
