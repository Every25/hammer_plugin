using HammerPluginBuilder;
using HammerPluginCore.Model;
using System.Globalization;
using System.Windows.Forms;

namespace HammerPlugin
{
    /// <summary>
    /// Форма ввода параметров и запуска построения модели.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Построитель 3D-модели, использующий API КОМПАС-3D
        /// для создания геометрической моде ли на основе валидных параметров.
        /// </summary>
        private Builder _builder;

        /// <summary>
        /// Параметры модели.
        /// </summary>
        private Parameters _parameters;

        /// <summary>
        /// Словарь соответствия типов параметров и текстовых полей.
        /// </summary>
        private Dictionary<ParameterType, TextBox> _textBoxDict;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления полей.
        /// </summary>
        private bool _isUpdatingCalculatedFields = false;

        /// <summary>
        /// Конструктор формы.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            _builder = new Builder();
            _parameters = new Parameters();

            _textBoxDict = new Dictionary<ParameterType, TextBox>
            {
                { ParameterType.HeightH, textBoxH },
                { ParameterType.LengthL, textBoxL },
                { ParameterType.MiddleM, textBoxM },
                { ParameterType.FaceDiameterD, textBoxD },
                { ParameterType.FaceWidthC, textBoxC },
                { ParameterType.NeckWidthA, textBoxA },
                { ParameterType.NeckDiameterB, textBoxB },
                { ParameterType.HeadHoleX1 , textBoxX1 },
                { ParameterType.HeadHoleY1 , textBoxY1 },
                { ParameterType.ClawLengthL , textBoxNL },
                { ParameterType.ClawWidthW , textBoxW }
            };

            InitializeDefaultValues();
            AttachEventHandlers();
        }

        /// <summary>
        /// Устанавливает значения по умолчанию в текстовые поля.
        /// </summary>
        private void InitializeDefaultValues()
        {
            textBoxH.Text = _parameters.GetParam(ParameterType.HeightH)
                .ToString(CultureInfo.InvariantCulture);
            textBoxL.Text = _parameters.GetParam(ParameterType.LengthL)
                .ToString(CultureInfo.InvariantCulture);
            textBoxM.Text = _parameters.GetParam(ParameterType.MiddleM)
                .ToString(CultureInfo.InvariantCulture);
            textBoxD.Text = _parameters.GetParam(ParameterType.FaceDiameterD)
                .ToString(CultureInfo.InvariantCulture);
            textBoxC.Text = _parameters.GetParam(ParameterType.FaceWidthC)
                .ToString(CultureInfo.InvariantCulture);
            textBoxA.Text = _parameters.GetParam(ParameterType.NeckWidthA)
                .ToString(CultureInfo.InvariantCulture);
            textBoxB.Text = _parameters.GetParam(ParameterType.NeckDiameterB)
             .ToString(CultureInfo.InvariantCulture);
            textBoxX1.Text = _parameters.GetParam(ParameterType.HeadHoleX1)
               .ToString(CultureInfo.InvariantCulture);
            textBoxY1.Text = _parameters.GetParam(ParameterType.HeadHoleY1)
              .ToString(CultureInfo.InvariantCulture);
            textBoxNL.Text = _parameters.GetParam(ParameterType.ClawLengthL)
              .ToString(CultureInfo.InvariantCulture);
            textBoxW.Text = _parameters.GetParam(ParameterType.ClawWidthW)
              .ToString(CultureInfo.InvariantCulture);

            checkBoxNail.Checked = false;
        }

        /// <summary>
        /// Подключает обработчики событий к элементам управления.
        /// </summary>
        private void AttachEventHandlers()
        {
            textBoxH.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.HeightH, textBoxH);
            textBoxM.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.MiddleM, textBoxM);
            textBoxD.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.FaceDiameterD, textBoxD);
            textBoxC.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.FaceWidthC, textBoxC);
            textBoxA.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.NeckWidthA, textBoxA);
            textBoxB.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.NeckDiameterB, textBoxB);
            textBoxX1.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.HeadHoleX1, textBoxX1);
            textBoxY1.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.HeadHoleY1, textBoxY1);
            textBoxW.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.ClawWidthW, textBoxW);
            textBoxNL.TextChanged += (s, e) =>
               OnDataChanged(ParameterType.ClawLengthL, textBoxNL);
        }

        //TODO: RSDN +
        /// <summary>
        /// Обработчик кнопки "Построить".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void BuildButton_Click(object sender, EventArgs e)
        {
            BuildModel();
        }

        /// <summary>
        /// Обрабатывает изменение значения параметра.
        /// </summary>
        /// <param name="paramType">Тип параметра.</param>
        /// <param name="textBox">Текстовое поле.</param>
        private void OnDataChanged(ParameterType paramType, TextBox textBox)
        {
            richTextBox1.Clear();
            bool hasBasicError = false;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BackColor = Color.LightCoral;
                richTextBox1.AppendText("Поля не должны быть пустыми.\n");
                hasBasicError = true;
            }
            else if (!double.TryParse(textBox.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double value))
            {
                textBox.BackColor = Color.LightCoral;
                richTextBox1.AppendText("Некорректные символы.\n");
                hasBasicError = true;
            }
            else
            {
                _parameters.SetParameter(paramType, value);
                UpdateCalculatedFields();
                if (!hasBasicError)
                {
                    textBox.BackColor = Color.White;
                }
            }
            if (hasBasicError)
            {
                richTextBox1.BackColor = Color.LightCoral;
                buttonBuild.Enabled = false;
                foreach (var kvp in _textBoxDict)
                {
                    if (kvp.Value != textBox)
                    {
                        kvp.Value.BackColor = Color.White;
                    }
                }
            }
            else
            {
                _parameters.Validate();
                ShowErrors();
                UpdateFieldColors();
            }
        }

        /// <summary>
        /// Обновляет цвет фона текстовых полей.
        /// </summary>
        private void UpdateFieldColors()
        {
            List<ValidationError> errors = _parameters.ErrorCollector;
            if (errors.Any())
            {
                foreach (var kvp in _textBoxDict)
                {
                    var paramType = kvp.Key;
                    var textBox = kvp.Value;
                    bool hasError = _parameters.ErrorCollector
                        .Any(e => e.ParameterType == paramType);

                    textBox.BackColor = 
                        hasError ? Color.LightCoral : Color.White;
                }
            }
            else
            {
                foreach (var textBox in _textBoxDict.Values)
                {
                    textBox.BackColor = Color.White;
                }
            }
        }

        /// <summary>
        /// Выводит описание ошибок в строку состояния.
        /// </summary>
        private void ShowErrors()
        {
            List<ValidationError> errors = _parameters.ErrorCollector;

            if (errors.Any())
            {
                richTextBox1.BackColor = Color.LightCoral;
                buttonBuild.Enabled = false;
                var uniqueMessages = new HashSet<string>();

                foreach (var error in errors)
                {
                    uniqueMessages.Add(error.Message);
                }

                foreach (var message in uniqueMessages)
                {
                    richTextBox1.AppendText($"{message}\n");
                }
            }
            else
            {
                richTextBox1.BackColor = Color.White;
                buttonBuild.Enabled = true;
            }
        }

        /// <summary>
        /// Обновляет вычисляемые поля на основе текущих параметров.
        /// </summary>
        private void UpdateCalculatedFields()
        {
            if (_isUpdatingCalculatedFields)
            {
                return;
            }

            _isUpdatingCalculatedFields = true;

            try
            {
                _parameters.UpdateCalculatedParameters();
                textBoxL.Text = _parameters.GetParam(ParameterType.LengthL)
                    .ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                _isUpdatingCalculatedFields = false;
            }
        }

        /// <summary>
        /// Выполняет построение модели.
        /// </summary>
        private void BuildModel()
        {
            try
            {
                foreach (var textBox in _textBoxDict.Values)
                {
                    textBox.BackColor = Color.White;
                }
                richTextBox1.BackColor = Color.White;

                buttonBuild.Enabled = false;
                buttonBuild.Text = "Построение...";
                Application.DoEvents();
                bool nailPuller = checkBoxNail.Checked;
                _parameters.HasNailPuller = nailPuller;
                _builder.Build(_parameters);

                MessageBox.Show(
                    "✓ Модель успешно построена в КОМПАС-3D!",
                    "Успех",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка при " +
                    $"построении модели:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _parameters.Validate();
                buttonBuild.Enabled = !_parameters.ErrorCollector.Any();
                buttonBuild.Text = "Построить";
            }
        }
    }
}