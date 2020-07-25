using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life
{
    class Bot
    {
        public static string[] commands = new string[]
        {
            "Photosynthesis", "Chemosyntesis", "Look", "Rotate", "Move", "Attack", "Eat", "Clone", "Heal", "Share", "Suicide", "SetBlock", "BreakBlock", "CheckEnergy","Mine"
        };
        public int digs = 0;
        private int l = 0;
        public int lastIteration = -1;
        public int kills = 0;
        public int age = 0;
        public double minEating = 0;
        public double sunEating = 0;
        public double orgEating = 0;
        public ConsoleColor cl = ConsoleColor.DarkGray;
        /// <summary>
        /// Мир, которому принадлежит бот
        /// </summary>
        private World wrld;
        /// <summary>
        /// Запас энергии бота
        /// </summary>
        public double energy;
        /// <summary>
        /// Запас минералов бота
        /// </summary>
        public double minerals;
        /// <summary>
        /// Здоровье бота
        /// </summary>
        public int health;
        /// <summary>
        /// Координаты бота
        /// </summary>
        public (int x, int y) coord;
        /// <summary>
        /// Направление, в котором смотрит бот (ноль - вправо, против часовой клетки по сорок пять градусов)
        /// </summary>
        private int direction;
        /// <summary>
        /// Генотип бота
        /// </summary>
        public int[] gene;
        public Bot(World w, int en, int min, int hlth, int x, int y, int dir, int[] gene)
        {
            this.wrld = w;
            this.energy = en;
            this.minerals = min;
            this.health = hlth;
            this.coord = (x, y);
            this.gene = gene;
            this.direction = dir;
        }
        public Bot(World w, int x, int y, int[] gene)
        {
            this.wrld = w;
            this.energy = 10;
            this.minerals = 0;
            this.health = 10;
            this.coord = (x, y);
            this.gene = gene;
            this.direction = this.wrld.Random(0,7);
        }
        public void Live(int li)
        {
            lastIteration = li;
            l = 0;
            this.age++;
            minEating = 0;
            sunEating = 0;
            orgEating = 0; ;
            int width = wrld.GetWidth;
            int height = wrld.GetHeight;

            int steps = 0;

            this.energy -= 5;


            if (this.energy > 20 && this.wrld.Random(0, 2) == 0)
            {
                this.Clone();
            }
            else
            {
                while (steps < 15)
                {
                    switch (this.gene[l])
                    {
                        //Фотосинтез
                        case 25:
                            {
                                //if (this.coord.y < height / 3 * 2)
                                {
                                    /*
                                    sunEating += 1.2;//(2 - this.coord.y / (height / 3.0)) * (Math.Cos(Math.PI * wrld.time / World.dayLength) + 1);
                                    this.energy += 1.2;//(2 - this.coord.y / (height / 3.0)) * (Math.Cos(Math.PI * wrld.time / World.dayLength) + 1);
                                    //*/

                                    this.energy += FotFromCoord(this.coord.y, height);
                                    sunEating += FotFromCoord(this.coord.y, height);
                                }
                                l++;
                                steps++;
                                break;
                            }
                        //Преобразует минералы в энергию
                        case 26:
                            {
                                if (this.minerals > 2)
                                {
                                    this.minerals -= 2;
                                    minEating += 3;
                                    this.energy += 3;
                                }
                                l++;
                                steps++;
                                break;
                            }
                        //Перемещает указатель, в зависимости от того, на что смотрит бот
                        case 27:
                            {
                                (int x, int y) = LookAt();
                                if (IsBlocked(x, y))
                                {
                                    l += 6;
                                }
                                else if (this.wrld.map[x, y] == 0)
                                {
                                    l++;
                                }
                                else if (this.wrld.map[x, y] == 1)
                                {
                                    l += 2;
                                }
                                else if (IsKin(this.wrld.bots[x, y]))
                                {
                                    l += 3;
                                }
                                else if (this.wrld.bots[x, y].kills > 0)
                                {
                                    l += 4;
                                }
                                else
                                {
                                    l += 5;
                                }
                                energy--;
                                steps++;
                                break;
                            }
                        //Поворачивает бота
                        case 28:
                            {
                                l++;
                                l %= 64;

                                this.direction += this.gene[l];
                                this.direction %= 8;

                                energy--;
                                l++;
                                steps++;
                                break;
                            }
                        //Двигает бота в том напрвлении, куда он смотрит
                        case 29:
                            {
                                energy -= 5;
                                (int x, int y) = LookAt();
                                if (!IsBlocked(x, y) && this.wrld.map[x, y] != 2)
                                {
                                    if (this.wrld.map[x, y] == 1)
                                    {
                                        orgEating += 20;
                                        this.energy += 20;
                                    }

                                    this.wrld.map[x, y] = 2;
                                    this.wrld.map[this.coord.x, this.coord.y] = 0;

                                    this.wrld.bots[x, y] = this;
                                    this.wrld.bots[this.coord.x, this.coord.y] = null;

                                    this.coord = (x, y);
                                }
                                l++;
                                steps += 5;
                                break;
                            }
                        //Атакует бота, в направлении взгляда
                        case 30:
                            {
                                (int x, int y) = LookAt();
                                this.energy -= 5;
                                if (!IsBlocked(x, y) && this.wrld.map[x, y] == 2)
                                {
                                    this.wrld.bots[x, y].Attack(20);
                                    this.energy += 10;
                                    if (this.wrld.map[x, y] == 1)
                                    {
                                        this.kills++;
                                    }
                                }
                                l++;
                                steps += 15;
                                break;
                            }
                        //Съедает еду напротив бота
                        case 31:
                            {
                                (int x, int y) = LookAt();
                                if (!IsBlocked(x, y) && this.wrld.map[x, y] == 1)
                                {
                                    this.wrld.map[x, y] = 0;
                                    this.energy += 20;
                                    orgEating += 20;
                                }
                                l++;
                                steps += 15;
                                break;
                            }
                        //Бот делится
                        case 32:
                            {
                                this.Clone(this.direction);
                                steps += 15;
                                l++;
                                break;
                            }
                        //Бот лечится
                        case 33:
                            {
                                if (this.energy > 5)
                                {
                                    this.energy -= 5;
                                    this.health += 5;
                                }
                                l++;
                                steps += 15;
                                break;
                            }
                        //Передаёт часть энергии другому боту
                        case 34:
                            {
                                (int x, int y) = LookAt();
                                if (!IsBlocked(x, y) && this.wrld.map[x, y] == 2)
                                {
                                    this.wrld.bots[x, y].energy += 5;
                                    this.energy -= 5;
                                }
                                l++;
                                steps += 15;
                                break;
                            }
                        //Убивает себя
                        case 35:
                            {
                                this.wrld.map[this.coord.x, this.coord.y] = 1;
                                this.wrld.bots[this.coord.x, this.coord.y] = null;
                                steps += 15;
                                break;
                            }
                        //Разрушает блок стены
                        case 36:
                            {
                                if (this.energy > 5)
                                {
                                    (int x, int y) = LookAt();
                                    if (y >= 0 && y < height && this.wrld.map[x, y] == 3)
                                    {
                                        this.wrld.map[x, y] = 0;
                                        this.minerals += 10;
                                        digs++;
                                    }
                                    this.energy -= 2;
                                }
                                l++;
                                steps += 10;
                                break;
                            }
                        //Ставит блок стены
                        case 37:
                            {
                                if (this.minerals >= 10)
                                {
                                    (int x, int y) = LookAt();
                                    if (y >= 0 && y < height && this.wrld.map[x, y] == 0)
                                    {
                                        this.wrld.map[x, y] = 3;
                                        this.minerals -= 10;
                                    }
                                }
                                l ++;
                                steps += 5;
                                break;
                            }
                        //Проверяет энергию
                        case 38:
                            {
                                l++;
                                l %= gene.Length;
                                if (1.0 * (this.energy + 200) / 200 > (gene[l] + 1) / 32.0)
                                {
                                    l++;
                                }
                                else
                                {
                                    l += 2;
                                }
                                steps++;
                                break;
                            }
                        //Добывает минералы
                        case 39:
                            {
                                //this.minerals += 1.2;// + this.coord.y / (height / 3.0);
                                this.minerals += MinFromCoord(this.coord.y,height);
                                if (this.minerals > 100)
                                {
                                    this.minerals = 100;
                                }
                                l++;
                                steps++;
                                break;
                            }
                        default:
                            {
                                l += this.gene[l];
                                steps++;
                                break;
                            }
                    }

                    l = l % gene.Length;

                    if (this.energy > 200)
                    {
                        this.energy = 200;
                    }
                    if (this.health > 99)
                    {
                        this.health = 99;
                    }

                    if (this.health <= 0 || this.energy <= 0)
                    {
                        steps += 15;
                        this.wrld.map[this.coord.x, this.coord.y] = 1;
                        this.wrld.bots[this.coord.x, this.coord.y] = null;
                    }
                }
            }

            if (this.energy > 200)
            {
                this.energy = 200;
            }
            if (this.health > 99)
            {
                this.health = 99;
            }
            if (this.health <= 0 || this.energy <= 0 || this.age > 50)
            {
                this.wrld.map[this.coord.x, this.coord.y] = 1;
                this.wrld.bots[this.coord.x, this.coord.y] = null;
            }

            /// <summary>
            ///double border = 1.7;
            ///if (orgEating > sunEating && orgEating > minEating)
            ///{
            ///if (this.kills > 0)
            ///this.cl = ConsoleColor.DarkRed;
            ///else
            ///this.cl = ConsoleColor.Yellow;
            ///}
            ///else if (this.kills > 0)
            ///{
            ///this.cl = ConsoleColor.Red;
            ///}
            ///else if (sunEating > minEating)
            ///{
            ///if (minEating == 0)
            ///{
            ///this.cl = ConsoleColor.Green;
            ///}
            ///else if (sunEating / minEating * 1.0 > border)
            ///{
            ///this.cl = ConsoleColor.DarkGreen;
            ///}
            ///else
            ///{
            ///this.cl = ConsoleColor.Cyan;
            ///}
            ///}
            ///else
            ///{
            ///if (sunEating == 0)
            ///{
            ///this.cl = ConsoleColor.DarkBlue; ;
            ///}
            ///else if (minEating / sunEating * 1.0 > border)
            ///{
            ///this.cl = ConsoleColor.Blue;
            ///}
            ///else
            ///{
            ///this.cl = ConsoleColor.DarkCyan;
            ///}
            ///}
            /// </summary>
        }

        public bool IsKin(Bot b)
        {
            return GetKin(b) < 10;
        }
        public int GetKin(Bot b)
        {
            int k = 0;
            for (int i = 0; i < 64; i++)
            {
                if (this.gene[i] != b.gene[i])
                {
                    k++;
                }
            }
            return k;
        }
        private (int x, int y) LookAt(int d)
        {
            int width = this.wrld.GetWidth;
            int x = this.coord.x + (int)Math.Round(Math.Cos(Math.PI * 0.25 * d));
            int y = this.coord.y - (int)Math.Round(Math.Sin(Math.PI * 0.25 * d));

            if (x < 0)
                x += width;
            else if (x > width - 1)
                x -= width;

            return (x, y);
        }
        private (int x, int y) LookAt()
        {
            return LookAt(this.direction);
        }
        private void Clone()
        {
            int width = this.wrld.GetWidth;
            int height = this.wrld.GetHeight;
            
            for (int i = 0; i < 8; i++)
            {
                (int x, int y) = LookAt(i);
                if (y >= 0 && y < height && (this.wrld.map[x, y] == 0))
                {
                    this.energy -= 10;
                    var g = new int[64];
                    for (int k = 0; k < 64; k++)
                    {
                        g[k] = this.gene[k];
                    }
                    if (this.wrld.Random(0, 2) == 0)
                    {
                        g[this.wrld.Random(0, 63)] = this.wrld.Random(0, 63);
                    }
                    this.wrld.map[x, y] = 2;
                    this.wrld.bots[x, y] = new Bot(this.wrld, x, y, g);
                    return;
                }
            }
            //this.wrld.map[this.coord.x, this.coord.y] = 1;
            //this.wrld.bots[this.coord.x, this.coord.y] = null;
        }
        private void Clone(int i)
        {
            int width = this.wrld.GetWidth;
            int height = this.wrld.GetHeight;
            
            (int x, int y) = LookAt(i);
            if (y >= 0 && y < height && (this.wrld.map[x, y] == 0))
            {
                this.energy -= 10;
                var g = new int[64];
                for (int k = 0; k < 64; k++)
                {
                    g[k] = this.gene[k];
                }
                if (this.wrld.Random(0, 2) == 0)
                {
                    g[this.wrld.Random(0, 63)] = this.wrld.Random(0, 63);
                }
                this.wrld.map[x, y] = 2;
                this.wrld.bots[x, y] = new Bot(this.wrld, x, y, g);
                return;
            }
        }
        private void Attack(int dmg)
        {
            this.health -= dmg;
            if (this.health <= 0)
            {
                this.wrld.map[this.coord.x, this.coord.y] = 1;
                this.wrld.bots[this.coord.x, this.coord.y] = null;
            }
        }
        public void WriteGene()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                    Console.Write($"{this.gene[i*8+j]}:");
            }
        
        }
        private bool IsBlocked(int x,int y)
        {
            return (y >= this.wrld.GetHeight || y < 0 || this.wrld.map[x, y] == 3);
        }
        public System.Drawing.Color GetColor(bool drawStyle)
        {
            int r; int g; int b;
            if (drawStyle)
            {
                r = (int)Math.Round(this.orgEating * 9.0);
                if (r > 255) { r = 255; } else if (r < 0) { r = 0; }

                g = (int)Math.Round(this.sunEating * 9.0);
                if (g > 255) { g = 255; } else if (g < 0) { g = 0; }

                b = (int)Math.Round(this.minEating * 9.0);
                if (b > 255) { b = 255; } else if (b < 0) { b = 0; }
                /*
                if (this.kills > 0)
                {
                    r = 255;
                }
                //*/
                return System.Drawing.Color.FromArgb(r, g, b);
            }
            else
            {
                r = (int)Math.Round(this.energy * 2.0);
                if (r > 255) { r = 255; } else if (r < 0) { r = 0; }

                g = 50; ;
                if (g > 255) { g = 255; } else if (g < 0) { g = 0; }

                b = 50;
                if (b > 255) { b = 255; } else if (b < 0) { b = 0; }

                return System.Drawing.Color.FromArgb(r, g, b);
            }
        }
        public static double FotFromCoord(int y, int height)
        {
            return (-1 / (1 + Math.Exp(-10 * (0.7 * y / height) + 5)) + 1) * 2;
        }
        public static double MinFromCoord(int y, int height)
        {
            return (1 / (1 + Math.Exp(-10 * (1.0 * y / height) + 3.5))) * 2;
        }
    }
}
