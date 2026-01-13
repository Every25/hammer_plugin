using NUnit.Framework;
using HammerPluginCore.Model;

namespace HammerPluginTests
{
    [TestFixture]
    [Description("Тесты для класса Parameter")]
    public class ParameterTests
    {
        [Test]
        [Description("Конструктор корректно инициализирует все поля")]
        public void Constructor_ShouldInitializeAllFields()
        {
            var parameter = new Parameter(10, 100, 50);

            Assert.AreEqual(10, parameter.MinValue);
            Assert.AreEqual(100, parameter.MaxValue);
            Assert.AreEqual(50, parameter.Value);
        }

        [Test]
        [Description("Value корректно устанавливается и возвращается")]
        public void Value_SetAndGet_ShouldWork()
        {
            var parameter = new Parameter(10, 100, 50);

            parameter.Value = 64;

            Assert.AreEqual(64, parameter.Value);
        }

        [Test]
        [Description("MinValue корректно устанавливается и возвращается")]
        public void MinValue_SetAndGet_ShouldWork()
        {
            var parameter = new Parameter(10, 100, 50);

            parameter.MinValue = 20;

            Assert.AreEqual(20, parameter.MinValue);
        }

        [Test]
        [Description("MaxValue корректно устанавливается и возвращается")]
        public void MaxValue_SetAndGet_ShouldWork()
        {
            var parameter = new Parameter(10, 100, 50);

            parameter.MaxValue = 400;

            Assert.AreEqual(400, parameter.MaxValue);
        }
    }
}
