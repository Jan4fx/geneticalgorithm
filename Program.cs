using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Data;
using MathNet.Numerics;

namespace GeneticAlgorithmSpaceUtilization
{
    public static class Softmax
    {
        public static double[] Compute(IEnumerable<double> input)
        {
            double max = input.Max();
            double[] expValues = input.Select(x => Math.Exp(x - max)).ToArray();
            double sumExp = expValues.Sum();

            return expValues.Select(x => x / sumExp).ToArray();
        }
    }

    public class Program
    {
        private const int PopulationSize = 1000;
        private const int Generations = 100;
        private const double MutationRate = 0.0000000000000001; //more fitness
        //private const double MutationRate = 10; //less fitness
        private const double FitnessImprovementThreshold = 0.01;
        static List<Activity> activities;
        static List<Room> rooms;
        static List<Facilitator> facilitators;
        static Random random = new Random();
        //how many possible children that parents can create
        //for child to be passed they need to have a higher fitness than 
        //one of the two parents
        private static int reproduction = 24;

        static void Main(string[] args)
        {
            File.WriteAllText("GenerationBestSchedule.txt", string.Empty);
            File.WriteAllText("AllSchedules.txt", string.Empty);
            InitializeData();
            List<Schedule> population = GenerateInitialPopulation(PopulationSize);
            Tuple<Schedule, int> result = GeneticAlgorithm(population);
            ScheduleOutput.PrintFinalScheduleToFile(result.Item1, result.Item2);
            Console.WriteLine("---------------------------------------------------------------------------------");
            string fileContent = File.ReadAllText("FinalSchedule.txt");
            Console.WriteLine(fileContent);
            Console.WriteLine("---------------------------------------------------------------------------------");
            //Hurts Performance
            //Console.WriteLine("To View All Schedules Generated --> AllSchedules.txt");
            Console.WriteLine("To View Each Generation's Best Schedule --> GenerationBestSchedule.txt");
            Console.WriteLine("---------------------------------------------------------------------------------");
        }


        static void InitializeData()
        {
            Console.WriteLine("Initializing Data ...");
            // Initialize example activities
            activities = ActivitiesData.GetActivities();

            // Initialize example rooms
            rooms = RoomsData.GetRooms();

            // Initialize example facilitators
            facilitators = FacilitatorsData.GetFacilitators();

            // Restrict activity start times
            List<int> allowedStartHours = new List<int> { 10, 11, 12, 13, 14, 15 };
            DayOfWeek[] allowedDays = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

            // Create activity instances for each day
            List<Activity> expandedActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                foreach (DayOfWeek day in allowedDays)
                {
                    Activity newActivity = activity.Clone();
                    newActivity.StartTime = new TimeSpan(allowedStartHours[random.Next(allowedStartHours.Count)], 0, 0);
                    newActivity.Day = day;
                    expandedActivities.Add(newActivity);
                }
            }
            activities = expandedActivities;
        }

        static List<Schedule> GenerateInitialPopulation(int size)
        {
            Console.WriteLine("Generating Generation 0 ...");
            List<Schedule> population = new List<Schedule>();

            for (int i = 0; i < size; i++)
            {
                Schedule schedule = new Schedule();

                foreach (Activity activity in activities)
                {
                    Assignment assignment = new Assignment();
                    assignment.Activity = activity;
                    assignment.Room = rooms[random.Next(rooms.Count)];
                    assignment.TimeSlot = activity.StartTime;
                    assignment.Facilitator = facilitators[random.Next(facilitators.Count)];
                    assignment.Day = activity.Day;

                    schedule.Assignments.Add(assignment);
                }

                population.Add(schedule);
            }

            return population;
        }

        static Tuple<Schedule, int> GeneticAlgorithm(List<Schedule> population)

        {
            List<double> generationFitness = new List<double>();
            int generation = 0;
            double prevAverageFitness = 0;
            double currentAverageFitness = 0;
            if (population.Count == 0)
            {
                throw new InvalidOperationException("The population is empty. Cannot run the genetic algorithm.");
            }

            DayOfWeek[] DayOrder = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

            using (StreamWriter outputFile = new StreamWriter("GenerationBestSchedule.txt", true))
            //using (StreamWriter detailedOutputFile = new StreamWriter("AllSchedules.txt", true))
            {
                Console.WriteLine("Running Generation 1 ...");
                while (generation < Generations || (currentAverageFitness - prevAverageFitness) / prevAverageFitness > FitnessImprovementThreshold)
                {
                    generation++;

                    // Evaluate the fitness of the population
                    FitnessEvaluator.EvaluateFitness(population, facilitators);

                    prevAverageFitness = currentAverageFitness;
                    currentAverageFitness = FitnessEvaluator.EvaluateFitness(population, facilitators);
                    double outputAverageFitness = Math.Round(currentAverageFitness, 2);
                    Console.WriteLine("Average Fitness is " + outputAverageFitness);

                    // Softmax normalization
                    var fitnessValues = population.Select(schedule => schedule.Fitness).ToArray();
                    double[] softmaxProbabilities = Softmax.Compute(fitnessValues);

                    // Tournament Selection
                    List<Schedule> parents = SelectParents(population, 10);

                    // Perform crossover and mutation to create offspring
                    List<Schedule> offspring = CrossoverAndMutation(parents);

                    if (offspring.Count > 0)
                    {
                        population = offspring;
                    }
                    else
                    {
                        break;
                    }
                    if(generation != 1){
                        Console.WriteLine("Running Generation " + generation + " ...");
                    }
                    Console.WriteLine("Population Count is " + population.Count);
                    if (population.Count > 25000)
                    {
                        Console.WriteLine("!!!Sorry population size has gotten too big!!!");
                        Console.WriteLine("----->Applying smaller reproduction restarting and trying again...");
                        reproduction /= 2;
                        InitializeData();
                        List<Schedule> newPopulation = GenerateInitialPopulation(PopulationSize);
                        return GeneticAlgorithm(newPopulation);
                    }

                    Schedule bestSchedule = offspring.First(schedule => schedule.Fitness == offspring.Max(s => s.Fitness));

                    outputFile.WriteLine($"Generation {generation}:");
                    outputFile.WriteLine($"Population: {population.Count}");
                    outputFile.WriteLine("Best Fitness: " + FitnessEvaluator.EvaluateFitness(offspring, facilitators));
                    outputFile.WriteLine("Best Schedule:");

                    var sortedAssignments = bestSchedule.Assignments.Where(a => DayOrder.Contains(a.Day)).OrderBy(a => Array.IndexOf(DayOrder, a.Day)).ThenBy(a => a.TimeSlot);

                    foreach (Assignment assignment in sortedAssignments)
                    {
                        outputFile.WriteLine($"Activity: {assignment.Activity.Name}, Day: {assignment.Day}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
                    }

                    outputFile.WriteLine("-------------------------------------------------");

                    // Write detailed offspring information to DetailedGenerationInfo.txt
                    //(Hurts performance)

                    /*
                    detailedOutputFile.WriteLine($"Generation {generation}:");
                    foreach (Schedule child in offspring)
                    {
                        detailedOutputFile.WriteLine($"Offspring Fitness: {child.Fitness}");
                        detailedOutputFile.WriteLine("Offspring Schedule:");

                        var offspringAssignments = child.Assignments.Where(a => DayOrder.Contains(a.Day)).OrderBy(a => Array.IndexOf(DayOrder, a.Day)).ThenBy(a => a.TimeSlot);

                        foreach (Assignment assignment in offspringAssignments)
                        {
                            detailedOutputFile.WriteLine($"Activity: {assignment.Activity.Name}, Day: {assignment.Day}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
                        }

                        detailedOutputFile.WriteLine("-------------------------------------------------");
                    }
                    */
                }
            }

            // Return the best schedule found
            //return population.OrderByDescending(schedule => schedule.Fitness).First();
            return new Tuple<Schedule, int>(population.OrderByDescending(schedule => schedule.Fitness).First(), generation);

            }

        static List<Schedule> SelectParents(List<Schedule> population, int tournamentSize)
        {
            List<Schedule> parents = new List<Schedule>();

            for (int i = 0; i < population.Count / 2; i++)
            {
                Schedule bestParent = TournamentSelection(population, tournamentSize);
                parents.Add(bestParent);
            }

            return parents;
        }

        static Schedule TournamentSelection(List<Schedule> population, int tournamentSize)
        {
            Schedule best = null;

            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = random.Next(population.Count);
                Schedule current = population[randomIndex];

                if (best == null || current.Fitness > best.Fitness)
                {
                    best = current;
                }
            }

            return best;
        }

        static List<Schedule> SelectParents(List<Schedule> population, double[] probabilities)
        {
            List<Schedule> parents = new List<Schedule>();

            for (int i = 0; i < population.Count / 2; i++)
            {
                int selectedIndex = SelectIndexByProbability(probabilities);
                parents.Add(population[selectedIndex]);
            }

            return parents;
        }

        // Utility method to select index based on probability distribution
        static int SelectIndexByProbability(double[] probabilities)
        {
            double randomNumber = random.NextDouble();
            double cumulativeProbability = 0.0;

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomNumber <= cumulativeProbability)
                {
                    return i;
                }
            }

            return probabilities.Length - 1;
        }
        static List<Schedule> CrossoverAndMutation(List<Schedule> parents)
        {
            List<Schedule> offspring = new List<Schedule>();

            int endIndex = parents.Count % 2 == 0 ? parents.Count : parents.Count - 1;

            for (int i = 0; i < endIndex; i += 2)
            {
                Schedule parent1 = parents[i];
                Schedule parent2 = parents[i + 1];

                List<Schedule> children = new List<Schedule>();
                //change j less than operator for more possible offspring (at your own risk could get astronomically big)
                for (int j = 0; j < reproduction; j++)
                {
                    Schedule child;
                    if (j % 2 == 0)
                    {
                        child = Crossover(parent1, parent2);
                    }
                    else
                    {
                        child = Crossover(parent2, parent1);
                    }
                    Mutate(child);
                    FitnessEvaluator.EvaluateFitness(new List<Schedule> { child }, facilitators);
                    children.Add(child);
                }

                // Add children only if their fitness is higher than at least one of the parents
                foreach (Schedule child in children)
                {
                    if (child.Fitness > parent1.Fitness || child.Fitness > parent2.Fitness)
                    {
                        offspring.Add(child);
                    }
                }
            }

            return offspring;
        }



        static Schedule Crossover(Schedule parent1, Schedule parent2)
        {
            Schedule child = new Schedule();

            for (int i = 0; i < parent1.Assignments.Count; i++)
            {
                if (new Random().NextDouble() < 0.5)
                {
                    child.Assignments.Add(parent1.Assignments[i]);
                }
                else
                {
                    child.Assignments.Add(parent2.Assignments[i]);
                }
            }

            return child;
        }

        static void Mutate(Schedule schedule)
        {
            DayOfWeek[] allowedDays = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

            for (int i = 0; i < schedule.Assignments.Count; i++)
            {
                if (new Random().NextDouble() < MutationRate)
                {
                    schedule.Assignments[i].Room = rooms[new Random().Next(rooms.Count)];
                    schedule.Assignments[i].TimeSlot = new TimeSpan(new Random().Next(10, 16), 0, 0);
                    schedule.Assignments[i].Facilitator = facilitators[new Random().Next(facilitators.Count)];
                    schedule.Assignments[i].Day = allowedDays[new Random().Next(allowedDays.Length)];
                }
            }
        }

    }   
}
