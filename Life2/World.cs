using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life
{
    class World
    {
        #region "Local variables"
        public int time;
        public const int dayLength = 200;
        public bool start = false;
        public System.Threading.AutoResetEvent autoEvent;
        private static ConsoleColor[] colorsFood =
        {
            ConsoleColor.Green, ConsoleColor.DarkGreen, ConsoleColor.Cyan, ConsoleColor.DarkCyan,  ConsoleColor.Blue , ConsoleColor.DarkBlue
        };
        private static ConsoleColor[] colorsEnergy =
        {
            ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.DarkYellow, ConsoleColor.DarkMagenta, ConsoleColor.Magenta, ConsoleColor.Red ,ConsoleColor.Green
        };
        /// <summary>
        /// Номер текущей итерации мира
        /// </summary>
        private int iteration;
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        public Random generator;
        /// <summary>
        /// Использованное количество случайных чисел
        /// </summary>
        private int randomCount = 0;
        /// <summary>
        /// Ширина мира
        /// </summary>
        private int width;
        /// <summary>
        /// Высота мира
        /// </summary>
        private int height;
        /// <summary>
        /// Карта мира
        /// </summary>
        public int[,] map;
        static string[] MapObjects = new string[] {"Empty", "Food", "Bot", "Wall"};
        /// <summary>
        /// Карта ботов
        /// </summary>
        public Bot[,] bots;
        #endregion
        #region "Properties" 
        /// <summary>
        /// Возвращает ширину мира
        /// </summary>
        public int GetWidth { get { return this.width; } }
        /// <summary>
        /// Возвращает высоту мира
        /// </summary>
        public int GetHeight { get { return this.height; } }
        /// <summary>
        /// Возвращает текущую итерацию
        /// </summary>
        public int GetIteration { get { return this.iteration; } }
        #endregion
        /// <summary>
        /// Генерирует новое случайное число в данном диапазоне
        /// </summary>
        /// <param name="first">Нижняя граница диапазона</param>
        /// <param name="second">Верхняя граница диапазона</param>
        /// <returns></returns>
        public int Random(int first, int second)
        {
            this.randomCount++;
            return this.generator.Next(first, second);
        }
        /// <summary>
        /// Создаёт новый экземпляр класса мир
        /// </summary>
        /// <param name="wdth">Ширина нового мира</param>
        /// <param name="hght">Высота нового мира</param>
        /// <param name="nmb">Количество ботов на момент создания</param>
        /// <param name="seed">Зерно генерации мира</param>
        public World(int wdth, int hght, int nmb, int seed)
        {
            time = 0;

            this.autoEvent = new System.Threading.AutoResetEvent(false);
            this.generator = new Random(seed);

            this.width = wdth;
            this.height = hght;
            
            this.map = new int[wdth,hght];
            this.bots = new Bot[wdth, hght];
            using (var reader = new System.IO.StreamReader(@"D:\Coding\C#\Life2\2.map"))
            {
                for (int i = 0; i < Life.width; i++)
                {
                    var s = reader.ReadLine();
                    for (int j = 0; j < Life.height; j++)
                    {
                        if (s[j] == '*')
                        {
                            map[i, j] = 3;
                        } 
                    }
                }
            }

            for (int j = 0; j < nmb; j++)
            {
                var gene = new int[64];
                for (int i = 0; i < 64; i++)
                {
                    gene[i] = 25;
                }
                int x = this.Random(0, width - 1);
                int y = this.Random(0, height - 1);
                while (map[x, y] != 0 && map[x, y] == 3)
                {
                    x = this.Random(0, width - 1);
                    y = this.Random(0, height - 1);
                }
                bots[x, y] = new Bot(this, x, y, gene);
                map[x, y] = 2;
            }
        }
        /// <summary>
        /// Совершает одну итерацию мира
        /// </summary>
        public void Live()
        {
            time++;
            if (time >= dayLength)
            {
                time -= dayLength * 2;
            }
            //Console.WriteLine($"Live: {System.Threading.Thread.CurrentThread.Name}");
            iteration++;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (this.map[i, j] == 2 && this.bots[i, j].lastIteration != this.iteration)
                        this.bots[i, j].Live(this.iteration);
                }
            }
            
        }
        /// <summary>
        /// Выводит мир в консоль
        /// </summary>
        public void Write()
        {
            
            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            int count = 0;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (this.map[i, j] != 2)
                        Console.Write(Life.txt[this.map[i, j]]);
                    else
                    {
                        Console.ForegroundColor = bots[i,j].cl;
                        Console.Write("o");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    if (this.map[i, j] == 2)
                        count++;
                }
                Console.Write("|");
                for (int i = 0; i < width; i++)
                {
                    if (this.map[i, j] != 2)
                        Console.Write(Life.txt[this.map[i, j]]);
                    else
                    {
                        if (bots[i, j].energy > 99)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ForegroundColor = World.colorsEnergy[(int)Math.Floor(bots[i, j].energy / 17.0)];
                        }
                        Console.Write("o");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    if (this.map[i, j] == 2)
                        count++;
                }
                Console.WriteLine();
            }
            //Бывший WriteCXolorLine()
            foreach (var c in World.colorsFood)
            {
                Console.ForegroundColor = c;
                Console.Write("0");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = 0; i < Console.BufferWidth - 14; i++)
                Console.Write("-");
            foreach (var c in World.colorsEnergy)
            {
                Console.ForegroundColor = c;
                Console.Write("0");
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            //Информация
            Console.WriteLine($"Step: {this.iteration};                                Nuber of bots: {count};");
            
        }
        public void Draw(System.Drawing.Bitmap bit, int botSize, bool drawStyle)
        {
            System.Drawing.Color c = System.Drawing.Color.White;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (this.map[i, j] == 2)
                    {
                        var bot = this.bots[i, j];
                        if (bot != null)
                        {
                            c = bot.GetColor(drawStyle);
                        }
                        else
                        {
                            Console.WriteLine(this.map[i, j]);
                        }
                    }
                    else if (this.map[i, j] == 1)
                    {
                        c = System.Drawing.Color.Gray;
                    }
                    else if (this.map[i, j] == 3 && !drawStyle)
                    {
                        c = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        c = System.Drawing.Color.White;
                    }
                    if (c != System.Drawing.Color.White)
                    {
                        for (int k = 0; k < botSize * botSize; k++)
                        {
                            bit.SetPixel(i * botSize + k / botSize, j * botSize + k % botSize, c);
                        }
                    }
                }
            }
        }
        public void DrawLens(System.Drawing.Bitmap bit,int x, int y)
        {
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    int xx = i + x - 20;
                    int yy = j + y - 20;

                    if (yy == -1)
                    {
                        for (int k = 0; k < 10; k++)
                        {
                            bit.SetPixel(i * 5 + k % 5, j * 5 + k / 5 + 3, Color.Black);
                        }
                    }
                    else if (yy == height)
                    {
                        for (int k = 0; k < 10; k++)
                        {
                            bit.SetPixel(i * 5 + k % 5, j  * 5 + k / 5, Color.Black);
                        }
                    }
                    else if (yy >= 0 && yy < height)
                    {
                        if (xx >= this.width)
                        {
                            xx -= width;
                        }
                        else if (xx < 0)
                        {
                            xx += width;
                        }
                        if (map[xx, yy] == 2)
                        {
                            var bot = bots[xx, yy];
                            if (bot != null)
                            {
                                var c = bot.GetColor(true);
                                for (int k = 0; k < 25; k++)
                                {
                                    bit.SetPixel(i * 5 + k / 5, j * 5 + k % 5, c);
                                }
                            }
                        }
                        else if (map[xx, yy] == 1)
                        {
                            for (int k = 0; k < 25; k++)
                            {
                                bit.SetPixel(i * 5 + k / 5, j * 5 + k % 5, Color.Gray);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Выводит в консоль ген бота по координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void WriteGene(int x, int y)
        {
            this.bots[x, y].WriteGene();
        }
        public void RunThr()
        {
            while (true)
            {
                while (start)
                {
                    this.Live();
                    //autoEvent.WaitOne();
                }
            }
        }
        public string GetInfo(int x,int y)
        {
            if (x >= width)
            {
                x -= width;
            }
            else if (x < 0)
            {
                x += width;
            }
            if (y > height || y < 0)
            {
                return "VOID";
            }
            var s = new StringBuilder();
            s.Append($"x: {x},   y: {y},   Type: {World.MapObjects[map[x,y]]}\r\n");
            s.Append($"S = {Bot.FotFromCoord(y, height):f3}, M = {Bot.MinFromCoord(y, height):f3}\r\n");
            if (map[x, y] == 2)
            {
                var bot = bots[x, y];
                if (bot != null)
                {
                    s.Append($"oE: {bot.orgEating}   sE: {bot.sunEating}   mE: {bot.minEating}\r\n");
                    bool rot = false;
                    for (int i = 0; i < bot.gene.Length; i++)
                    {
                        if (rot)
                        {
                            s.Append($"{i}:\t{bot.gene[i] % 8} ({bot.gene[i]})\r\n");
                            rot = false;
                        }
                        else
                        {
                            if (bot.gene[i] >= 25 && bot.gene[i] <= 39)
                            {
                                s.Append($"{i}:\t{Bot.commands[bot.gene[i] - 25]}\r\n");
                                if (bot.gene[i] == 28)
                                {
                                    rot = true;
                                }
                            }
                            else
                            {
                                s.Append($"{i}:\tGoto {(i + bot.gene[i]) % bot.gene.Length} ({bot.gene[i]})\r\n");
                            }
                        }
                    }
                }
            }
            return s.ToString();
        }
        public string GetRelation(int x1, int y1, int x2, int y2)
        {
            if (map[x1, y1] == 2)
            {
                var bot = bots[x1, y1];
                if (bot != null)
                {
                    if (map[x2, y2] == 2)
                    {
                        var bot1 = bots[x2, y2];
                        if (bot1 != null)
                        {
                            return bot.GetKin(bot1).ToString();
                        }
                    }
                }
            }
            return "NULL";
        }
    }
}
