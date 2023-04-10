using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Data;
using MathNet.Numerics; // Added for Softmax



//tournament selection randomly pick 10 out of the grouup and select top 3 
namespace GeneticAlgorithmSpaceUtilization
{

    public class Assignment
        {
        public Activity Activity { get; set; } = new Activity();
        public Room Room { get; set; } = new Room();
        public TimeSpan TimeSlot { get; set; }
        public Facilitator Facilitator { get; set; } = new Facilitator();
    }

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

    public class Schedule
    {
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public double Fitness { get; set; }
    }

    public class Program
    {
        private const int PopulationSize = 1000;
        private const int Generations = 200;
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

            // Overwrite the "output.txt" file with the best schedule found
            using (StreamWriter outputFile = new StreamWriter("final_output.txt"))
            {
                outputFile.WriteLine("Best Schedule:");
                outputFile.WriteLine("Fitness: " + bestSchedule.Fitness);

                foreach (Assignment assignment in bestSchedule.Assignments)
                {
                    outputFile.WriteLine($"Activity: {assignment.Activity.Name}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
                }
            }
        }


        static void InitializeData()
        {
            // Initialize example activities
            activities = ActivitiesData.GetActivities();

            // Initialize example rooms
            rooms = RoomsData.GetRooms();

            // Initialize example facilitators
            facilitators = FacilitatorsData.GetFacilitators();
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

                    schedule.Assignments.Add(assignment);
                }

                population.Add(schedule);
            }

            return population;
        }

        static Schedule GeneticAlgorithm(List<Schedule> population)
        {
            int generation = 0;
            double prevAverageFitness = 0;
            double currentAverageFitness = 0;

            if (population.Count == 0)
            {
                throw new InvalidOperationException("The population is empty. Cannot run the genetic algorithm.");
            }

            while (generation < Generations || (currentAverageFitness - prevAverageFitness) / prevAverageFitness > FitnessImprovementThreshold)
            //while (generation < Generations && (currentAverageFitness - prevAverageFitness) / prevAverageFitness > FitnessImprovementThreshold)
            {
                generation++;

                // Evaluate the fitness of the population
                EvaluateFitness(population);

                // Compute the average fitness
                prevAverageFitness = currentAverageFitness;
                currentAverageFitness = population.Average(schedule => schedule.Fitness);

                // Softmax normalization
                var fitnessValues = population.Select(schedule => schedule.Fitness).ToArray();
                //var softmaxProbabilities = SpecialFunctions.Softmax(fitnessValues);
                double[] softmaxProbabilities = Softmax.Compute(fitnessValues);

                // Select parents for reproduction based on their fitness
                List<Schedule> parents = SelectParents(population, softmaxProbabilities);

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

                // Find the best schedule in the current generation and print it to the file
                Schedule bestSchedule = population.OrderByDescending(schedule => schedule.Fitness).First();
                PrintScheduleToFile(bestSchedule, generation); // Remove 'generation' variable from the method call
            }

            // Return the best schedule found
            return population.OrderByDescending(schedule => schedule.Fitness).First();
        }





    static void EvaluateFitness(List<Schedule> population)
        {
            foreach (Schedule schedule in population)
            {
                double fitness = 0;

                // Count the facilitator's load
                Dictionary<Facilitator, int> facilitatorLoad = new Dictionary<Facilitator, int>();
                foreach (Facilitator facilitator in facilitators)
                {
                    facilitatorLoad[facilitator] = 0;
                }

                foreach (Assignment assignment in schedule.Assignments)
                {
                    // Activity is scheduled at the same time in the same room as another of the activities: -0.5
                    if (schedule.Assignments.Any(a => a != assignment && a.Room == assignment.Room && a.TimeSlot == assignment.TimeSlot))
                    {
                        fitness -= 0.5;
                    }

                    // Room size penalties and rewards
                    if (assignment.Room.Capacity < assignment.Activity.ExpectedEnrollment)
                    {
                        fitness -= 0.5;
                    }
                    else if (assignment.Room.Capacity > 3 * assignment.Activity.ExpectedEnrollment)
                    {
                        fitness -= 0.2;
                    }
                    else if (assignment.Room.Capacity > 6 * assignment.Activity.ExpectedEnrollment)
                    {
                        fitness -= 0.4;
                    }
                    else
                    {
                        fitness += 0.3;
                    }
                    
                    // Facilitator-related penalties and rewards
                    if (assignment.Activity.Preferred.Any(p => p.Name == assignment.Facilitator.Name))
                    {
                        fitness += 0.5;
                    }
                    //change to backup
                    else if (assignment.Activity.Other.Any(o => o.Name == assignment.Facilitator.Name))
                    {
                        fitness += 0.2;
                    }
                    else
                    {
                        fitness -= 0.1;
                    }

                    // Update the facilitator load count
                    facilitatorLoad[assignment.Facilitator] += 1;
                    
                }

                // Facilitator load penalties and rewards

                //difference between facilitator and activity facilitator? ignored activity facilitator
                //activity facilitator is the time that they facilitate and if they do more than one at same time
                foreach (var kvp in facilitatorLoad)
                {
                    Facilitator facilitator = kvp.Key;
                    int load = kvp.Value;

                    if (facilitator.Name != "Dr. Tyler" && load > 3)
                    {
                        fitness -= 0.4;
                    }
                    else
                    {
                        if (load > 4)
                        {
                            fitness -= 0.5;
                        }
                    }
                }

                // Activity-specific adjustments
                //... Add the activity-specific adjustments here

                schedule.Fitness = fitness;
            }
        }

        static List<Schedule> SelectParents(List<Schedule> population)
        {
            return population.OrderByDescending(schedule => schedule.Fitness).Take(population.Count / 2).ToList();
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
            for (int i = 0; i < schedule.Assignments.Count; i++)
            {
                if (new Random().NextDouble() < MutationRate)
                {
                    schedule.Assignments[i].Room = rooms[new Random().Next(rooms.Count)];
                    schedule.Assignments[i].TimeSlot = new TimeSpan(new Random().Next(10, 16), new Random().Next(60), 0);
                    schedule.Assignments[i].Facilitator = facilitators[new Random().Next(facilitators.Count)];
                }
            }
        }
        static void PrintScheduleToFile(Schedule bestSchedule, int generation)
        {
            using (StreamWriter outputFile = new StreamWriter("output.txt", true))
            {
                outputFile.WriteLine($"Generation {generation}:");
                outputFile.WriteLine("Best Schedule:");
                outputFile.WriteLine("Fitness: " + bestSchedule.Fitness);

                foreach (Assignment assignment in bestSchedule.Assignments)
                {
                    outputFile.WriteLine($"Activity: {assignment.Activity.Name}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
                }

                outputFile.WriteLine(); // Add an empty line to separate generations
            }
        }
}
}