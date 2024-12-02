using System;
using System.Collections.Generic;
using System.Linq;

class AntColonyTSP
{
    private static int numberOfCities = 300;
    private static int numberOfAnts = 100;
    private static int maxIterations = 100;
    private static double alpha = 1.0; // Вплив феромонів
    private static double beta = 5.0; // Вплив відстані
    private static double evaporationRate = 0.2; // Швидкість випаровування феромонів
    private static double pheromoneInitial = 1.0; // Початковий рівень феромонів

    private static Random random = new Random();

    private static double[,] distances;
    private static double[,] pheromones;
    private const string MatrixFileName = "distances.csv";

    public static void Main(string[] args)
    {
        if (File.Exists(MatrixFileName))
        {
            LoadDistances(MatrixFileName);
            Console.WriteLine("Matrix loaded from file:");
        }
        else
        {
            GenerateRandomDistances();
            SaveDistances(MatrixFileName);
        }
        
        PrintDistances();
        InitializePheromones();

        double bestLength = double.MaxValue;
        List<int> bestRoute = null;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            var routes = new List<List<int>>();
            var lengths = new List<double>();

            for (int ant = 0; ant < numberOfAnts; ant++)
            {
                var route = ConstructSolution();
                var length = CalculateRouteLength(route);

                routes.Add(route);
                lengths.Add(length);

                if (length < bestLength)
                {
                    bestLength = length;
                    bestRoute = route;
                }
            }

            UpdatePheromones(routes, lengths);
            Console.WriteLine($"Iteration {iteration + 1}: Best Length = {bestLength}");
        }

        Console.WriteLine("Best Route: " + string.Join(" -> ", bestRoute));
        Console.WriteLine($"Best Length: {bestLength}");
    }
    
    private static void SaveDistances(string fileName)
    {
        using (var writer = new StreamWriter(fileName))
        {
            for (int i = 0; i < numberOfCities; i++)
            {
                var line = string.Join(",", Enumerable.Range(0, numberOfCities).Select(j => distances[i, j].ToString()));
                writer.WriteLine(line);
            }
        }
        Console.WriteLine($"Matrix saved to {fileName}");
    }

    private static void LoadDistances(string fileName)
    {
        var lines = File.ReadAllLines(fileName);
        distances = new double[numberOfCities, numberOfCities];

        for (int i = 0; i < numberOfCities; i++)
        {
            var values = lines[i].Split(',').Select(double.Parse).ToArray();
            for (int j = 0; j < numberOfCities; j++)
            {
                distances[i, j] = values[j];
            }
        }
        Console.WriteLine($"Matrix loaded from {fileName}");
    }
    private static void PrintDistances()
    {
        Console.WriteLine("Graph Distances (matrix):");
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.Write($"{distances[i, j],4} ");
            }
            Console.WriteLine();
        }
    }

    private static void GenerateRandomDistances()
    {
        distances = new double[numberOfCities, numberOfCities];

        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                if (i != j)
                {
                    distances[i, j] = random.Next(5, 151);
                }
                else
                {
                    distances[i, j] = 0;
                }
            }
        }
    }

    private static void InitializePheromones()
    {
        pheromones = new double[numberOfCities, numberOfCities];

        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                pheromones[i, j] = pheromoneInitial;
            }
        }
    }

    private static List<int> ConstructSolution()
    {
        var route = new List<int>();
        var visited = new HashSet<int>();

        int currentCity = random.Next(numberOfCities);
        route.Add(currentCity);
        visited.Add(currentCity);

        while (route.Count < numberOfCities)
        {
            int nextCity = SelectNextCity(currentCity, visited);
            route.Add(nextCity);
            visited.Add(nextCity);
            currentCity = nextCity;
        }

        return route;
    }

    private static int SelectNextCity(int currentCity, HashSet<int> visited)
    {
        var probabilities = new double[numberOfCities];
        double sum = 0;

        for (int i = 0; i < numberOfCities; i++)
        {
            if (!visited.Contains(i))
            {
                double attraction = Math.Pow(pheromones[currentCity, i], alpha) * Math.Pow(1.0 / distances[currentCity, i], beta);
                probabilities[i] = attraction;
                sum += attraction;
            }
        }

        double randomValue = random.NextDouble() * sum;
        double cumulative = 0;

        for (int i = 0; i < numberOfCities; i++)
        {
            if (!visited.Contains(i))
            {
                cumulative += probabilities[i];
                if (cumulative >= randomValue)
                {
                    return i;
                }
            }
        }

        throw new Exception("No city selected. This should never happen.");
    }

    private static double CalculateRouteLength(List<int> route)
    {
        double length = 0;

        for (int i = 0; i < route.Count - 1; i++)
        {
            length += distances[route[i], route[i + 1]];
        }

        length += distances[route[route.Count - 1], route[0]];
        return length;
    }

    private static void UpdatePheromones(List<List<int>> routes, List<double> lengths)
    {
        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                pheromones[i, j] *= (1 - evaporationRate);
            }
        }

        for (int k = 0; k < routes.Count; k++)
        {
            var route = routes[k];
            double contribution = 1.0 / lengths[k];

            for (int i = 0; i < route.Count - 1; i++)
            {
                pheromones[route[i], route[i + 1]] += contribution;
            }

            pheromones[route[route.Count - 1], route[0]] += contribution;
        }
    }
}