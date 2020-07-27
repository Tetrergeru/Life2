using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Life2
{
    class World
    {
        public const int GeneLength = 64;
        
        public int time;

        private const int DayLength = 200;
        
        public bool start = false;

        /// <summary>
        /// Номер текущей итерации мира
        /// </summary>
        private int iteration;
        
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        private readonly Random generator;
        
        /// <summary>
        /// Использованное количество случайных чисел
        /// </summary>
        private int randomCount;
        
        /// <summary>
        /// Ширина мира
        /// </summary>
        private readonly int width;
        
        /// <summary>
        /// Высота мира
        /// </summary>
        private readonly int height;
        
        /// <summary>
        /// Карта мира
        /// </summary>
        public ObjectType[][] map;

        /// <summary>
        /// Карта ботов
        /// </summary>
        public Bot[][] bots;
        
        /// <summary>
        /// Возвращает ширину мира
        /// </summary>
        public int GetWidth => width;

        /// <summary>
        /// Возвращает высоту мира
        /// </summary>
        public int GetHeight => height;

        /// <summary>
        /// Возвращает текущую итерацию
        /// </summary>
        public int GetIteration => iteration;

        /// <summary>
        /// Генерирует новое случайное число в данном диапазоне
        /// </summary>
        /// <param name="first">Нижняя граница диапазона</param>
        /// <param name="second">Верхняя граница диапазона</param>
        /// <returns></returns>
        public int Random(int first, int second)
        {
            randomCount++;
            return generator.Next(first, second);
        }
        
        /// <summary>
        /// Создаёт новый экземпляр класса мир
        /// </summary>
        /// <param name="width">Ширина нового мира</param>
        /// <param name="height">Высота нового мира</param>
        /// <param name="startupBotCount">Количество ботов на момент создания</param>
        /// <param name="seed">Зерно генерации мира</param>
        public World(int width, int height, int startupBotCount, int seed)
        {
            time = 0;
            generator = new Random(seed);

            this.width = width;
            this.height = height;
            
            
            bots = Enumerable
                .Range(0, height)
                .Select(_ => Enumerable
                    .Range(0, height)
                    .Select(__ => (Bot)null)
                    .ToArray())
                .ToArray();
            
            map = File
                .ReadLines(@"3.map")
                .Select(line => line
                    .Select(ch => ch == '*' ? ObjectType.Wall : ObjectType.Empty)
                    .ToArray())
                .ToArray();

            for (var j = 0; j < startupBotCount; j++)
            {
                var gene = new int[GeneLength];
                for (var i = 0; i < GeneLength; i++)
                    gene[i] = 25;
                
                var x = Random(0, this.width - 1);
                var y = Random(0, this.height - 1);
                while (map[x][y] != ObjectType.Empty)
                {
                    x = Random(0, this.width - 1);
                    y = Random(0, this.height - 1);
                }
                bots[x][y] = new Bot(this, x, y, gene);
                map[x][y] = ObjectType.Bot;
            }
        }
        
        /// <summary>
        /// Совершает одну итерацию мира
        /// </summary>
        private void Live()
        {
            time += 1;
            if (time >= DayLength)
                time -= DayLength * 2;
            
            iteration++;
            for (var i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                    if (map[i][j] == ObjectType.Bot && bots[i][j].lastIteration != iteration)
                        bots[i][j].Live(iteration);
            }
            
        }

        public void Draw(Bitmap bit, int botSize, DrawType drawStyle)
        {
            var c = Color.White;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    switch (map[i][j])
                    {
                        case ObjectType.Bot:
                        {
                            var bot = bots[i][j];
                            if (bot != null)
                                c = bot.GetColor(drawStyle);
                            else
                                Console.WriteLine(map[i][j]);
                            break;
                        }
                        case ObjectType.Food:
                            c = Color.Gray;
                            break;
                        case ObjectType.Wall when drawStyle == DrawType.Energy:
                            c = Color.Orange;
                            break;
                        default:
                            c = Color.White;
                            break;
                    }

                    if (c == Color.White) continue;
                    for (var k = 0; k < botSize * botSize; k++)
                        bit.SetPixel(i * botSize + k / botSize, j * botSize + k % botSize, c);
                }
            }
        }
        public void DrawLens(Bitmap bit, int x, int y, DrawType drawStyle)
        {
            for (var i = 0; i < 40; i++)
            {
                for (var j = 0; j < 40; j++)
                {
                    var xx = i + x - 20;
                    var yy = j + y - 20;

                    if (yy == -1)
                        for (var k = 0; k < 10; k++)
                            bit.SetPixel(i * 5 + k % 5, j * 5 + k / 5 + 3, Color.Black);
                    else if (yy == height)
                        for (var k = 0; k < 10; k++)
                            bit.SetPixel(i * 5 + k % 5, j  * 5 + k / 5, Color.Black);
                    else if (yy >= 0 && yy < height)
                    {
                        if (xx >= width)
                            xx -= width;
                        else if (xx < 0)
                            xx += width;
                        if (map[xx][yy] == ObjectType.Bot)
                        {
                            var bot = bots[xx][yy];
                            if (bot != null)
                            {
                                var c = bot.GetColor(drawStyle);
                                for (int k = 0; k < 25; k++)
                                    bit.SetPixel(i * 5 + k / 5, j * 5 + k % 5, c);
                            }
                        }
                        else if (map[xx][yy] == ObjectType.Food)
                        {
                            for (int k = 0; k < 25; k++)
                                bit.SetPixel(i * 5 + k / 5, j * 5 + k % 5, Color.Gray);
                        }
                    }
                }
            }
        }
        
        public void RunThread()
        {
            while (true)
            while (start)
                Live();
        }
        
        public string GetInfo(int x,int y)
        {
            if (x < 0)
                x += width;
            x %= width;
            
            if (y > height || y < 0)
                return "";
            
            var s = new StringBuilder();
            s.Append($"x: {x},   y: {y},   Type: {map[x][y]}\r\n");
            s.Append($"S = {Bot.FotFromCoordinates(y, height):f3}, M = {Bot.MinFromCoordinates(y, height):f3}\r\n");
            
            if (map[x][y] != ObjectType.Bot) 
                return s.ToString();
            
            var bot = bots[x][y];
            if (bot == null) 
                return s.ToString();
            
            s.Append($"oE: {bot.orgEating}   sE: {bot.sunEating}   mE: {bot.minEating}\r\n");
            var rotationParameter = false;
            for (var i = 0; i < bot.gene.Length; i++)
            {
                if (rotationParameter)
                {
                    s.Append($"{i}:\t{bot.gene[i] % 8} ({bot.gene[i]})\r\n");
                    rotationParameter = false;
                }
                else
                {
                    if (bot.gene[i] >= 25 && bot.gene[i] <= 39)
                    {
                        s.Append($"{i}:\t{Bot.commands[bot.gene[i] - 25]}\r\n");
                        if (bot.gene[i] == 28)
                            rotationParameter = true;
                    }
                    else
                        s.Append($"{i}:\tGoto {(i + bot.gene[i]) % bot.gene.Length} ({bot.gene[i]})\r\n");
                }
            }
            return s.ToString();
        }
        public string GetRelation(int x1, int y1, int x2, int y2)
        {
            if (map[x1][y1] != ObjectType.Bot)
                return "";
            if (map[x2][y2] != ObjectType.Bot)
                return "";
            
            var bot1 = bots[x1][y1];
            var bot2 = bots[x2][y2];

            if (bot1 == null || bot2 == null)
                return "";
            
            return bot1.GetKinship(bot2).ToString();
        }
    }
}
