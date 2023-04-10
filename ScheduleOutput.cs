using System;
using System.IO;
using GeneticAlgorithmSpaceUtilization;

public static class ScheduleOutput
{
    public static void PrintScheduleToFile(Schedule bestSchedule, int generation)
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

    public static void PrintFinalScheduleToFile(Schedule bestSchedule)
    {
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
}
