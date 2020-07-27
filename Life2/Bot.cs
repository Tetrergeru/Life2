using System;
using System.Drawing;
using System.Linq;

namespace Life2
{
    internal class Bot
    {
        public static string[] commands =
        {
            "Photosynthesis",
            "Chemosyntesis",
            "Look",
            "Rotate",
            "Move",
            "Attack",
            "Eat",
            "Clone",
            "Heal",
            "Share",
            "Suicide",
            "SetBlock",
            "BreakBlock",
            "CheckEnergy",
            "Mine"
        };

        private int instructionPointer;
        public int lastIteration = -1;

        private int marker;
        private int kills;
        private int age;

        public double minEating;
        public double sunEating;
        public double orgEating;

        /// <summary>
        /// Мир, которому принадлежит бот
        /// </summary>
        private readonly World world;

        /// <summary>
        /// Запас энергии бота
        /// </summary>
        private double energy;

        /// <summary>
        /// Запас минералов бота
        /// </summary>
        private double minerals;

        /// <summary>
        /// Здоровье бота
        /// </summary>
        private int health;

        /// <summary>
        /// Координаты бота
        /// </summary>
        private (int x, int y) coordinates;

        /// <summary>
        /// Направление, в котором смотрит бот (ноль - вправо, против часовой клетки по сорок пять градусов)
        /// </summary>
        private int direction;

        /// <summary>
        /// Генотип бота
        /// </summary>
        public readonly int[] gene;

        public Bot(World world, int x, int y, int[] gene)
        {
            this.world = world;
            energy = 10;
            minerals = 0;
            health = 10;
            coordinates = (x, y);
            this.gene = gene;
            direction = this.world.Random(0, 7);
        }

        public void Live(int lastIteration)
        {
            this.lastIteration = lastIteration;
            age++;

            instructionPointer = 0;
            minEating = 0;
            sunEating = 0;
            orgEating = 0;

            var height = world.GetHeight;

            energy -= 5;
            marker += 1;


            if (energy > 50 && world.Random(0, 3) == 0)
                Clone();
            else
            {
                for (var steps = 0; steps < 30;)
                {
                    switch (gene[instructionPointer])
                    {
                        //Фотосинтез
                        case 25:
                        {
                            energy += FotFromCoordinates(coordinates.y, height);
                            sunEating += FotFromCoordinates(coordinates.y, height);
                            instructionPointer++;
                            steps++;
                            break;
                        }
                        //Преобразует минералы в энергию
                        case 26:
                        {
                            if (minerals > 2)
                            {
                                minerals -= 2;
                                minEating += 3;
                                energy += 3;
                            }

                            instructionPointer++;
                            steps++;
                            break;
                        }
                        //Перемещает указатель, в зависимости от того, на что смотрит бот
                        case 27:
                        {
                            var (x, y) = LookAt();

                            if (IsBlocked(x, y))
                                instructionPointer += 6;
                            else if (world.map[x][y] == 0)
                                instructionPointer += 1;
                            else if (world.map[x][y] == ObjectType.Food)
                                instructionPointer += 2;
                            else if (IsKin(world.bots[x][y]))
                                instructionPointer += 3;
                            else if (world.bots[x][y].kills > 0)
                                instructionPointer += 4;
                            else
                                instructionPointer += 5;

                            energy -= 1;
                            steps += 1;
                            break;
                        }
                        //Поворачивает бота
                        case 28:
                        {
                            energy -= 1;
                            steps += 1;

                            instructionPointer += 1;
                            instructionPointer %= World.GeneLength;

                            direction += gene[instructionPointer];
                            direction %= 8;

                            instructionPointer += 1;
                            break;
                        }
                        //Двигает бота в том направлении, куда он смотрит
                        case 29:
                        {
                            energy -= 5;
                            var (x, y) = LookAt();
                            if (!IsBlocked(x, y) && world.map[x][y] != ObjectType.Bot)
                            {
                                if (world.map[x][y] == ObjectType.Food)
                                {
                                    orgEating += 20;
                                    energy += 20;
                                }

                                world.map[x][y] = ObjectType.Bot;
                                world.map[coordinates.x][coordinates.y] = 0;

                                world.bots[x][y] = this;
                                world.bots[coordinates.x][coordinates.y] = null;

                                coordinates = (x, y);
                            }

                            instructionPointer++;
                            steps += 5;
                            break;
                        }
                        //Атакует бота, в направлении взгляда
                        case 30:
                        {
                            var (x, y) = LookAt();
                            energy -= 5;
                            if (!IsBlocked(x, y) && world.map[x][y] == ObjectType.Bot)
                            {
                                world.bots[x][y].Attack(20);
                                energy += 10;
                                if (world.map[x][y] == ObjectType.Food)
                                    kills++;
                            }

                            instructionPointer++;
                            steps += 15;
                            break;
                        }
                        //Съедает еду напротив бота
                        case 31:
                        {
                            instructionPointer += 1;
                            steps += 15;

                            var (x, y) = LookAt();
                            if (!IsBlocked(x, y) && world.map[x][y] == ObjectType.Food)
                            {
                                world.map[x][y] = 0;
                                energy += 20;
                                orgEating += 20;
                            }

                            break;
                        }
                        //Бот делится
                        case 32:
                        {
                            instructionPointer += 1;
                            steps += 15;

                            Clone(direction);
                            break;
                        }
                        //Бот лечится
                        case 33:
                        {
                            instructionPointer++;
                            steps += 15;

                            if (energy > 5)
                            {
                                energy -= 5;
                                health += 5;
                            }

                            break;
                        }

                        //Передаёт часть энергии другому боту
                        case 34:
                        {
                            instructionPointer++;
                            steps += 15;

                            var (x, y) = LookAt();
                            if (!IsBlocked(x, y) && world.map[x][y] == ObjectType.Bot)
                            {
                                world.bots[x][y].energy += 5;
                                energy -= 5;
                            }

                            break;
                        }

                        //Убивает себя
                        case 35:
                        {
                            Die();
                            return;
                        }
                        //Разрушает блок стены
                        case 36:
                        {
                            instructionPointer++;
                            steps += 10;

                            if (energy <= 5)
                                break;

                            var (x, y) = LookAt();
                            if (y >= 0 && y < height && world.map[x][y] == ObjectType.Wall)
                            {
                                world.map[x][y] = 0;
                                minerals += 11;
                            }

                            energy -= 2;

                            break;
                        }
                        //Ставит блок стены
                        case 37:
                        {
                            instructionPointer += 1;
                            steps += 5;

                            if (minerals >= 10)
                            {
                                var (x, y) = LookAt();
                                if (y >= 0 && y < height && world.map[x][y] == 0)
                                {
                                    world.map[x][y] = ObjectType.Wall;
                                    minerals -= 10;
                                }
                            }

                            break;
                        }
                        //Проверяет энергию
                        case 38:
                        {
                            instructionPointer += 1;
                            steps += 1;

                            instructionPointer %= gene.Length;
                            if (1.0 * (energy + 200) / 200 > (gene[instructionPointer] + 1) / 32.0)
                                instructionPointer += 2;
                            else
                                instructionPointer += 3;

                            break;
                        }
                        //Добывает минералы
                        case 39:
                        {
                            instructionPointer += 1;
                            steps += 1;

                            minerals += MinFromCoordinates(coordinates.y, height);
                            if (minerals > 100)
                                minerals = 100;

                            break;
                        }
                        default:
                        {
                            instructionPointer += gene[instructionPointer];
                            steps += 3;

                            break;
                        }
                    }

                    instructionPointer %= World.GeneLength;

                    if (energy > 200)
                        energy = 200;

                    if (health > 99)
                        health = 99;

                    if (health <= 0 || energy <= 0)
                    {
                        Die();
                        return;
                    }
                }
            }

            if (health <= 0 || energy <= 0 || age > 50)
                Die();
        }

        private bool IsKin(Bot bot) => GetKinship(bot) < 10;

        public int GetKinship(Bot bot)
        {
            var k = 0;
            for (var i = 0; i < 64; i++)
                if (gene[i] != bot.gene[i])
                    k++;
            return k;
        }

        private (int x, int y) LookAt(int direction)
        {
            var width = world.GetWidth;
            var x = coordinates.x + (int) Math.Round(Math.Cos(Math.PI * 0.25 * direction));
            var y = coordinates.y - (int) Math.Round(Math.Sin(Math.PI * 0.25 * direction));

            x = (x + width) % width;

            return (x, y);
        }

        private (int x, int y) LookAt() => LookAt(direction);

        private void Clone()
        {
            for (var i = 0; i < 8; i++)
                if (Clone(i))
                    break;
        }

        private bool Clone(int i)
        {
            var height = world.GetHeight;

            var (x, y) = LookAt(i);
            if (y < 0 || y >= height || world.map[x][y] != 0)
                return false;
                
            energy -= 30;
            var babyGene = gene.Select(_ => _).ToArray();

            if (world.Random(0, 2) == 0)
                babyGene[world.Random(0, World.GeneLength - 1)] = world.Random(0, World.GeneLength - 1);

            world.map[x][y] = ObjectType.Bot;
            world.bots[x][y] = new Bot(world, x, y, babyGene);
            
            return true;
        }

        private void Die()
        {
            world.map[coordinates.x][coordinates.y] = ObjectType.Food;
            world.bots[coordinates.x][coordinates.y] = null;
        }

        private void Attack(int dmg)
        {
            health -= dmg;
            if (health <= 0)
                Die();
        }

        private bool IsBlocked(int x, int y)
            => y >= world.GetHeight || y < 0 || world.map[x][y] == ObjectType.Wall;

        private static int ToBounds(int value, int from, int to)
            => value > to ? to : value < from ? from : value;
        
        public Color GetColor(DrawType drawStyle)
        {
            int r = 0, g = 0, b = 0;
            if (drawStyle == DrawType.Food)
            {
                r = (int) Math.Round(orgEating * 9.0);
                r = ToBounds(r, 0, 255);
                
                g = (int) Math.Round(sunEating * 9.0);
                g = ToBounds(g, 0, 255);

                b = (int) Math.Round(minEating * 9.0);
                b = ToBounds(b, 0, 255);
            }
            else if (drawStyle == DrawType.Energy)
            {
                r = (int) Math.Round(energy * 2.0);
                r = ToBounds(r, 0, 255);

                g = 50;

                b = marker;
                b = ToBounds(b, 0, 255);
            }
            
            return Color.FromArgb(r, g, b);
        }

        public static double FotFromCoordinates(int y, int height)
            => (-1 / (1 + Math.Exp(-10 * (0.7 * y / height) + 5)) + 1) * 2;

        public static double MinFromCoordinates(int y, int height)
            => 1 / (1 + Math.Exp(-10 * (1.0 * y / height) + 3.5)) * 2;
    }
}