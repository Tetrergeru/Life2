using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Life
{
    static class Life
    {
        public const int botSize = 2;
        public static int width = 200;
        public static int height = 200;
        public static int nmb = 10;
        public static LifeForm fm;

        public static string[] txt = { " ", ".", "o" };
        /// <summary>
        /// Возвращает ширину матрицы (количество столбцов)
        /// </summary>
        public static int GetWidth<T>(this T[,] matrix)
        {
            return matrix.GetLength(0);
        }
        /// <summary>
        /// Возвращает высоту матрицы (количество строк)
        /// </summary>
        public static int GetHeight<T>(this T[,] matrix)
        {
            return matrix.GetLength(1);
        }

        static void Main(string[] args)
        {
            fm = new LifeForm(width,height);
            Application.Run(fm);
        }
    }
}
