namespace DocsParserLib
{
    public interface IAssessmentItem
    {
        /// <summary>
        /// Номер элемента (от 0)
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; }
    }

    public interface ICompetencinable
    {
        /// <summary>
        /// Компетенция, которой привязан экземпляр
        /// </summary>
        public Competention Competention { get; set; }
    }


    /// <summary>
    /// Структура, представляющая вопрос.
    /// </summary>
    public struct Question : IAssessmentItem, ICompetencinable
    {
        public int Number { get; set; }
        public string Description { get; set; }
        public Competention Competention { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Rectangle"/>.
        /// </summary>
        /// 
        /// <param name="_num">Номер вопроса (от 0)</param>
        /// <param name="_comp_name">Компетенция, к которой вопрос относиться</param>
        /// <param name="_descr">Описание вопроса</param>
        public Question(int _num, Competention _comp_name, string _descr)
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

    /// <summary>
    /// Структура, представляющий вариант ответа в практическом задании
    /// </summary>
    public struct AnswerVariant : IAssessmentItem
    {
        /// <inheritdoc/>
        public int Number { get; set; }
        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Указывает, является ли ответ правильным.
        /// </summary>
        public bool ValidAnswer { get; set; }

        /// <summary>
        /// Буква ответа. (Например, если ответ под номером 0, то его буква - А
        /// </summary>
        public char AnswerLetter { 
            get
            {
                return Letters[Number];
            } 
        }

        /// <summary>
        /// Удобочитаемый номер задания для человека. (Например, если ответ под номером 0, то его свойство вернёт 1)
        /// </summary>
        public int AnswerNormalNumber
        {
            get
            {
                return Number + 1;
            }
        }

        private string Letters = "АБВГДЕЁЖЗИЙКЛМНОПРСТ";

        /// <summary>
        /// Инициализирует экземпляр структуры <see cref="AnswerVariant">
        /// </summary>
        /// <param name="_num">Номер варианта ответа (от 0)</param>
        /// <param name="_desc">Описание задания</param>
        /// <param name="valid">Флаг, обозначающий является ли данный вариант ответа правильным</param>
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

    /// <summary>
    /// Структура, обозначающая практическое задание
    /// </summary>
    public struct PracticTask : IAssessmentItem, ICompetencinable
    {
        /// <inheritdoc/>
        public int Number { get; set; }
        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Указывает на компетенцию, к которой привязано практическое задание
        /// </summary>
        public Competention Competention { get; set; }
        
        /// <summary>
        /// Указывает на список вариантов ответа
        /// </summary>
        public List<AnswerVariant> answerVariants { get; private set; }

        /// <summary>
        /// Инициализирует экземпляр структуры <see cref="PracticTask"/>. 
        /// </summary>
        /// <param name="_numb">Номер задания (от 0)</param>
        /// <param name="_competetion">Компетенция, к которой относится задание. <seealso cref="Competention"/></param>
        /// <param name="_descr">Описание задания</param>
        /// <param name="answers">Список вариантов ответа. <seealso cref="AnswerVariant"></seealso></param>
        public PracticTask(int _numb, Competention _competetion, string _descr, List<AnswerVariant> answers)
        {
            Number = _numb;
            Competention = _competetion;
            Description = _descr;
            answerVariants = answers;
        }

        /// <summary>
        /// Инициализирует экземпляр структуры <see cref="PracticTask"/>. 
        /// </summary>
        /// <param name="_numb">Номер задания (от 0)</param>
        /// <param name="_competetion">Компетенция, к которой относится задание. <seealso cref="Competention"/></param>
        /// <param name="_descr">Описание задания</param>
        /// <param name="answers">Список вариантов ответа. <seealso cref="AnswerVariant"></seealso></param>
        public PracticTask(int _numb, Competention _competetion, string _descr) : this(_numb, _competetion, _descr, new List<AnswerVariant>()) { }

        /// <summary>
        /// Получение правильного ответа на задание
        /// </summary>
        /// <returns>
        /// Экземпляр класса <see cref="AnswerVariant"/>, который является правильным ответом на задание (В котором флаг ValidAnswer равен true)
        /// </returns>
        public AnswerVariant GetValidVariant()
        {
            return answerVariants.First(n => n.ValidAnswer);
        }

        public override string ToString()
        {
            string answers = "";
            foreach (var variant in answerVariants)
                answers += variant.ToString() + "\n\n";
                
            return $"{Number} {Competention.Name} {Description}\n{answers}";
        }
    }
}
