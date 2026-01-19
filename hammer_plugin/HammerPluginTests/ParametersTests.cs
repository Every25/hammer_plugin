using NUnit.Framework;
using HammerPluginCore.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace HammerPluginTests
{
    [TestFixture]
    [Description("Тесты для класса Parameters")]
    public class ParametersTests
    {
        [Test]
        [Description("Конструктор должен инициализировать параметры " +
            "значениями по умолчанию")]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            var parameters = new Parameters();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    540, parameters.GetParam(ParameterType.HeightH));
                Assert.AreEqual(
                    85, parameters.GetParam(ParameterType.FaceDiameterD));
                Assert.AreEqual(
                    40, parameters.GetParam(ParameterType.FaceWidthC));
                Assert.AreEqual(
                    40, parameters.GetParam(ParameterType.NeckWidthA));
                Assert.AreEqual(
                    70, parameters.GetParam(ParameterType.NeckDiameterB));
                Assert.AreEqual(
                    32, parameters.GetParam(ParameterType.HeadHoleX1));
                Assert.AreEqual(
                    52, parameters.GetParam(ParameterType.HeadHoleY1));
                Assert.AreEqual(
                    32, parameters.GetParam(ParameterType.HandleWidthX2));
                Assert.AreEqual(
                    52, parameters.GetParam(ParameterType.HandleWidthY2));
                Assert.AreEqual(
                    200, parameters.GetParam(ParameterType.ClawLengthL));
                Assert.AreEqual(
                    60, parameters.GetParam(ParameterType.ClawWidthW));
            });
        }

        [Test]
        [Description("GetParam должен возвращать " +
            "корректное значение параметра")]
        public void GetParam_ShouldReturnCorrectValue()
        {
            var parameters = new Parameters();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    540, parameters.GetParam(ParameterType.HeightH));
                Assert.AreEqual(
                    360, parameters.GetParam(ParameterType.LengthL));
            });
        }

        [Test]
        [Description("SetParameter должен устанавливать значение " +
            "и обновлять вычисляемые параметры")]
        public void SetParameter_ShouldSetValueAndUpdateCalculatedParameters()
        {
            var parameters = new Parameters();
            double originalMiddleM =
                parameters.GetParam(ParameterType.MiddleM);
            double originalLengthL =
                parameters.GetParam(ParameterType.LengthL);
            double newMiddleM = 100;

            parameters.SetParameter(ParameterType.MiddleM, newMiddleM);
            double expectedLengthL = 
                originalLengthL + (newMiddleM - originalMiddleM);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    newMiddleM, parameters.GetParam(ParameterType.MiddleM));
                Assert.AreEqual(
                    expectedLengthL, parameters.GetParam(ParameterType.LengthL));
            });
        }

        [Test]
        [Description("Validate должен находить ошибки " +
            "при выходе за минимальные границы")]
        public void Validate_ShouldDetectMinBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149);

            var error = parameters.ErrorCollector.FirstOrDefault(
                e => e.ParameterType == ParameterType.HeightH);

            Assert.IsNotNull(error);
            StringAssert.Contains(
                "выходит за допустимые пределы", error.Message);
        }

        [Test]
        [Description("Validate должен находить ошибки " +
            "при выходе за максимальные границы")]
        public void Validate_ShouldDetectMaxBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.ClawLengthL, 251);

            var error = parameters.ErrorCollector.FirstOrDefault(
                e => e.ParameterType == ParameterType.ClawLengthL);

            Assert.IsNotNull(error);
            StringAssert.Contains(
                "выходит за допустимые пределы", error.Message);
        }

        [Test]
        [Description("Validate должен находить ошибку " +
            "при NeckDiameter >= FaceDiameter")]
        public void Validate_ShouldDetectNeckDiameterGreaterOrEqualFaceDiameter()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.FaceDiameterD, 85);
            parameters.SetParameter(ParameterType.NeckDiameterB, 85);

            var neckError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.NeckDiameterB)
                .FirstOrDefault(e => e.Message.Contains("диаметра бойка D"));

            var faceError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.FaceDiameterD)
                .FirstOrDefault(e => e.Message.Contains("диаметра бойка D"));

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(
                    neckError, "Ошибка для NeckDiameterB не найдена");
                Assert.IsNotNull(
                    faceError, "Ошибка для FaceDiameterD не найдена");
            });
        }

        [Test]
        [Description("Validate должен находить ошибку " +
            "при ClawWidth > FaceDiameter")]
        public void Validate_ShouldDetectClawWidthGreaterThanFaceDiameter()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.FaceDiameterD, 85);
            parameters.SetParameter(ParameterType.ClawWidthW, 86);

            var clawError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.ClawWidthW)
                .FirstOrDefault(e => e.Message.Contains("диаметра бойка D"));

            var faceError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.FaceDiameterD)
                .FirstOrDefault(e => e.Message.Contains("диаметра бойка D"));


            Assert.Multiple(() =>
            {
                Assert.IsNotNull(clawError);
                Assert.IsNotNull(faceError);
            });
        }

        [Test]
        [Description("Validate должен находить ошибку " +
            "при HeadHoleY1 >= MiddleM")]
        public void Validate_ShouldDetectHeadHoleY1GreaterOrEqualMiddleM()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.MiddleM, 50);
            parameters.SetParameter(ParameterType.HeadHoleY1, 55);

            var error = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.HeadHoleY1)
                .FirstOrDefault(e => e.Message.Contains("длины средней части m"));

            Assert.IsNotNull(
                error, "Ожидаемая ошибка зависимости для HeadHoleY1 не найдена.");
        }

        [Test]
        [Description("Validate должен находить ошибку " +
            "при HeadHoleX1 >= NeckDiameter")]
        public void Validate_ShouldDetectHeadHoleX1GreaterOrEqualNeckDiameter()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.NeckDiameterB, 50);
            parameters.SetParameter(ParameterType.HeadHoleX1, 55);

            var holeError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.HeadHoleX1)
                .FirstOrDefault(e => e.Message.Contains("ширины выступа b"));

            var neckError = parameters.ErrorCollector
                .Where(e => e.ParameterType == ParameterType.NeckDiameterB)
                .FirstOrDefault(e => e.Message.Contains("ширины выступа b"));

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(
                    holeError, "Ошибка зависимости для HeadHoleX1 не найдена.");
                Assert.IsNotNull(
                    neckError, "Ошибка зависимости для NeckDiameterB не найдена.");
            });
        }


        [Test]
        [Description("Проверка всех минимальных граничных значений")]
        public void Validate_ShouldDetectAllMinBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.ClearErrors();
            parameters.SetParameter(ParameterType.HeightH, 149);
            parameters.SetParameter(ParameterType.MiddleM, 19);
            parameters.SetParameter(ParameterType.FaceDiameterD, 19);
            parameters.SetParameter(ParameterType.FaceWidthC, 9);
            parameters.SetParameter(ParameterType.NeckWidthA, 9);
            parameters.SetParameter(ParameterType.NeckDiameterB, 14);
            parameters.SetParameter(ParameterType.HeadHoleX1, 9);
            parameters.SetParameter(ParameterType.HeadHoleY1, 9);
            parameters.SetParameter(ParameterType.ClawLengthL, 49);
            parameters.SetParameter(ParameterType.ClawWidthW, 9);
            parameters.Validate();

            var rangeErrors = parameters.ErrorCollector.Count(
                e => e.Message.Contains("выходит за допустимые пределы"));
            Assert.AreEqual(12, rangeErrors);
        }

        [Test]
        [Description("Проверка всех максимальных граничных значений")]
        public void Validate_ShouldDetectAllMaxBoundaryViolations()
        {
            var parameters = new Parameters();
            parameters.ClearErrors();
            parameters.SetParameter(ParameterType.HeightH, 601);
            parameters.SetParameter(ParameterType.MiddleM, 101);
            parameters.SetParameter(ParameterType.FaceDiameterD, 151);
            parameters.SetParameter(ParameterType.FaceWidthC, 81);
            parameters.SetParameter(ParameterType.NeckWidthA, 81);
            parameters.SetParameter(ParameterType.NeckDiameterB, 101);
            parameters.SetParameter(ParameterType.HeadHoleX1, 61);
            parameters.SetParameter(ParameterType.HeadHoleY1, 61);
            parameters.SetParameter(ParameterType.ClawLengthL, 251);
            parameters.SetParameter(ParameterType.ClawWidthW, 101);
            parameters.Validate();

            var rangeErrors = parameters.ErrorCollector.Count(
                e => e.Message.Contains("выходит за допустимые пределы"));
            Assert.AreEqual(12, rangeErrors);
        }

        [Test]
        [Description("Validate не должен находить ошибок " +
            "при валидных параметрах")]
        public void Validate_ShouldNotFindErrorsWithValidParameters()
        {
            var parameters = new Parameters();
            parameters.ClearErrors();
            parameters.SetParameter(ParameterType.NeckDiameterB, 50);
            parameters.SetParameter(ParameterType.FaceDiameterD, 60);
            parameters.SetParameter(ParameterType.HeadHoleX1, 30);
            parameters.SetParameter(ParameterType.HeadHoleY1, 20);
            parameters.SetParameter(ParameterType.ClawWidthW, 50);

            Assert.IsEmpty(parameters.ErrorCollector);
        }

        [Test]
        [Description("ClearErrors должен очищать список ошибок")]
        public void ClearErrors_ShouldClearErrorList()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149);
            Assert.IsNotEmpty(parameters.ErrorCollector);
            parameters.ClearErrors();
            Assert.IsEmpty(parameters.ErrorCollector);
        }

        [Test]
        [Description("ErrorCollector должен возвращать " +
            "корректный список ошибок")]
        public void ErrorCollector_ShouldReturnCorrectErrorList()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 149);
            var errors = parameters.ErrorCollector;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(errors);
                Assert.IsInstanceOf<List<ValidationError>>(errors);
                Assert.IsNotEmpty(errors);
            });
        }

        [Test]
        [Description("UpdateCalculatedParameters должен " +
            "правильно вычислять LengthL")]
        public void UpdateCalculatedParameters_ShouldCorrectlyCalculateLengthL()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.MiddleM, 80);
            parameters.SetParameter(ParameterType.NeckWidthA, 40);
            parameters.SetParameter(ParameterType.FaceWidthC, 40);
            parameters.SetParameter(ParameterType.ClawLengthL, 200);
            parameters.UpdateCalculatedParameters();

            double expectedLength = 80 + 40 + 40 + 200;
            double actualLength = parameters.GetParam(ParameterType.LengthL);
            Assert.AreEqual(expectedLength, actualLength,
                $"Ожидалась длина {expectedLength}," +
                $" но получено {actualLength}");
        }

        [Test]
        [Description("UpdateCalculatedParameters должен " +
            "обновлять HandleWidth")]
        public void UpdateCalculatedParameters_ShouldUpdateHandleWidth()
        {
            var parameters = new Parameters();
            double newHeadHoleX1 = 40;
            double newHeadHoleY1 = 55;
            parameters.SetParameter(ParameterType.HeadHoleX1, newHeadHoleX1);
            parameters.SetParameter(ParameterType.HeadHoleY1, newHeadHoleY1);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(
                    newHeadHoleX1, 
                    parameters.GetParam(ParameterType.HandleWidthX2));
                Assert.AreEqual(
                    newHeadHoleY1, 
                    parameters.GetParam(ParameterType.HandleWidthY2));
            });
        }

        [Test]
        [Description("SetParameter для вычисляемого параметра LengthL " +
            "должен быть перезаписан")]
        public void SetParameter_ForCalculatedLengthL_ShouldBeOverwritten()
        {
            var parameters = new Parameters();
            var expectedLength = parameters.GetParam(ParameterType.LengthL);
            var arbitraryValue = 999;

            parameters.SetParameter(ParameterType.LengthL, arbitraryValue);
            var actualLength = parameters.GetParam(ParameterType.LengthL);

            Assert.AreNotEqual(arbitraryValue, actualLength);
            Assert.AreEqual(expectedLength, actualLength);
        }

        [Test]
        [Description("SetParameter для вычисляемого параметра " +
            "HandleWidthX2 должен быть перезаписан")]
        public void SetParameter_ForCalculatedHandleWidthX2_ShouldBeOverwritten()
        {
            var parameters = new Parameters();
            var expectedValue = parameters.GetParam(ParameterType.HeadHoleX1);
            var arbitraryValue = 999;

            parameters.SetParameter(ParameterType.HandleWidthX2, arbitraryValue);
            var actualValue = parameters.GetParam(ParameterType.HandleWidthX2);

            Assert.AreNotEqual(arbitraryValue, actualValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        [Description("SetParameter для вычисляемого параметра " +
            "HandleWidthY2 должен быть перезаписан")]
        public void SetParameter_ForCalculatedHandleWidthY2_ShouldBeOverwritten()
        {
            var parameters = new Parameters();
            var expectedValue =
                parameters.GetParam(ParameterType.HeadHoleY1);
            var arbitraryValue = 999;
            
            parameters
                .SetParameter(ParameterType.HandleWidthY2, arbitraryValue);
            var actualValue = 
                parameters.GetParam(ParameterType.HandleWidthY2);

            Assert.AreNotEqual(arbitraryValue, actualValue);
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
