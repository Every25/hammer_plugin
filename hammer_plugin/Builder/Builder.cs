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
        /// Путь к текущей папке сборки
        /// </summary>
        private string _currentAssemblyDirectory;

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

            _currentAssemblyDirectory =
                CreateUniqueAssemblyDirectory(parameters);

            string headFileName = "HammerHead.m3d";
            string handleFileName = "HammerHandle.m3d";
            string assemblyFileName = "Assembly.a3d";

            string headPartPath =
                Path.Combine(_currentAssemblyDirectory, headFileName);
            string handlePartPath =
                Path.Combine(_currentAssemblyDirectory, handleFileName);
            string assemblyPath =
                Path.Combine(_currentAssemblyDirectory, assemblyFileName);

            BuildHammerHead(parameters, headPartPath);
            BuildHandle(parameters, handlePartPath);

            BuildAssembly(headPartPath, handlePartPath, assemblyPath);
        }

        /// <summary>
        /// Строит головку молотка как отдельную деталь.
        /// </summary>
        private void BuildHammerHead(Parameters parameters, string savePath)
        {
            _wrapper.ResetForNewPart();
            _wrapper.CreateDocument3D();

            BuildHeadBody(parameters);
            BuildHole(parameters);
            BuildClaw(parameters);

            _wrapper.SavePartAs(savePath);
            _wrapper.CloseActiveDocument();
        }

        /// <summary>
        /// Строит основное тело головки молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildHeadBody(Parameters parameters)
        {
            List<ksEntity> sections = new List<ksEntity>();

            double size =
                parameters.GetParam(ParameterType.ClawWidthW);

            double middleDiameter = 
                parameters.GetParam(ParameterType.NeckDiameterB) / 1.4;

            double neckExtr = 
                parameters.GetParam(ParameterType.NeckWidthA) 
                + (parameters.GetParam(ParameterType.FaceWidthC) / 2);

            ksEntity sketch1 = _wrapper.CreateSketchOnPlane("YOZ");
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

            ksEntity sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
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

            ksEntity sketch3 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
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

            ksEntity sketch4 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
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

            ksEntity sketch5 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
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
        }


        /// <summary>
        /// Строит носок молотка.
        /// </summary>
        /// <param name="parameters">Параметры модели.</param>
        private void BuildClaw(Parameters parameters)
        {
            List<ksEntity> sections = new List<ksEntity>();
            double size = parameters.GetParam(ParameterType.ClawWidthW);
            ksEntity sketch1 = _wrapper.CreateSketchOnPlane("YOZ");
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
            ksEntity sketch2 = _wrapper.CreateSketchOnOffsetPlane("YOZ",
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
            ksEntity nailPullerSketch = _wrapper.CreateSketchOnPlane("XOY");
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
            ksEntity ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
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
        /// Строит рукоять молотка как отдельную деталь.
        /// </summary>
        private void BuildHandle(Parameters parameters, string savePath)
        {
            _wrapper.ResetForNewPart();
            _wrapper.CreateDocument3D();

            ksEntity ellipseSketch = _wrapper.CreateSketchOnPlane("XOY");
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
                parameters.GetParam(ParameterType.FaceDiameterD) / 2, true);

            _wrapper.Extrude(ellipseSketch,
                parameters.GetParam(ParameterType.HeightH) -
                (parameters.GetParam(ParameterType.FaceDiameterD) / 2),
                false);

            _wrapper.SavePartAs(savePath);
            _wrapper.CloseActiveDocument();
        }

        /// <summary>
        /// Создает сборку из головки и рукоятки молотка.
        /// </summary>
        private void BuildAssembly(string headPartPath,
            string handlePartPath, string assemblySavePath)
        {
            _wrapper.ResetForNewPart();
            _wrapper.CreateAssemblyDocument();

            _wrapper.InsertPartIntoAssembly(headPartPath);

            ksEntity headHolePlane = _wrapper.GetHolePlaneForMating();
            _wrapper.InsertPartIntoAssembly(handlePartPath, headHolePlane);

            _wrapper.UpdateAssembly();
            _wrapper.SaveAssemblyAs(assemblySavePath);
            //_wrapper.CloseActiveDocument();
        }

        /// <summary>
        /// Создает уникальную папку для хранения файлов текущей сборки
        /// </summary>
        /// <param name="parameters">Параметры модели</param>
        /// <returns>Путь к созданной папке</returns>
        private static string CreateUniqueAssemblyDirectory(
            Parameters parameters)
        {
            var baseDocumentsPath = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);

            var hammerBaseDir = Path.Combine(
                baseDocumentsPath, "HammerPlugin", "Models");

            string folderName = CreateAssemblyFolderName(parameters);
            var assemblyDir = Path.Combine(hammerBaseDir, folderName);
            int counter = 1;
            string originalAssemblyDir = assemblyDir;
            while (Directory.Exists(assemblyDir))
            {
                assemblyDir = $"{originalAssemblyDir}_{counter:00}";
                counter++;
            }

            Directory.CreateDirectory(assemblyDir);
            return assemblyDir;
        }

        /// <summary>
        /// Формирует имя папки для сборки на основе параметров
        /// </summary>
        private static string CreateAssemblyFolderName(Parameters parameters)
        {
            string format = "Hammer_H{0:0}_L{1:0}_D{2:0}_{3}_{4:yyyyMMdd_HHmmss}";

            return string.Format(
                CultureInfo.InvariantCulture,
                format,
                parameters.GetParam(ParameterType.HeightH),
                parameters.GetParam(ParameterType.LengthL),
                parameters.GetParam(ParameterType.FaceDiameterD),
                parameters.HasNailPuller ? "WithPuller" : "NoPuller",
                DateTime.Now
            );
        }

    }
}
