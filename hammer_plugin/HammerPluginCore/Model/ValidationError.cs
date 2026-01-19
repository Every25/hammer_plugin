namespace HammerPluginCore.Model
{
    /// <summary>
    /// Описание одной ошибки валидации конкретного параметра.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Хранит текст ошибки.
        /// </summary>
        private string _message;

        /// <summary>
        /// Хранит тип некорректного параметра.
        /// </summary>
        private ParameterType _errorParameter;

        /// <summary>
        /// Создаёт новый объект ошибки валидации параметра.
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке.</param>
        /// <param name="parameter">Идентификатор параметра,
        /// к которому относится ошибка.</param>
        public ValidationError(ParameterType parameter, string message)
        {
            _errorParameter = parameter;
            _message = message;
        }

        /// <summary>
        /// Параметр, для которого произошла ошибка.
        /// </summary>
        public ParameterType ParameterType => _errorParameter;

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Message => _message;
    }
}