using HammerPluginCore.Model;
using Kompas6API5;
using System;
using System.Drawing;
using System.Globalization;

namespace HammerPlugin.Services
{
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
        /// <param name="parameters">параметры модели</param>
        public void Build(Parameters parameters)
        {

            _wrapper.RunCAD();
            _wrapper.CreateDocument3D();

            double middle = parameters.GetParameter(ParameterType.LengthL) - (parameters.GetParameter(ParameterType.ClawLengthL)
               + parameters.GetParameter(ParameterType.NeckWidthA) + (parameters.GetParameter(ParameterType.FaceWidthC) / 2));

            BuildHammerHead(parameters, middle);
            BuildHole(parameters, middle);
            BuildHandle(parameters, middle);

            try
            {
                var directory = GetModelsDirectory();
                var filePath = Path.Combine(directory, CreateFileName());
                SaveModel(filePath);
            }
            finally
            {
                _wrapper.CloseActiveDocument();
            }
        }

        /// <summary>
        /// Строит головку молотка.
        /// </summary>
        /// <param name="parameters">параметры модели</param>
        /// <param name="middle">длина средней части молотка</param>
        private void BuildHammerHead(Parameters parameters, double middle)
        {
            List<object> sections = new List<object>();

            double size = parameters.GetParameter(ParameterType.ClawWidthW);
            double middleDiameter = parameters.GetParameter(ParameterType.NeckDiameterB) / 1.4;
            double neckExtr = parameters.GetParameter(ParameterType.NeckWidthA) + (parameters.GetParameter(ParameterType.FaceWidthC) / 2);

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

            object sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ", middle / 4, false);
            try
            {
                _wrapper.DrawCircle(0, 0, middleDiameter / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch2);
                sections.Add(sketch2);
            }

            object sketch3 = _wrapper.CreateSketchOnOffsetPlane("YOZ", middle / 2, false);
            try
            {
                _wrapper.DrawCircle(0, 0, middleDiameter / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch3);
                sections.Add(sketch3);
            }

            object sketch4 = _wrapper.CreateSketchOnOffsetPlane("YOZ", middle, false);
            try
            {
                _wrapper.DrawCircle(0, 0, parameters.GetParameter(ParameterType.NeckDiameterB) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch4);
                sections.Add(sketch4);
            }

            _wrapper.Loft(sections);
            _wrapper.Extrude(sketch4, neckExtr, false);

            object sketch5 = _wrapper.CreateSketchOnOffsetPlane("YOZ", middle + neckExtr, false);
            try
            {
                _wrapper.DrawCircle(0, 0, parameters.GetParameter(ParameterType.FaceDiameterD) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(sketch5);
            }

            _wrapper.Extrude(sketch5, parameters.GetParameter(ParameterType.FaceWidthC), false, true);
            BuildClaw(parameters, size);
        }


        /// <summary>
        /// Строит носок молотка.
        /// </summary>
        /// <param name="parameters">параметры модели</param>
        /// <param name="size">длина головки молотка</param>
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
            object sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ", parameters.GetParameter(ParameterType.ClawLengthL), true);
            try
            {
                double firstRectLeftX = parameters.GetParameter(ParameterType.ClawWidthW) / 2;
                double firstRectBottomY = -parameters.GetParameter(ParameterType.ClawWidthW) / 2;
                double height = 0.5;
                double width = parameters.GetParameter(ParameterType.ClawWidthW);
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
        }

        /// <summary>
        /// Создает отверстие согласно заданным параметрам.
        /// </summary>
        /// <param name="parameters">параметры модели</param>
        /// <param name="middle">длина средней части молотка</param>
        private void BuildHole(Parameters parameters, double middle)
        {
            object ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
            try
            {
                _wrapper.DrawEllipse(middle / 2, 0, parameters.GetParameter(ParameterType.HeadHoleY1) / 2,
                    parameters.GetParameter(ParameterType.HeadHoleX1) / 2);
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
        /// <param name="parameters">параметры модели</param>
        /// <param name="middle">длина средней части молотка</param>
        private void BuildHandle(Parameters parameters, double middle)
        {
            object ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
            try
            {
                _wrapper.DrawEllipse(middle / 2, 0, parameters.GetParameter(ParameterType.HandleWidthY2) / 2,
                    parameters.GetParameter(ParameterType.HandleWidthX2) / 2);
            }
            finally
            {
                _wrapper.FinishSketch(ellipseSketch);
            }
            _wrapper.Extrude(ellipseSketch, parameters.GetParameter(ParameterType.FaceDiameterD) / 2, true);
            _wrapper.Extrude(ellipseSketch, parameters.GetParameter(ParameterType.HeightH)
                - (parameters.GetParameter(ParameterType.FaceDiameterD) / 2), false);
        }


        /// <summary>
        /// Сохраняет построенную модель в файл.
        /// </summary>
        /// <param name="path">путь к файлу</param>
        public void SaveModel(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Путь к файлу не задан.", nameof(path));
            }

            _wrapper.SaveAs(path);
        }

        /// <summary>
        /// Формирует уникальное имя файла по параметрам и времени.
        /// </summary>
        private static string CreateFileName()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Hammer",
                DateTime.Now);
        }

        /// <summary>
        /// Возвращает путь к директории, в которую будет сохраняться 3D-документ.
        /// </summary>
        private static string GetModelsDirectory()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directory = Path.Combine(documents, "HammerPlugin", "Models");
            Directory.CreateDirectory(directory);
            return directory;
        }

    }
}
