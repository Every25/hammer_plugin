using System;


namespace HammerPluginCore.Model
{
    /// <summary>
    /// Список параметров молотка.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Общая высота молотка (H). 
        /// </summary>
        HeightH,

        /// <summary>
        /// Длина головки молотка (L).
        /// </summary>
        LengthL,

        /// <summary>
        /// Диаметр бойка (D).
        /// </summary>
        FaceDiameterD,

        /// <summary>
        /// Ширина бойка (c).
        /// </summary>
        FaceWidthC,

        /// <summary>
        /// Ширина выступа перед бойком (a).
        /// </summary>
        NeckWidthA,

        /// <summary>
        /// Диаметр выступа перед бойком (b).
        /// </summary>
        NeckDiameterB,

        /// <summary>
        /// Ширина отверстия под рукоять (x1).
        /// </summary>
        HeadHoleX1,

        /// <summary>
        /// Ширина отверстия под рукоять (y1).
        /// </summary>
        HeadHoleY1,

        /// <summary>
        /// Ширина рукояти (x2).
        /// </summary>
        HandleWidthX2,

        /// <summary>
        /// Ширина рукояти (y2).
        /// </summary>
        HandleWidthY2,

        /// <summary>
        /// Длина носка (l).
        /// </summary>
        ClawLengthL,

        /// <summary>
        /// Ширина носка (w).
        /// </summary>
        ClawWidthW
    }
}
