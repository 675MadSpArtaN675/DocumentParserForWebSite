using DocumentFormat.OpenXml.Office2013.Excel;
using System;

namespace DocsParserLib
{
    public struct Question
    {
        public int Number;
        public int CompetentionNum;
        public string Description;

        public Question(int _num, string _descr)
        { 
            Number = _num;
            Description = _descr;
        }
    }

    public struct AnswerVariant
    {
        public int Number;
        public string Description;

        public AnswerVariant(int _num, string _desc)
        {
            Number = _num;
            Description = _desc;
        }
    }

    public struct ParcticTask
    {
        public int Number;
        public int CompetentionNum;
        public string Description;

        public List<AnswerVariant> answerVariants { get; private set; }

        public ParcticTask(int _numb, int _comp_num, string _descr, List<AnswerVariant> answers)
        {
            Number = _numb;
            CompetentionNum = _comp_num;
            Description = _descr;
            answerVariants = answers;
        }

        public ParcticTask(int _numb, int _comp_num, string _descr) : this(_numb, _comp_num, _descr, new List<AnswerVariant>()) { }
    }
}
