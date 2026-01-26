using HammerPluginCore.Model;
using Kompas6API5;
using System;
using System.Drawing;
using System.Globalization;
using static System.Windows.Forms.AxHost;

namespace HammerPluginBuilder
{
    //TODO: refactor +
    /// <summary>
    /// Строит 3D-модель молотка в КОМПАС-3D.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Обертка для работы с API КОМПАС-3D.
        /// </summary>
        private readonly Wrapper _wrapper;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public Builder()
        {
            _wrapper = new Wrapper();
        }

        /// <summary>
        /// Главный сценарий построения модели молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        public void Build(Parameters parameters)
        {
            _wrapper.OpenKompas();
            _wrapper.CreateDocument3D();

            BuildHammerHead(parameters);
            BuildHole(parameters);
            BuildHandle(parameters);
        }

        /// <summary>
        /// Строит головку молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildHammerHead(Parameters parameters)
        {
            List<object> sections = new List<object>();

            double size =
                parameters.GetParam(ParameterType.ClawWidthW);

            double middleDiameter = 
                parameters.GetParam(ParameterType.NeckDiameterB) / 1.4;

            double neckExtr = 
                parameters.GetParam(ParameterType.NeckWidthA) 
                + (parameters.GetParam(ParameterType.FaceWidthC) / 2);

            object sketch1 = _wrapper.CreateSketchOnPlane("YOZ");
            try
            {
                double leftX = -size / 2;
                double bottomY = -size / 2;
                double rightX = size / 2;
                double topY = size / 2;

                _wrapper.DrawRectangle(leftX, bottomY, rightX, topY);

            }
            finally
            {
                _wrapper.FinishSketch(sketch1);
                sections.Add(sketch1);
            }

            object sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
                parameters.GetParam(ParameterType.MiddleM) / 4, false);
            try
            {
                _wrapper.DrawCircle(0, 0, middleDiameter / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch2);
                sections.Add(sketch2);
            }

            object sketch3 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
                parameters.GetParam(ParameterType.MiddleM) / 2, false);
            try
            {
                _wrapper.DrawCircle(0, 0, middleDiameter / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch3);
                sections.Add(sketch3);
            }

            object sketch4 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
                parameters.GetParam(ParameterType.MiddleM), false);
            try
            {
                _wrapper.DrawCircle(0, 0, 
                    parameters.GetParam(ParameterType.NeckDiameterB) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch4);
                sections.Add(sketch4);
            }

            _wrapper.Loft(sections);
            _wrapper.Extrude(sketch4, neckExtr, false);

            object sketch5 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
                parameters.GetParam(ParameterType.MiddleM) 
                + neckExtr, false);
            try
            {
                _wrapper.DrawCircle(0, 0, 
                    parameters.GetParam(ParameterType.FaceDiameterD) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch5);
            }

            _wrapper.Extrude(sketch5, 
                parameters.GetParam(ParameterType.FaceWidthC), false, true);
            BuildClaw(parameters, size);
        }


        /// <summary>
        /// Строит носок молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        /// <param name="size">Длина головки молотка.</param>
        private void BuildClaw(Parameters parameters, double size)
        {
            List<object> sections = new List<object>();
            object sketch1 = _wrapper.CreateSketchOnPlane("YOZ");
            try
            {
                double leftX = -size / 2;
                double bottomY = -size / 2;
                double rightX = size / 2;
                double topY = size / 2;

                _wrapper.DrawRectangle(leftX, bottomY, rightX, topY);

            }
            finally
            {
                _wrapper.FinishSketch(sketch1);
                sections.Add(sketch1);
            }
            sections.Add(sketch1);
            object sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
                parameters.GetParam(ParameterType.ClawLengthL), true);
            try
            {
                double firstRectLeftX =
                    parameters.GetParam(ParameterType.ClawWidthW) / 2;
                double firstRectBottomY =
                    -parameters.GetParam(ParameterType.ClawWidthW) / 2;
                double height = 0.5;
                double width =
                    parameters.GetParam(ParameterType.ClawWidthW);
                double leftX = firstRectLeftX;
                double bottomY = firstRectBottomY;
                double rightX = firstRectLeftX + height;
                double topY = firstRectBottomY + width;

                _wrapper.DrawRectangle(leftX, bottomY, rightX, topY);
            }
            finally
            {
                _wrapper.FinishSketch(sketch2);
                sections.Add(sketch2);
            }
            _wrapper.Loft(sections);
            
            if (parameters.HasNailPuller)
            {
                BuildNailPuller(parameters);
            }
        }

        /// <summary>
        /// Создает отверстие гвоздодера.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildNailPuller(Parameters parameters)
        {
            object nailPullerSketch = _wrapper.CreateSketchOnPlane("XOY");
            try
            {
                double xc =
                    -parameters.GetParam(ParameterType.ClawLengthL) / 1.5;
                double yc = 0;
                double rad =
                    parameters.GetParam(ParameterType.ClawWidthW) / 9.8;
                double x1 = xc;
                double y1 = yc + rad;
                double x2 = xc;
                double y2 = yc - rad;
                _wrapper.DrawArcByThreePoints(
                    xc, yc, rad, x1, y1, x2, y2, -1);
                double x3 = 
                    -parameters.GetParam(ParameterType.ClawLengthL);
                double y3 = 
                    parameters.GetParam(ParameterType.ClawWidthW) / 6;
                double x4 = x3;
                double y4 = -y3;
                _wrapper.DrawLine(x1, y1, x3, y3);
                _wrapper.DrawLine(x2, y2, x4, y4);
                _wrapper.DrawLine(x3, y3, x4, y4);
            }
            finally
            {
                _wrapper.FinishSketch(nailPullerSketch);
            }
            _wrapper.Cut(nailPullerSketch);
        }

        /// <summary>
        /// Создает отверстие согласно заданным параметрам.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildHole(Parameters parameters)
        {
            object ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
            try
            {
                _wrapper.DrawEllipse(
                    parameters.GetParam(ParameterType.MiddleM) / 2, 0, 
                    parameters.GetParam(ParameterType.HeadHoleY1) / 2,
                    parameters.GetParam(ParameterType.HeadHoleX1) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(ellipseSketch);
            }
            _wrapper.Cut(ellipseSketch);
        }

        /// <summary>
        /// Строит рукоять молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildHandle(Parameters parameters)
        {
            object ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
            try
            {
                _wrapper.DrawEllipse(
                    parameters.GetParam(ParameterType.MiddleM) / 2, 0, 
                    parameters.GetParam(ParameterType.HandleWidthY2) / 2,
                    parameters.GetParam(ParameterType.HandleWidthX2) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(ellipseSketch);
            }
            _wrapper.Extrude(ellipseSketch,
                parameters.GetParam(ParameterType.FaceDiameterD) / 2, 
                true);

            _wrapper.Extrude(ellipseSketch, 
                parameters.GetParam(ParameterType.HeightH) - 
                (parameters.GetParam(ParameterType.FaceDiameterD) / 2),
                false);
        }
    }
}
