namespace DocsParserLib
{
    public enum EvalMaterialType : byte
    {
        Question, // Вопросы к экзамену
        StandartTask, // Стандартные задания
        AppliedTask // Прикладные задания
    }

    public enum EvalulationIndicators : byte
    {
        FullKnowledge, // Полнота знаний
        HasSkills, // Наличие умений
        HasAbilities, // Наличие навыков
    }

    public struct ParsedData
    {
        public List<Competention> compets;
        public List<Question> question;
        public List<PracticTask> practicTasks;
    }
}
