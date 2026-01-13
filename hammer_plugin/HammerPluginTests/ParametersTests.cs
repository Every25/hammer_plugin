using NUnit.Framework;
using HammerPluginCore.Model;

namespace HammerPluginTests
{
    [TestFixture]
    [Description("Тесты для класса Parameters")]
    public class ParametersTests
    {
        [Test]
        [Description("Конструктор должен инициализировать параметры значениями по умолчанию")]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            var parameters = new Parameters();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(540, parameters.GetParameter(ParameterType.HeightH));
                Assert.AreEqual(360, parameters.GetParameter(ParameterType.LengthL));
                Assert.AreEqual(85, parameters.GetParameter(ParameterType.FaceDiameterD));
                Assert.AreEqual(40, parameters.GetParameter(ParameterType.FaceWidthC));
                Assert.AreEqual(40, parameters.GetParameter(ParameterType.NeckWidthA));
                Assert.AreEqual(70, parameters.GetParameter(ParameterType.NeckDiameterB));
                Assert.AreEqual(32, parameters.GetParameter(ParameterType.HeadHoleX1));
                Assert.AreEqual(52, parameters.GetParameter(ParameterType.HeadHoleY1));
                Assert.AreEqual(32, parameters.GetParameter(ParameterType.HandleWidthX2));
                Assert.AreEqual(52, parameters.GetParameter(ParameterType.HandleWidthY2));
                Assert.AreEqual(200, parameters.GetParameter(ParameterType.ClawLengthL));
                Assert.AreEqual(60, parameters.GetParameter(ParameterType.ClawWidthW));
            });
        }

        [Test]
        [Description("GetParameter должен возвращать корректный объект BrickParameter")]
        public void GetParameter_ShouldReturnCorrectParameter()
        {
            var parameters = new Parameters();

            var height = parameters.GetParameter(ParameterType.HeightH);
            var length = parameters.GetParameter(ParameterType.LengthL);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(height);
                Assert.AreEqual(540, height);
                Assert.AreEqual(360, length);
            });
        }

        [Test]
        [Description("SetParameter должен устанавливать значение и обновляеть вычисляемые параметры")]
        public void SetParameter_ShouldSetValueAndUpdateCalculatedParameters()
        {
            var parameters = new Parameters();
            double newHeight = 600;

            parameters.SetParameter(ParameterType.HeightH, newHeight);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(newHeight, parameters.GetParameter(ParameterType.HeightH));
                Assert.AreEqual(400, parameters.GetParameter(ParameterType.LengthL));
            });
        }

        [Test]
        [Description("Validate должен находить ошибки при выходе за минимальные границы")]
        public void Validate_ShouldDetectMinBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149);

            parameters.Validate();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(parameters.ErrorCollector.Count > 0);
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.HeightH &&
                    e.Message.Contains("выходит за допустимые пределы")));
            });
        }

        [Test]
        [Description("Validate должен находить ошибки при выходе за максимальные границы")]
        public void Validate_ShouldDetectMaxBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.ClawLengthL, 251);

            parameters.Validate();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(parameters.ErrorCollector.Count > 0);
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.ClawLengthL &&
                    e.Message.Contains("выходит за допустимые пределы")));
            });
        }

        [Test]
        [Description("Validate должен находить ошибку при NeckDiameter >= FaceDiameter")]
        public void Validate_ShouldDetectNeckDiameterGreaterOrEqualFaceDiameter()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.NeckDiameterB, 85);
            parameters.SetParameter(ParameterType.FaceDiameterD, 85);

            parameters.Validate();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(parameters.ErrorCollector.Count > 0);
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.NeckDiameterB &&
                    e.Message.Contains("должен быть меньше диаметра бойка D")));
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.FaceDiameterD &&
                    e.Message.Contains("должен быть меньше диаметра бойка D")));
            });
        }

        [Test]
        [Description("Validate должен находить ошибку при ClawWidth > FaceDiameter")]
        public void Validate_ShouldDetectClawWidthGreaterThanFaceDiameter()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.ClawWidthW, 86);
            parameters.SetParameter(ParameterType.FaceDiameterD, 85);

            parameters.Validate();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(parameters.ErrorCollector.Count > 0);
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.ClawWidthW &&
                    e.Message.Contains("должна быть не больше диаметра бойка D")));
                Assert.IsTrue(parameters.ErrorCollector.Any(e =>
                    e.ParameterType == ParameterType.FaceDiameterD &&
                    e.Message.Contains("должна быть не больше диаметра бойка D")));
            });
        }

        [Test]
        [Description("Validate не должен находить ошибок при валидных параметрах")]
        public void Validate_ShouldNotFindErrorsWithValidParameters()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.LengthL, 500);
            parameters.SetParameter(ParameterType.ClawLengthL, 100);
            parameters.SetParameter(ParameterType.NeckWidthA, 40);
            parameters.SetParameter(ParameterType.FaceWidthC, 40);
            parameters.SetParameter(ParameterType.HeadHoleY1, 20);
            parameters.SetParameter(ParameterType.NeckDiameterB, 50);
            parameters.SetParameter(ParameterType.FaceDiameterD, 60);
            parameters.SetParameter(ParameterType.HeadHoleX1, 30);
            parameters.SetParameter(ParameterType.ClawWidthW, 50);

            parameters.Validate();

            Assert.IsEmpty(parameters.ErrorCollector,
                $"Найдены ошибки: {string.Join("; ", parameters.ErrorCollector.Select(e => e.Message))}");
        }

        [Test]
        [Description("ClearErrors должен очищать список ошибок")]
        public void ClearErrors_ShouldClearErrorList()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149); // Создаем ошибку
            parameters.Validate();
            Assert.IsTrue(parameters.ErrorCollector.Count > 0);

            parameters.ClearErrors();

            Assert.IsEmpty(parameters.ErrorCollector);
        }

        [Test]
        [Description("ErrorCollector должен возвращать корректный список ошибок")]
        public void ErrorCollector_ShouldReturnCorrectErrorList()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149);
            parameters.Validate();


            var errors = parameters.ErrorCollector;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(errors);
                Assert.IsInstanceOf<List<ValidationError>>(errors);
                Assert.IsTrue(errors.Count > 0);
            });
        }

        [Test]
        [Description("UpdateCalculatedParameters должен правильно вычислять LengthL")]
        public void UpdateCalculatedParameters_ShouldCorrectlyCalculateLengthL()
        {
            var parameters = new Parameters();
            double[] testHeights = { 150, 300, 450, 600 };
            double[] expectedLengths = { 100, 200, 300, 400 };

            for (int i = 0; i < testHeights.Length; i++)
            {
                parameters.SetParameter(ParameterType.HeightH, testHeights[i]);

                Assert.AreEqual(expectedLengths[i], parameters.GetParameter(ParameterType.LengthL),
                    $"Для высоты {testHeights[i]} ожидалась длина {expectedLengths[i]}");
            }
        }
    }
}
