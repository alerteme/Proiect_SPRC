using System;
using System.Collections.Generic;
using System.Text;

namespace ServerJoc
{
    public class Ship
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public List<Tuple<int, int>> Cells { get; set; } = new List<Tuple<int, int>>();
        public HashSet<string> HitCells { get; set; } = new HashSet<string>();
        public bool IsSunk { get { return HitCells.Count == Size; } }
    }

    public class ShipGrid
    {
        public const int SIZE = 8;
        private int[,] grid = new int[SIZE, SIZE];
        public List<Ship> Ships { get; private set; } = new List<Ship>();

        private static readonly Tuple<string, int>[] SHIP_TYPES = new Tuple<string, int>[]
        {
            Tuple.Create("Crucisator", 4),
            Tuple.Create("Distragator", 3),
            Tuple.Create("Submarin", 2),
            Tuple.Create("Submarin2", 2)
        };

        private static readonly Random rng = new Random(); // static = același obiect
        public void PlaceShipsRandomly()
        {
            Ships.Clear();
            grid = new int[SIZE, SIZE];

            foreach (var shipType in SHIP_TYPES)
            {
                string name = shipType.Item1;
                int size = shipType.Item2;
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 200)
                {
                    attempts++;
                    bool horizontal = rng.Next(2) == 0;
                    int row = rng.Next(SIZE);
                    int col = rng.Next(SIZE);

                    if (CanPlace(row, col, size, horizontal))
                    {
                        var ship = new Ship { Name = name, Size = size };
                        for (int i = 0; i < size; i++)
                        {
                            int r = horizontal ? row : row + i;
                            int c = horizontal ? col + i : col;
                            grid[r, c] = 1;
                            ship.Cells.Add(Tuple.Create(r, c));
                        }
                        Ships.Add(ship);
                        placed = true;
                    }
                }
            }
        }

        private bool CanPlace(int row, int col, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int r = horizontal ? row : row + i;
                int c = horizontal ? col + i : col;
                if (r >= SIZE || c >= SIZE) return false;
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE)
                            if (grid[nr, nc] == 1) return false;
                    }
            }
            return true;
        }
        // Înlocuiește metoda Attack complet:
        public string Attack(int row, int col)
        {
            if (grid[row, col] == 1)
            {
                grid[row, col] = 2;
                foreach (var ship in Ships)
                {
                    foreach (var cell in ship.Cells)
                    {
                        if (cell.Item1 == row && cell.Item2 == col)
                        {
                            // Adaugăm cheia bazată pe poziția în grilă
                            string key = row + "_" + col;
                            ship.HitCells.Add(key);

                            if (ship.IsSunk)
                                return "SINK:" + ship.Name;
                            return "HIT";
                        }
                    }
                }
                return "HIT";
            }
            else if (grid[row, col] == 0)
            {
                grid[row, col] = 3;
                return "MISS";
            }
            else if (grid[row, col] == 2 || grid[row, col] == 3)
            {
                return "ALREADY";
            }
            return "MISS";
        }
        public bool AllSunk
        {
            get
            {
                foreach (var ship in Ships)
                    if (!ship.IsSunk) return false;
                return true;
            }
        }

        public string EncodeForOwner()
        {
            var sb = new StringBuilder();
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    sb.Append(grid[r, c] == 1 || grid[r, c] == 2 ? "1" : "0");
            return sb.ToString();
        }
    }
}