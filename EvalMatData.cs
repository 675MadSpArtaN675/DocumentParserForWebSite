namespace DocsParserLib
{
    /// <summary>
    /// Структура, представляющая показатель оценивания
    /// </summary>
    public struct EvalulationMaterial
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string EM_Type { get; set; }
        public string EvalulationIndicator { get; set; }

        /// <summary>
        /// Инициализация экземпляра структуры <see cref="EvalulationMaterial"/>
        /// </summary>
        /// 
        /// <param name="name">Название показателя (Например, знать, уметь, владеть, и т.д.</param>
        /// <param name="description">Описание показателя</param>
        /// <param name="em_type">Тип показателя оценивания</param>
        /// <param name="evalulation_indicator">Показатель оценивания (например, Наличие умений, полнота знаний, и т.д.)</param>
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

    /// <summary>
    /// Структура, представляющая компетенцию
    /// </summary>
    public struct Competention
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public List<EvalulationMaterial> EvalulationMaterial { get; private set; }

        /// <summary>
        /// Инициализация экземпляра структуры <see cref="Competention"/>
        /// </summary>
        /// <param name="_number">Порядковый номер компетенции</param>
        /// <param name="_name">Название компетенции (Например, ПК-2)</param>
        /// <param name="e_mat">Список оценочных критериев. Представляет List экземпляров структуры <see cref="EvalulationMaterial"/></param>
        public Competention(int _number, string _name, List<EvalulationMaterial> e_mat)
        {
            Number = _number;
            Name = _name;
            EvalulationMaterial = e_mat;
        }

        /// <summary>
        /// Инициализация экземпляра структуры <see cref="Competention"/>
        /// </summary>
        /// <param name="_number">Порядковый номер компетенции</param>
        /// <param name="_name">Название компетенции (Например, ПК-2)</param>
        public Competention(int _number, string _name) : this(_number, _name, new List<EvalulationMaterial>()) { }

        /// <summary>
        /// Инициализация экземпляра структуры <see cref="Competention"/>
        /// </summary>
        public Competention() : this(1, "", new List<EvalulationMaterial>()) { }

        public override string ToString()
        {
            string list_em = String.Empty;

            foreach (EvalulationMaterial item in EvalulationMaterial)
                list_em += $"{item.ToString()}\n";
            
            return $"Номер: {Number}\nКомпетенция: {Name}\nСписок оценочных критериев:\n\n{list_em}\n";
        }
    }  
}
