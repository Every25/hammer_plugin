using System;

namespace HammerPluginCore.Model
{
    /// <summary>
    /// Представляет параметр молотка с ограничениями по минимальному и максимальному значению.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Текущее значение параметра.
        /// </summary>
        private double _value;

        /// <summary>
        /// Минимальное допустимое значение параметра.
        /// </summary>
        private double _minValue;

        /// <summary>
        /// Максимальное допустимое значение параметра.
        /// </summary>
        private double _maxValue;

        /// <summary>
        /// Инициализирует новый экземпляр класса BrickParameter.
        /// </summary>
        /// <param name="minValue">Минимальное значение.</param>
        /// <param name="maxValue">Максимальное значение.</param>
        /// <param name="defaultValue">Значение по умолчанию.</param>
        public Parameter(
            double minValue,
            double maxValue,
            double defaultValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _value = defaultValue;
        }

        /// <summary>
        /// Получает или задает текущее значение параметра.
        /// </summary>
        public double Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Получает или задает минимальное допустимое значение параметра.
        /// </summary>
        public double MinValue
        {
            get => _minValue;
            set => _minValue = value;
        }

        /// <summary>
        /// Получает или задает максимальное допустимое значение параметра.
        /// </summary>
        public double MaxValue
        {
            get => _maxValue;
            set => _maxValue = value;
        }
    }
}
