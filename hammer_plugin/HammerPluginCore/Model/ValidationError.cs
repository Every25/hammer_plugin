using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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
        string _message;

        /// <summary>
        /// Хранит тип некорректного параметра.
        /// </summary>
        ParameterType _errorParameter;

        /// <summary>
        /// Создаёт новый объект ошибки валидации параметра.
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке.</param>
        /// <param name="parameter">Идентификатор параметра, к которому относится ошибка.</param>
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
