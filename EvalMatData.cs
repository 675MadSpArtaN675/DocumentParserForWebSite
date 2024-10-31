namespace DocsParserLib
{
    public struct EvalulationMaterial
    {
        public string Name;
        public string Description;
        public string EM_Type;
        public string EvalulationIndicator;

        public EvalulationMaterial(string name, string description, string em_type, string evalulation_indicator)
        {
            Name = name;
            Description = description;
            EM_Type = em_type;
            EvalulationIndicator = evalulation_indicator;
        }

        public override string ToString()
        {
            return $"Имя: {Name}\nОписание: {Description}\nТип оценочного материала: {EM_Type}\nПоказатели оценивания: {EvalulationIndicator}\n";
        }
    }

    public struct Competetion
    {
        public int Number;
        public string Name;
        public List<EvalulationMaterial> EvalulationMaterial;

        public Competetion(int _number, string _name, List<EvalulationMaterial> e_mat)
        {
            Number = _number;
            Name = _name;
            EvalulationMaterial = e_mat;
        }

        public Competetion(int _number, string _name) : this(_number, _name, new List<EvalulationMaterial>()) { }

        public Competetion() : this(1, "", new List<EvalulationMaterial>()) { }

        public override string ToString()
        {
            string list_em = String.Empty;

            foreach (EvalulationMaterial item in EvalulationMaterial)
                list_em += $"{item.ToString()}\n";
            
            return $"Номер: {Number}\nКомпетенция: {Name}\nСписок оценочных критериев:\n\n{list_em}\n";
        }
    }  
}
