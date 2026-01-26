using Kompas6API5;
using Kompas6Constants;
using Kompas6Constants3D;
using System;
using System.Runtime.InteropServices;
using static System.Windows.Forms.AxHost;


namespace HammerPluginBuilder
{
    /// <summary>
    /// Обертка для работы с API КОМПАС-3D.
    /// </summary>
    public class Wrapper
    {
        /// <summary>
        /// Объект для работы с КОМПАС-3D.
        /// </summary>
        private KompasObject _kompas;

        /// <summary>
        /// 3D-документ.
        /// </summary>
        private ksDocument3D _doc3D;

        /// <summary>
        /// Деталь в 3D документе.
        /// </summary>
        private ksPart _part;

        /// <summary>
        /// Текущий 2D-документ эскиза.
        /// </summary>
        private ksDocument2D _current2dDoc;

        /// <summary>
        /// Подключается к запущенному КОМПАС-3D или запускает новый процесс.
        /// Если подключение "протухло", пересоздает его.
        /// </summary>
        public void OpenKompas()
        {
            if (_kompas != null)
            {
                try
                {
                    var isVisible = _kompas.Visible;
                    return;
                }
                catch (COMException)
                {
                    ReleaseComObject(_kompas);
                    _kompas = null;
                }
            }

            //TODO: RSDN +
            var kompasType = Type.GetTypeFromProgID("KOMPAS.Application.5");
            if (kompasType == null)
            {
                throw new InvalidOperationException(
                    "Не найден ProgID KOMPAS.Application.5. " +
                    "Убедитесь, что КОМПАС-3D установлен.");
            }

            _kompas = (KompasObject)Activator.CreateInstance(kompasType)
                ?? throw new InvalidOperationException(
                    "Не удалось создать KompasObject.");

            _kompas.Visible = true;
            _kompas.ActivateControllerAPI();
        }

        /// <summary>
        /// Создаёт новый 3D-документ (деталь) и получает верхнюю деталь.
        /// </summary>
        public void CreateDocument3D()
        {
            if (_kompas == null)
            {
                throw new InvalidOperationException(
                    "Kompas не инициализирован. Сначала вызови OpenKompas().");
            }

            _doc3D = (ksDocument3D)_kompas.Document3D()
                     ?? throw new InvalidOperationException(
                         "Не удалось получить ksDocument3D.");

            _doc3D.Create();

            _part = (ksPart)_doc3D.GetPart((short)Part_Type.pTop_Part)
                       ?? throw new InvalidOperationException(
                           "Не удалось получить верхнюю деталь.");
        }

        /// <summary>
        /// Создаёт эскиз на базовой плоскости.
        /// </summary>
        /// <param name="plane">Название базовой плоскости:
        /// "XOY", "XOZ", "YOZ".</param>
        /// <returns>Объект эскиза (ksEntity).</returns>
        public object CreateSketchOnPlane(string plane)
        {
            if (_part == null)
            {
                throw new InvalidOperationException(
                    "Часть не инициализирована. Вызови CreateDocument3D().");
            }

            short planeType = plane?.ToUpperInvariant() switch
            {
                "XOY" => (short)Obj3dType.o3d_planeXOY,
                "XOZ" => (short)Obj3dType.o3d_planeXOZ,
                "YOZ" => (short)Obj3dType.o3d_planeYOZ,
                _ => (short)Obj3dType.o3d_planeXOY
            };

            var basePlane = (ksEntity)_part.GetDefaultEntity(planeType)
                           ?? throw new InvalidOperationException(
                               "Не удалось получить базовую плоскость.");

            var sketchEntity = (ksEntity)_part
                               .NewEntity((short)Obj3dType.o3d_sketch)
                               ?? throw new InvalidOperationException(
                                  "Не удалось создать сущность o3d_sketch.");

            var sketchDef = (ksSketchDefinition)sketchEntity.GetDefinition();
            sketchDef.SetPlane(basePlane);
            sketchEntity.Create();

            _current2dDoc = (ksDocument2D)sketchDef.BeginEdit();

            return sketchEntity;
        }

        /// <summary>
        /// Рисует прямоугольник в текущем эскизе.
        /// </summary>
        /// <param name="x1">X-координата левой нижней точки.</param>
        /// <param name="y1">Y-координата левой нижней точки.</param>
        /// <param name="x2">X-координата правой верхней точки.</param>
        /// <param name="y2">Y-координата правой верхней точки.</param>
        public void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            if (_current2dDoc == null)
            {
                throw new InvalidOperationException(
                    "Нет активного 2D-эскиза." +
                    " Сначала вызови CreateSketchOnPlane().");
            }

            _current2dDoc.ksLineSeg(x1, y1, x2, y1, 1);
            _current2dDoc.ksLineSeg(x2, y1, x2, y2, 1);
            _current2dDoc.ksLineSeg(x2, y2, x1, y2, 1);
            _current2dDoc.ksLineSeg(x1, y2, x1, y1, 1);
        }

        /// <summary>
        /// Рисует окружность на активном эскизе.
        /// </summary>
        /// <param name="xc">X-координата центра окружности.</param>
        /// <param name="yc">Y-координата центра окружности.</param>
        /// <param name="radius">Радиус окружности.</param>
        public void DrawCircle(double xc, double yc, double radius)
        {
            if (_current2dDoc == null)
            {
                throw new InvalidOperationException(
                    "Нет активного 2D-эскиза." +
                    " Сначала вызови CreateSketchOnPlane().");
            }

            _current2dDoc.ksCircle(xc, yc, radius, 1);
        }

        /// <summary>
        /// Рисует эллипс на активном 2D-эскизе.
        /// </summary>
        /// <param name="xc">X-координата центра эллипса.</param>
        /// <param name="yc">Y-координата центра эллипса.</param>
        /// <param name="xRadius">Радиус по оси X.</param>
        /// <param name="yRadius">Радиус по оси Y.</param>
        public void DrawEllipse(double xc, double yc,
            double xRadius, double yRadius)
        {
            if (_current2dDoc == null)
            {
                throw new InvalidOperationException(
                    "Нет активного 2D-эскиза." +
                    " Сначала вызови CreateSketchOnPlane().");
            }

            ksEllipseParam ellipseParam =
                (ksEllipseParam)_kompas.GetParamStruct(
                (short)StructType2DEnum.ko_EllipseParam);

            if (ellipseParam == null)
            {
                throw new InvalidOperationException(
                    "Не удалось получить параметры эллипса.");
            }

            ellipseParam.Init();
            ellipseParam.xc = xc;
            ellipseParam.yc = yc;
            ellipseParam.A = xRadius;
            ellipseParam.B = yRadius;
            ellipseParam.angle = 0;
            ellipseParam.style = 1;

            _current2dDoc.ksEllipse(ellipseParam);
        }

        /// <summary>
        /// Рисует линию между двумя точками.
        /// </summary>
        /// <param name="xStart">X-координата начальной точки.</param>
        /// <param name="yStart">Y-координата начальной точки.</param>
        /// <param name="xEnd">X-координата конечной точки.</param>
        /// <param name="yEnd">Y-координата конечной точки.</param>
        public void DrawLine(double xStart, double yStart,
            double xEnd, double yEnd)
        {
            if (_current2dDoc == null)
            {
                throw new InvalidOperationException(
                    "Нет активного 2D-эскиза." +
                    " Сначала вызови CreateSketchOnPlane().");
            }

            _current2dDoc.ksLineSeg(xStart, yStart, xEnd, yEnd, 1);
        }

        /// <summary>
        /// Рисует дугу по трем точкам.
        /// </summary>
        /// <param name="xc">X-координата центра дуги.</param>
        /// <param name="yc">Y-координата центра дуги.</param>
        /// <param name="rad">Радиус дуги.</param>
        /// <param name="startX">X-координата начальной точки.</param>
        /// <param name="startY">Y-координата начальной точки.</param>
        /// <param name="endX">X-координата конечной точки.</param>
        /// <param name="endY">Y-координата конечной точки.</param>
        /// <param name="direction">Направление отрисовки дуги.
        /// 1 - против часовой стрелки, -1 - по часовой стрелке.</param>
        public void DrawArcByThreePoints(double xc, double yc,
            double rad, double startX, double startY,
            double endX, double endY, short direction)
        {
            if (_current2dDoc == null)
            {
                throw new InvalidOperationException(
                    "Нет активного 2D-эскиза." +
                    " Сначала вызови CreateSketchOnPlane().");
            }

            _current2dDoc.ksArcByPoint(xc, yc, rad, startX, startY,
                endX, endY, direction, 1);
        }

        /// <summary>
        /// Завершает редактирование эскиза.
        /// </summary>
        /// <param name="sketch">Объект эскиза (ksEntity), который нужно
        /// завершить редактировать.</param>
        public void FinishSketch(object sketch)
        {
            if (sketch is not ksEntity sketchEntity)
            {
                throw new ArgumentException(
                    "Ожидался объект эскиза (ksEntity).",
                    nameof(sketch));
            }

            var sketchDef = (ksSketchDefinition)sketchEntity.GetDefinition();
            sketchDef.EndEdit();
            _current2dDoc = null;
        }

        /// <summary>
        /// Выполняет операцию выдавливания.
        /// </summary>
        /// <param name="sketch">Эскиз (ksEntity) для выдавливания.</param>
        /// <param name="height">Высота выдавливания.</param>
        /// <param name="direction">Направление выдавливания:
        /// true - прямое, false - обратное.</param>
        /// <param name="symmetric">Флаг симметричного выдавливания в обе
        /// стороны от плоскости эскиза.</param>
        public void Extrude(object sketch, double height,
            bool direction = true, bool symmetric = false)
        {
            if (_part == null)
            {
                throw new InvalidOperationException(
                    "Часть не инициализирована. Вызови CreateDocument3D().");
            }

            if (sketch is not ksEntity sketchEntity)
            {
                throw new ArgumentException(
                    "Ожидался объект эскиза (ksEntity).", nameof(sketch));
            }

            ksEntity extr = 
                _part.NewEntity((short)Obj3dType.o3d_baseExtrusion)
                          ?? throw new InvalidOperationException(
                          "Не удалось создать сущность o3d_baseExtrusion.");

            ksBaseExtrusionDefinition def =
                (ksBaseExtrusionDefinition)extr.GetDefinition();

            def.SetSketch(sketchEntity);

            ksExtrusionParam p = (ksExtrusionParam)def.ExtrusionParam();

            if (symmetric)
            {
                p.direction = (short)Direction_Type.dtBoth;
                p.typeNormal = (short)End_Type.etBlind;
                p.typeReverse = (short)End_Type.etBlind;
                p.depthNormal = height / 2;
                p.depthReverse = height / 2;
            }
            else if (!direction)
            {
                p.direction = (short)Direction_Type.dtReverse;
                p.typeReverse = (short)End_Type.etBlind;
                p.depthReverse = height;
            }
            else
            {
                p.direction = (short)Direction_Type.dtNormal;
                p.typeNormal = (short)End_Type.etBlind;
                p.depthNormal = height;
            }

            extr.Create();
        }

        /// <summary>
        /// Выполняет операцию вырезания насквозь.
        /// </summary>
        /// <param name="sketch">Эскиз (ksEntity) для вырезания.</param>
        public void Cut(object sketch)
        {
            ksEntity op = _part.NewEntity((short)Obj3dType.o3d_cutExtrusion);
            ksCutExtrusionDefinition def =
                (ksCutExtrusionDefinition)op.GetDefinition();
            def.SetSketch(sketch);
            def.directionType = (short)Direction_Type.dtBoth;
            def.SetSideParam(true, (short)End_Type.etThroughAll, 0, 0, true);
            def.SetSideParam(false,(short)End_Type.etThroughAll, 0, 0, true);
            op.Create();
        }

        /// <summary>
        /// Создаёт эскиз на плоскости, 
        /// смещённой от заданной базовой плоскости.
        /// </summary>
        /// <param name="plane">
        /// Базовая плоскость: "XOY", "XOZ", "YOZ".</param>
        /// <param name="offset">
        /// Величина смещения от базовой плоскости.</param>
        /// <param name="direction">
        /// Направление смещения:
        /// true - положительное, false - отрицательное.</param>
        /// <returns>Объект эскиза на смещенной плоскости.</returns>
        public object CreateSketchOnOffsetPlane(string plane, double offset,
            bool direction)
        {
            if (_part == null)
                throw new InvalidOperationException(
                    "Часть не инициализирована. Вызови CreateDocument3D().");

            short planeType = plane?.ToUpperInvariant() switch
            {
                "XOY" => (short)Obj3dType.o3d_planeXOY,
                "XOZ" => (short)Obj3dType.o3d_planeXOZ,
                "YOZ" => (short)Obj3dType.o3d_planeYOZ,
                _ => throw new ArgumentException(
                    "Неверное название базовой плоскости. " +
                    "Допустимые значения: \"XOY\", \"XOZ\", \"YOZ\"",
                    nameof(plane))
            };

            var basePlane = (ksEntity)_part.GetDefaultEntity(planeType)
                           ?? throw new InvalidOperationException(
                               $"Не удалось получить базовую плоскость.");

            var offsetPlaneEntity =
                (ksEntity)_part.NewEntity((short)Obj3dType.o3d_planeOffset)
                ?? throw new InvalidOperationException(
                    "Не удалось создать сущность o3d_planeOffset.");

            var offsetDef = 
                (ksPlaneOffsetDefinition)offsetPlaneEntity.GetDefinition();
            offsetDef.SetPlane(basePlane);
            offsetDef.direction = direction;
            offsetDef.offset = offset;

            offsetPlaneEntity.Create();
            var sketchEntity =
                (ksEntity)_part.NewEntity((short)Obj3dType.o3d_sketch)
                ?? throw new InvalidOperationException(
                    "Не удалось создать сущность o3d_sketch.");

            var sketchDef = (ksSketchDefinition)sketchEntity.GetDefinition();
            sketchDef.SetPlane(offsetPlaneEntity);
            sketchEntity.Create();

            _current2dDoc = (ksDocument2D)sketchDef.BeginEdit();

            return sketchEntity;
        }

        /// <summary>
        /// Создает элемент по сечениям.
        /// </summary>
        /// <param name="sections">Список сечений.</param>
        public void Loft(List<object> sections)
        {
            if (_part == null)
            {
                throw new InvalidOperationException(
                    "Часть не инициализирована. Вызови CreateDocument3D().");
            }

            if (sections == null || sections.Count < 2)
            {
                throw new ArgumentException(
                    "Для loft необходимо минимум два сечения.",
                    nameof(sections));
            }

            var entities = new ksEntity[sections.Count];

            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i] is ksEntity entity)
                {
                    entities[i] = entity;
                }
                else
                {
                    throw new ArgumentException(
                        $"Элемент с индексом {i}" +
                        $" не является объектом ksEntity.",
                        nameof(sections));
                }
            }

            ksEntity loftEntity = 
                (ksEntity)_part.NewEntity((short)Obj3dType.o3d_baseLoft)
                    ?? throw new InvalidOperationException(
                        "Не удалось создать сущность o3d_baseLoft.");

            ksBaseLoftDefinition loftDef = 
                (ksBaseLoftDefinition)loftEntity.GetDefinition();
            ksEntityCollection sectionsCollection = 
                (ksEntityCollection)loftDef.Sketchs();
            sectionsCollection.Clear();
            foreach (var section in entities)
            {
                sectionsCollection.Add(section);
            }
            loftEntity.Create();
        }

        /// <summary>
        /// Сохранение модели на диск.
        /// </summary>
        /// <param name="path">Полный путь к файлу для сохранения.</param>
        public void SaveAs(string path)
        {
            if (_doc3D == null)
            {
                throw new InvalidOperationException(
                    "Документ не создан. Вызови CreateDocument3D().");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Путь к файлу не задан.", nameof(path));
            }

            _doc3D.SaveAs(path);
        }

        /// <summary>
        /// Безопасно освобождает COM-объект.
        /// </summary>
        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null)
            {
                return;
            }

            if (Marshal.IsComObject(comObject))
            {
                Marshal.FinalReleaseComObject(comObject);
            }
        }

        /// <summary>
        /// Закрывает текущий 3D-документ КОМПАС-3D.
        /// </summary>
        public void CloseActiveDocument()
        {
            if (_doc3D == null)
            {
                return;
            }

            try
            {
                _doc3D.close();
            }
            catch
            {
                throw new ArgumentException(
                    $"Ошибка при закрытии документа");
            }
            finally
            {
                _current2dDoc = null;
                _part = null;
                _doc3D = null;
            }
        }
    }
}