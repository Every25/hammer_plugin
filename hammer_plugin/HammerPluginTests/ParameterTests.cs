using NUnit.Framework;
using HammerPluginCore.Model;
using System;

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
        [Description("Value установка граничных значений")]
        public void Value_SetBoundaryValues_ShouldWork()
        {
            var parameter = new Parameter(10, 100, 50);

            parameter.Value = 10;
            Assert.AreEqual(10, parameter.Value);

            parameter.Value = 100;
            Assert.AreEqual(100, parameter.Value);
        }

        [Test]
        [Description("Value установка значения " +
            "ниже минимального вызывает исключение")]
        public void Value_SetBelowMin_ShouldThrowArgumentOutOfRangeException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                parameter.Value = 5;
            });

            StringAssert.Contains("Значение 5 " +
                "вне допустимого диапазона [10, 100]", ex.Message);
        }

        [Test]
        [Description("Value установка значения " +
            "выше максимального вызывает исключение")]
        public void Value_SetAboveMax_ShouldThrowArgumentOutOfRangeException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                parameter.Value = 150;
            });

            StringAssert.Contains("Значение 150 " +
                "вне допустимого диапазона [10, 100]", ex.Message);
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
        [Description("MinValue установка значения больше " +
            "MaxValue вызывает исключение")]
        public void MinValue_SetGreaterThanMaxValue_ShouldThrowArgumentException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                parameter.MinValue = 150;
            });

            StringAssert.Contains("Минимальное значение (150) " +
                "не может быть больше максимального значения (100)", ex.Message);
        }

        [Test]
        [Description("MinValue установка значения больше " +
            "текущего Value вызывает исключение")]
        public void MinValue_SetGreaterThanCurrentValue_ShouldThrowArgumentOutOfRangeException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                parameter.MinValue = 75;
            });

            StringAssert.Contains("Текущее значение (50) меньше " +
                "нового минимального значения (75)", ex.Message);
        }

        [Test]
        [Description("MaxValue корректно устанавливается и возвращается")]
        public void MaxValue_SetAndGet_ShouldWork()
        {
            var parameter = new Parameter(10, 100, 50);

            parameter.MaxValue = 400;

            Assert.AreEqual(400, parameter.MaxValue);
        }

        [Test]
        [Description("MaxValue установка значения " +
            "меньше MinValue вызывает исключение")]
        public void MaxValue_SetLessThanMinValue_ShouldThrowArgumentException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                parameter.MaxValue = 5;
            });

            StringAssert.Contains("Максимальное значение (5) не может быть " +
                "меньше минимального значения (10)", ex.Message);
        }

        [Test]
        [Description("MaxValue установка значения меньше " +
            "текущего Value вызывает исключение")]
        public void MaxValue_SetLessThanCurrentValue_ShouldThrowInvalidOperationException()
        {
            var parameter = new Parameter(10, 100, 50);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                parameter.MaxValue = 30;
            });

            StringAssert.Contains("Текущее значение (50) больше " +
                "нового максимального значения (30)", ex.Message);
        }

    }
}