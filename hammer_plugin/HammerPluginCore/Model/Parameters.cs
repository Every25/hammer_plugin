using System;

namespace HammerPluginCore.Model
{
    /// <summary>
    /// Управляет параметрами и их валидацией.
    /// </summary>
    public class Parameters
    {

        /// <summary>
        /// Коллекция параметров молотка, индексируемая по типу параметра.
        /// </summary>
        private readonly Dictionary<ParameterType, Parameter> _parameters;

        /// <summary>
        /// Список ошибок, возникших при валидации.
        /// </summary>
        public List<ValidationError> _errorCollector;

        /// <summary>
        /// Возвращает список ошибок.
        /// </summary>
        public List<ValidationError> ErrorCollector 
        { 
            get { return _errorCollector; } 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса Parameters с параметрами по умолчанию.
        /// </summary>
        public Parameters()
        {
            _errorCollector = new List<ValidationError>();
            _parameters = new Dictionary<ParameterType, Parameter>
            {
                { ParameterType.HeightH, new Parameter(150, 600, 540) },
                { ParameterType.LengthL, new Parameter(100, 1000, 360) },
                { ParameterType.FaceDiameterD, new Parameter(20, 100, 85) },
                { ParameterType.FaceWidthC, new Parameter(10, 100, 40) },
                { ParameterType.NeckWidthA, new Parameter(10, 100, 40) },
                { ParameterType.NeckDiameterB, new Parameter(10, 95, 70) },
                { ParameterType.HeadHoleX1, new Parameter(5, 100, 32) },
                { ParameterType.HeadHoleY1, new Parameter(5, 80, 52) },
                { ParameterType.HandleWidthX2, new Parameter(5, 100, 32) },
                { ParameterType.HandleWidthY2, new Parameter(5, 80, 52) },
                { ParameterType.ClawLengthL, new Parameter(50, 250, 200) },
                { ParameterType.ClawWidthW, new Parameter(10, 100, 60) }
            };

            UpdateCalculatedParameters();
        }

        /// <summary>
        /// Возвращает значение параметра по названию
        /// </summary>
        /// <param name="type">тип параметра</param>
        /// <returns></returns>
        public double GetParameter(ParameterType type)
        {
            return _parameters[type].Value;
        }

        /// <summary>
        /// Устанавливает значение параметра с обновлением зависимых параметров
        /// </summary>
        /// <param name="type">Тип параметра</param>
        /// <param name="value">Новое значение</param>
        public void SetParameter(ParameterType type, double value)
        {
            var parameter = _parameters[type];
            double oldValue = parameter.Value;
            parameter.Value = value;
            UpdateCalculatedParameters();
            Validate();
        }

        /// <summary>
        /// Полная проверка параметров.
        /// </summary>
        public void Validate()
        {
            _errorCollector.Clear();
            double neckDiameter = _parameters[ParameterType.NeckDiameterB].Value;
            double faceDiameter = _parameters[ParameterType.FaceDiameterD].Value;
            double clawWidth = _parameters[ParameterType.ClawWidthW].Value;
            double headHoleX1 = _parameters[ParameterType.HeadHoleX1].Value;
            double headHoleY1 = _parameters[ParameterType.HeadHoleY1].Value;
            double middle = _parameters[ParameterType.LengthL].Value - (_parameters[ParameterType.ClawLengthL].Value 
                + _parameters[ParameterType.NeckWidthA].Value + (_parameters[ParameterType.FaceWidthC].Value / 2));

            // Проверка диапазонов.
            foreach (var kvp in _parameters)
            {
                var parameterType = kvp.Key;
                var parameter = kvp.Value;

                if (parameter.Value < parameter.MinValue || parameter.Value > parameter.MaxValue)
                {
                    _errorCollector.Add(new ValidationError(parameterType,
                        $"Значение параметра {parameterType}" +
                        $" выходит за допустимые пределы [{parameter.MinValue}, {parameter.MaxValue}]."));
                }
            }

            // Проверка взаимосвязанных параметров.
            if (neckDiameter >= faceDiameter)
            {
                _errorCollector.Add(new ValidationError(ParameterType.NeckDiameterB,
            $"Диаметр выступа b должен быть меньше диаметра бойка D."));
                _errorCollector.Add(new ValidationError(ParameterType.FaceDiameterD,
           $"Диаметр выступа b должен быть меньше диаметра бойка D."));
            }

            if (headHoleX1 >= neckDiameter)
            {
                _errorCollector.Add(new ValidationError(ParameterType.HeadHoleX1,
            $"Ширина отверстия x1 должна быть меньше ширины выступа b."));
                _errorCollector.Add(new ValidationError(ParameterType.NeckDiameterB,
          $"Ширина отверстия x1 должна быть меньше ширины выступа b."));
            }

            if (headHoleY1 >= middle)
            {
                _errorCollector.Add(new ValidationError(ParameterType.HeadHoleY1,
            $"Ширина отверстия y1 должна быть меньше длины средней части."));
            }

            if (clawWidth > faceDiameter)
            {
                _errorCollector.Add(new ValidationError(ParameterType.ClawWidthW,
            $"Ширина носка w должна быть не больше диаметра бойка D."));
                _errorCollector.Add(new ValidationError(ParameterType.FaceDiameterD,
           $"Ширина носка w должна быть не больше диаметра бойка D."));
            }
        }

        /// <summary>
        /// Очищает список ошибок.
        /// </summary>
        public void ClearErrors()
        {
            _errorCollector.Clear();
        }

        /// <summary>
        /// Обновляет вычисляемые параметры на основе основных.
        /// </summary>
        public void UpdateCalculatedParameters()
        {
            double heightH = _parameters[ParameterType.HeightH].Value;
            double lengthL = heightH * 2.0 / 3.0;
            _parameters[ParameterType.LengthL].Value = Math.Round(lengthL, 2);

            double headHoleX1 = _parameters[ParameterType.HeadHoleX1].Value;
            _parameters[ParameterType.HandleWidthX2].Value = headHoleX1;

            double headHoleY1 = _parameters[ParameterType.HeadHoleY1].Value;
            _parameters[ParameterType.HandleWidthY2].Value = headHoleY1;
        }
    }
}

