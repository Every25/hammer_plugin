using NUnit.Framework;
using HammerPluginCore.Model;

namespace HammerPluginTests
{
    [TestFixture]
    [Description("Тесты для класса ValidationError")]
    public class ValidationErrorTests
    {
        [Test]
        [Description("Конструктор должен корректно " +
            "инициализировать параметр и сообщение")]
        public void Constructor_ShouldInitializeParameterAndMessage()
        {
            var expectedParameter = ParameterType.HeightH;
            var expectedMessage = "Значение выходит за допустимые пределы";

            var error = new ValidationError(
                expectedParameter, expectedMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    expectedParameter, error.ParameterType);
                Assert.AreEqual(
                    expectedMessage, error.Message);
            });
        }

        [Test]
        [Description("Свойство ParameterType должно возвращать " +
            "корректное значение")]
        public void ParameterType_ShouldReturnCorrectValue()
        {
            var expectedParameter = ParameterType.FaceDiameterD;
            var error = new ValidationError(
                expectedParameter, "Test message");

            var actualParameter = error.ParameterType;

            Assert.AreEqual(expectedParameter, actualParameter);
        }

        [Test]
        [Description("Свойство Message должно возвращать " +
            "корректное значение")]
        public void Message_ShouldReturnCorrectValue()
        {
            var expectedMessage = "Диаметр выступа b должен быть меньше " +
                "диаметра бойка D.";
            var error = new ValidationError(
                ParameterType.NeckDiameterB, expectedMessage);

            var actualMessage = error.Message;

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [Test]
        [Description("Можно создать ошибки для всех типов параметров")]
        public void Constructor_ShouldWorkForAllParameterTypes()
        {
            var allParameterTypes = Enum.GetValues(typeof(ParameterType));

            foreach (ParameterType parameterType in allParameterTypes)
            {
                var error = new ValidationError(
                    parameterType, $"Error for {parameterType}");

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(
                        parameterType, error.ParameterType);
                    Assert.AreEqual(
                        $"Error for {parameterType}", error.Message);
                });
            }
        }

        [Test]
        [Description("Две ошибки с разными параметрами " +
            "не должны быть равны")]
        public void TwoErrors_WithDifferentParameters_ShouldNotBeEqual()
        {
            var error1 = new ValidationError(
                ParameterType.HeightH, "Same message");
            var error2 = new ValidationError(
                ParameterType.LengthL, "Same message");

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(
                    error1.ParameterType, error2.ParameterType);
                Assert.AreEqual(
                    error1.Message, error2.Message);
            });
        }

        [Test]
        [Description("Тест с длинным сообщением об ошибке")]
        public void Constructor_ShouldHandleLongMessages()
        {
            var longMessage = new string('A', 1000);
            var error = new ValidationError(
                ParameterType.ClawLengthL, longMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    longMessage, error.Message);
                Assert.AreEqual(
                    ParameterType.ClawLengthL, error.ParameterType);
            });
        }
    }
}