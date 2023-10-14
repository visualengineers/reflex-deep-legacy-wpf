using System;
using System.Windows.Media;

namespace PhysicsSimulation.Utilities
{
    /// <summary>
    /// Konvertiert Farben zwischen den verschiedenen Farbmodellen.
    /// </summary>
    public static class ColorConversion
    {
        #region Felder
        /// <summary> Referenzweiß D65. </summary>
        private static readonly double[] Xn = { 0.950456, 1, 1.088754 };
        #endregion
        //---------------------------------------------------------------------
        #region RGB-Konvertierungen
        /// <summary> Konvertiert eine sRGB-Farbe zu einer HSV-Farbe. </summary>
        /// <param name="color">Die sRGB-Farbe.</param>
        ///  <returns> Ein 3 dimensionaler Vektor mit den HSV-Farbkomponenten mit h in [0,360], s in [0,1] und v in [0,1]. </returns>
        /// <exception cref="ArgumentException"> Es wurd null übergeben und somit kann keine Konvertierung durchgeführt werden. </exception>
        public static double[] RGB2HSV(Color color)
        {
            if (color == null) 
                throw new ArgumentException();
            //-----------------------------------------------------------------
            // RGB in [0,255] -> RGB in [0,1]:
            var r = color.R / 255d;
            var g = color.G / 255d;
            var b = color.B / 255d;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            double h = 0;
            if (Math.Abs(max - r) < Double.Epsilon)
            {
                if (g > b)
                    h = 60 * (g - b) / (max - min);
                else if (g < b)
                    h = 60 * (g - b) / (max - min) + 360;
            }
            else if (Math.Abs(max - g) < Double.Epsilon)
                h = 60 * (b - r) / (max - min) + 120;
            else if (Math.Abs(max - b) < Double.Epsilon)
                h = 60 * (r - g) / (max - min) + 240;

            var s = (Math.Abs(max) < Double.Epsilon) ? 0 : 1 - min / max;

            return new[] { h, s, max };
        }
        //---------------------------------------------------------------------
        /// <summary> Konvertiert eine sRGB-Farbe zu einer CIE-Lab-Farbe. </summary>
        /// <param name="color">Die sRGB-Farbe.</param>
        /// <returns>3D-Vektor mit den Lab-Komponenten der Farbe.</returns>
        /// <exception cref="ArgumentException">Es wurde null übergeben und somit kann keine Konvertierung durchgeführt werden. </exception>
        /// <remarks> Die sRGB-Werte müssen aus dem Intervall [0,255] sein. Der L*-Wert ist aus dem Intervall [0,100] und die Werte für a* und b* sind  ungefähr aus dem Intervall [-110,110].
        /// <para> Die Transformation basiert auf ITU-R Recommendation BT.709 und verwendet den D65 Referenzweißpunkt. Der algorithmische Fehler für die Transformation sRGB -> L*a*b* -> sRGB ist in der Größenordnung 
        /// 1e-5. Durch Rundung auf ganzzahlige Werte kann der Fehler der Ausgabe jedoch 1 betragen. </para>
        /// <para> Quelle: <a href="http://ai.stanford.edu/~ruzon/software/rgblab.html"> [URL]http://ai.stanford.edu/~ruzon/software/rgblab.html[/URL] </a></para>
        /// </remarks>
        /// <seealso cref="Lab2RGB"/>
        public static double[] RGB2Lab(Color color)
        {
            if (color == null) 
                throw new ArgumentException();
            //-----------------------------------------------------------------
            // RGB in [0,255] -> RGB in [0,1]:
            double[] rgb =
            {
                color.R / 255d,
                color.G / 255d,
                color.B / 255d
            };

            // RGB -> XYZ. Dabei selben Referenzweißpunkt D65 verwenden:
            var xyz = new double[3];
            double[,] cXr =
            {
                { 0.412453, 0.357580, 0.180423 },
                { 0.212671, 0.715160, 0.072169 },
                { 0.019334, 0.119193, 0.950227 }
            };

            xyz[0] = cXr[0, 0] * rgb[0] + cXr[0, 1] * rgb[1] + cXr[0, 2] * rgb[2];
            xyz[1] = cXr[1, 0] * rgb[0] + cXr[1, 1] * rgb[1] + cXr[1, 2] * rgb[2];
            xyz[2] = cXr[2, 0] * rgb[0] + cXr[2, 1] * rgb[1] + cXr[2, 2] * rgb[2];

            // Auf Referenzpunkt normieren:
            var xyzNorm = new double[3];
            for (var i = 0; i < xyzNorm.Length; i++)
                xyzNorm[i] = xyz[i] / Xn[i];

            // Schwellenwert:
            const double T = 0.008856;

            var xt = xyzNorm[0] > T;
            var yt = xyzNorm[1] > T;
            var zt = xyzNorm[2] > T;
            var y3 = Math.Pow(xyzNorm[1], 1d / 3d);

            // Nichtlinear Projektion von XYZ -> Lab:
            var fX = xt ? Math.Pow(xyzNorm[0], 1d / 3d) : 7.787 * xyzNorm[0] + 16d / 116d;
            var fY = yt ? y3 : 7.787 * xyzNorm[1] + 16d / 116d;
            var fZ = zt ? Math.Pow(xyzNorm[2], 1d / 3d) : 7.787 * xyzNorm[1] + 16d / 116d;

            var lab = new double[3];
            lab[0] = yt ? 116d * y3 - 16d : 903.3 * xyzNorm[1];
            lab[1] = 500d * (fX - fY);
            lab[2] = 200d * (fY - fZ);

            return lab;
        }
        #endregion
        //---------------------------------------------------------------------
        #region HSV-Konvertierungen
        /// <summary>Konvertiert die HSV-Farbe zu einer sRGB-Farbe. </summary>
        /// <param name="hsv">Ein 3 dimensionaler Vektor mit den HSV-Farbkomponenten mit h in [0,360], s in [0,1] und v in [0,1]. </param>
        /// <returns>Die sRGB-Farbe.</returns>
        /// <exception cref="ArgumentNullException"> <paramref name="hsv"/> ist <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="hsv"/> hat eine andere Dimension als 3 oder die Werte des Vektors liegen außerhalb des gültigen Intervalls. </exception>
        /// <remarks>
        /// Durch Variation des Hue-Anteils von 0 nach 360 ändert sich die resultierende Farbe von Rot nach Gelb, Grün, Cyan, Blau und Magenta wieder zurück nach Rot.<br />
        /// Wenn die Sättigung 0 ist so ist die Farbe ungestättigt, d.h. die Farbe ist eine Graustufe. Ist die Sättigung 1 so ist die Farbe voll gesättigt was bedeutet dass keine Weißkomponente vorhanden
        /// ist.<br />
        /// Der Wert der Helligkeit gibt die Helligkeit an ;).
        /// <para> Quelle: <a href="http://www.codeproject.com/KB/recipes/colorspace1.aspx"> Alvy Ray Smith, Color Gamut Transform Pairs, SIGGRAPH '78. </a> </para>
        /// </remarks>
        public static Color HSV2RGB(double[] hsv)
        {
            if (hsv == null) 
                throw new ArgumentNullException();

            if (hsv.Length != 3) 
                throw new ArgumentException();

            if (hsv[0] < 0 || hsv[0] > 360) 
                throw new ArgumentException();
            if (hsv[1] < 0 || hsv[1] > 1) 
                throw new ArgumentException();
            if (hsv[2] < 0 || hsv[2] > 1) 
                throw new ArgumentException();
            //-----------------------------------------------------------------
            double[] rgb;

            // Wenn die Sättigung 0 ist -> Graustufen:
            if (Math.Abs(hsv[1]) < Double.Epsilon)
                rgb = new[] { hsv[2], hsv[2], hsv[2] };
            else
            {
                // Das Farbenrad ist in 6 Sektoren geteilt. Ermitteln in 
                // welchen Sektor wir uns befinden:
                var sectorPos = hsv[0] / 60d;
                var sectorNumber = (int)Math.Floor(sectorPos);
                var fractionalSector = sectorPos - sectorNumber;

                // Die Werte der 3-Achsen der Farbe berechnen:
                var p = hsv[2] * (1 - hsv[1]);
                var q = hsv[2] * (1 - hsv[1] * fractionalSector);
                var t = hsv[2] * (1 - hsv[1] * (1 - fractionalSector));

                // Fallunterscheidung je nach Sektor -> die Farbwerte für
                // RGB zuweisen:
                switch (sectorNumber)
                {
                    case 0:
                        rgb = new[] { hsv[2], t, p };
                        break;
                    case 1:
                        rgb = new[] { q, hsv[2], p };
                        break;
                    case 2:
                        rgb = new[] { p, hsv[2], t };
                        break;
                    case 3:
                        rgb = new[] { p, q, hsv[2] };
                        break;
                    case 4:
                        rgb = new[] { t, p, hsv[2] };
                        break;
                    default:
                        rgb = new[] { hsv[2], p, q };
                        break;
                }
            }

            // Skalieren der RGB-Anteile von [0,1] nach [0,255]:
            for (var i = 0; i < rgb.Length; i++)
                rgb[i] *= 255;

            return Color.FromArgb(
                255,
                Convert.ToByte((int) rgb[0]),
                Convert.ToByte((int) rgb[1]),
                Convert.ToByte((int) rgb[2]));
        }
        #endregion
        //---------------------------------------------------------------------
        #region CIE-Lab-Konvertierungen
        /// <summary>
        /// Konvertiert eine CIE-Lab-Farbe zu einer sRGB-Farbe.
        /// </summary>
        /// <param name="lab"></param>
        /// <returns>Die sRGB-Farbe.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lab"/> ist <c>null</c>. </exception>
        /// <exception cref="ArgumentException"><paramref name="lab"/> hat eine ungültige Dimension. </exception>
        /// <remarks>
        /// Der Wert für L* muss im Intervall [0,100] und die Werte für a* und b* im groben Intervall [-110,110] sein. Die Ausgabewerte für sRGB sind im Intervall [0,255]. Kann eine L*a*b*-Farbe nicht im 
        /// RGB-Farbraum dargestellt werden so werden die Werte in das Intervall [0,255] gezwängt. Es wird dabei kein Fehler ausgelöst.
        /// <para>
        /// Die Transformation basiert auf ITU-R Recommendation BT.709 und verwendet den D65 Referenzweißpunkt. Der algorithmische Fehler für die Transformation RGB -> L*a*b* -> RGB ist in der Größenordnung 
        /// 1e-5. Durch Rundung auf ganzzahlige Werte kann der Fehler der Ausgabe jedoch 1 betragen.
        /// </para>
        /// <para> Quelle: <a href="http://ai.stanford.edu/~ruzon/software/rgblab.html"> [URL]http://ai.stanford.edu/~ruzon/software/rgblab.html[/URL] </a> </para>
        /// </remarks>
        /// <seealso cref="RGB2Lab"/>
        public static Color Lab2RGB(double[] lab)
        {
            if (lab == null)
                throw new ArgumentNullException();

            if (lab.Length != 3)
                throw new ArgumentException();
            //-----------------------------------------------------------------
            var xyz = new double[3];
            const double t1 = 0.008856;
            const double t2 = 0.206893;

            // Y berechnen:
            var fY = Math.Pow((lab[0] + 16d) / 116d, 3);
            var yt = fY > t1;
            fY = yt ? fY : lab[0] / 903.3;
            xyz[1] = fY;

            // fY leicht modifizieren für die weiteren Berechnungen:
            fY = yt ? Math.Pow(fY, 1d / 3d) : 7.787 * fY + 16d / 116d;

            // X berechnen:
            var fX = lab[1] / 500d + fY;
            xyz[0] = fX > t2 ? Math.Pow(fX, 3) : (fX - 16d / 116d) / 7.787;

            // Z berechnen:
            var fZ = fY - lab[2] / 200d;
            xyz[2] = fZ > t2 ? Math.Pow(fZ, 3) : (fZ - 16d / 116d) / 7.787;

            // Auf Referenzweiß D65 normalisieren:
            for (var i = 0; i < xyz.Length; i++)
                xyz[i] *= Xn[i];

            // XYZ -> RGB. Dabei selben Referenzweißpunkt D65 verwenden:
            var rgb = new double[3];
            double[,] cRx =
            {
                { 3.240479, -1.537150, -0.498535 },
                { -0.969256, 1.875992, 0.041556 },
                { 0.055648, -0.204043, 1.057311 }
            };

            rgb[0] = cRx[0, 0] * xyz[0] + cRx[0, 1] * xyz[1] + cRx[0, 2] * xyz[2];
            rgb[1] = cRx[1, 0] * xyz[0] + cRx[1, 1] * xyz[1] + cRx[1, 2] * xyz[2];
            rgb[2] = cRx[2, 0] * xyz[0] + cRx[2, 1] * xyz[1] + cRx[2, 2] * xyz[2];

            // RGB in [0,1] -> RGB in [0,255]. Dabei auch die Wert in dieses
            // Intervall zwingen:
            for (var i = 0; i < rgb.Length; i++)
            {
                rgb[i] *= 255;

                if (rgb[i] < 0) rgb[i] = 0;
                if (rgb[i] > 255) rgb[i] = 255;
            }

            return Color.FromArgb(
                255,
                Convert.ToByte((int)rgb[0]),
                Convert.ToByte((int)rgb[1]),
                Convert.ToByte((int)rgb[2]));
        }
        #endregion
        //---------------------------------------------------------------------
        #region Farbabstände
        /// <summary>
        /// Berechnet den quadratischen perzeptuellen Abstand zwischen den 
        /// beiden Farben im CIE-L*a*b* Farbraum gemäß euklidischer Norm.
        /// </summary>
        /// <param name="lab1">Die Referenzfarbe.</param>
        /// <param name="lab2">Die Vergleichsfarbe.</param>
        /// <returns>Den quadratischen perzeptuellen Abstand der Farben.</returns>
        public static double ColorDistance2(double[] lab1, double[] lab2)
        {
            var dist2 =
                (lab1[0] - lab2[0]) * (lab1[0] - lab2[0]) +
                (lab1[1] - lab2[1]) * (lab1[1] - lab2[1]) +
                (lab1[2] - lab2[2]) * (lab1[2] - lab2[2]);

            return dist2;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Berechnet den perzeptuellen Abstand zwischen den beiden Farben im
        /// CIE-L*a*b* Farbraum gemäß euklidischer Norm.
        /// </summary>
        /// <param name="lab1">Die Referenzfarbe.</param>
        /// <param name="lab2">Die Vergleichsfarbe.</param>
        /// <returns>Den perzeptuellen Abstand der Farben.</returns>
        public static double ColorDistance(double[] lab1, double[] lab2)
        {
            return Math.Sqrt(ColorDistance2(lab1, lab2));
        }
        #endregion
    }
}
