using System;

namespace HammerPluginCore.Model
{
    /// <summary>
    /// Представляет параметр молотка с ограничениями
    /// по минимальному и максимальному значению.
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
            //TODO: validation +
            set
            {
                if (value < _minValue || value > _maxValue)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Значение {value} вне допустимого" +
                        $" диапазона [{_minValue}, {_maxValue}]");
                }
                _value = value;
            }
        }

        /// <summary>
        /// Получает или задает минимальное допустимое значение параметра.
        /// </summary>
        public double MinValue
        {
            get => _minValue;
            //TODO: validation +
            set
            {
                if (value > _maxValue)
                {
                    throw new ArgumentException(
                        $"Минимальное значение ({value}) не может быть больше " +
                        $"максимального значения ({_maxValue}).",
                        nameof(value));
                }

                if (_value < value)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Текущее значение ({_value}) меньше " +
                        $"нового минимального значения ({value}). " +
                        $"Сначала установите значение в допустимый диапазон.");
                }

                _minValue = value;
            }
        }

        /// <summary>
        /// Получает или задает максимальное допустимое значение параметра.
        /// </summary>
        public double MaxValue
        {
            get => _maxValue;
            //TODO: validation +
            set
            {
                if (value < _minValue)
                {
                    throw new ArgumentException(
                        $"Максимальное значение ({value}) не может быть меньше " +
                        $"минимального значения ({_minValue}).",
                        nameof(value));
                }

                if (_value > value)
                {
                    throw new InvalidOperationException(
                        $"Текущее значение ({_value}) больше " +
                        $"нового максимального значения ({value}). " +
                        $"Сначала установите значение в допустимый диапазон.");
                }

                _maxValue = value;
            }
        }
    }
}
