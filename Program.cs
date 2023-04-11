using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Data;
using MathNet.Numerics; 

//Make two offspring instead of 1
//Questions to ask, are activities scheduled everyday of the three days
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
        //static List<float> generationFitness = new List<float>();
        private const int PopulationSize = 1000;
        private const int Generations = 100;
        private const double MutationRate = 0.01;
        private const double FitnessImprovementThreshold = 0.01;
        static List<Activity> activities;
        static List<Room> rooms;
        static List<Facilitator> facilitators;
        static Random random = new Random();
        static void Main(string[] args)
        {
            File.WriteAllText("output.txt", string.Empty);
            InitializeData();
            List<Schedule> population = GenerateInitialPopulation(PopulationSize);
            Schedule bestSchedule = GeneticAlgorithm(population);
            //PrintFinalScheduleToFile(bestSchedule);
            ScheduleOutput.PrintFinalScheduleToFile(bestSchedule);

        }

        static void InitializeData()
        {
            // Initialize example activities
            activities = ActivitiesData.GetActivities();

            // Initialize example rooms
            rooms = RoomsData.GetRooms();

            // Initialize example facilitators
            facilitators = FacilitatorsData.GetFacilitators();

            // Restrict activity start times
            List<int> allowedStartHours = new List<int> { 10, 11, 12, 13, 14, 15 };
            foreach (Activity activity in activities)
            {
                activity.StartTime = new TimeSpan(allowedStartHours[random.Next(allowedStartHours.Count)], 0, 0);
            }
            DayOfWeek[] allowedDays = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            foreach (Activity activity in activities)
            {
                activity.StartTime = new TimeSpan(allowedStartHours[random.Next(allowedStartHours.Count)], 0, 0);
                activity.Day = allowedDays[random.Next(allowedDays.Length)];
    }
        }


        static List<Schedule> GenerateInitialPopulation(int size)
        {
            List<Schedule> population = new List<Schedule>();

            for (int i = 0; i < size; i++)
            {
                Schedule schedule = new Schedule();

                foreach (Activity activity in activities)
                {
                    Assignment assignment = new Assignment();
                    assignment.Activity = activity;
                    assignment.Room = rooms[random.Next(rooms.Count)];
                    assignment.TimeSlot = new TimeSpan(random.Next(10, 16), random.Next(60), 0);
                    assignment.Facilitator = facilitators[random.Next(facilitators.Count)];
                    assignment.Day = activity.Day;

                    schedule.Assignments.Add(assignment);
                }

                population.Add(schedule);
            }

            return population;
        }

        static Schedule GeneticAlgorithm(List<Schedule> population)
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

            using (StreamWriter outputFile = new StreamWriter("output.txt", true))
            {
                while (generation < Generations || (currentAverageFitness - prevAverageFitness) / prevAverageFitness > FitnessImprovementThreshold)
                {
                    generation++;

                    // Evaluate the fitness of the population
                    FitnessEvaluator.EvaluateFitness(population, facilitators);

                    // Compute the average fitness
                    //currentAverageFitness = population.Average(schedule => schedule.Fitness);

                    prevAverageFitness = currentAverageFitness;
                    ///Figure out if averaging
                    currentAverageFitness = FitnessEvaluator.EvaluateFitness(population, facilitators);

                    // Softmax normalization
                    var fitnessValues = population.Select(schedule => schedule.Fitness).ToArray();
                    double[] softmaxProbabilities = Softmax.Compute(fitnessValues);

                    // Tournament Selection
                    List<Schedule> parents = SelectParents(population, 10); // Change 10 to your desired tournament size

                    // Perform crossover and mutation to create offspring
                    List<Schedule> offspring = CrossoverAndMutation(parents);

                    // Replace the old population with the new offspring if the offspring list is not empty
                    if (offspring.Count > 0)
                    {
                        population = offspring;
                    }
                    else
                    {
                        break;
                    }

                    // Output offspring schedules, fitness, and generation number to the text file
                    Schedule bestSchedule = offspring.OrderByDescending(schedule => schedule.Fitness).First();
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
                }
            }

            // Return the best schedule found
            return population.OrderByDescending(schedule => schedule.Fitness).First();
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

                Schedule child1 = Crossover(parent1, parent2);
                Schedule child2 = Crossover(parent2, parent1);

                Mutate(child1);
                Mutate(child2);

                offspring.Add(child1);
                offspring.Add(child2);
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
                    schedule.Assignments[i].TimeSlot = new TimeSpan(new Random().Next(10, 16), new Random().Next(60), 0);
                    schedule.Assignments[i].Facilitator = facilitators[new Random().Next(facilitators.Count)];
                    schedule.Assignments[i].Day = allowedDays[new Random().Next(allowedDays.Length)];
                }
            }
        }

    }   
}