using System;
using System.IO;
using GeneticAlgorithmSpaceUtilization;
using Data;
using System.Linq;

public static class ScheduleOutput
{
    private static DayOfWeek[] DayOrder = { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

    public static void PrintScheduleToFile(Schedule bestSchedule, int generation)
    {
        using (StreamWriter outputFile = new StreamWriter("GenerationBestSchedule.txt", true))
        {
            outputFile.WriteLine($"Generation {generation}:");
            outputFile.WriteLine("Best Schedule:");
            outputFile.WriteLine("Fitness: " + bestSchedule.Fitness);

            var sortedAssignments = bestSchedule.Assignments.Where(a => DayOrder.Contains(a.Day)).OrderBy(a => Array.IndexOf(DayOrder, a.Day)).ThenBy(a => a.TimeSlot);

            foreach (Assignment assignment in sortedAssignments)
            {
                outputFile.WriteLine($"Activity: {assignment.Activity.Name}, Day: {assignment.Day}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
            }

            outputFile.WriteLine(); // Add an empty line to separate generations
        }
    }

    public static void PrintFinalScheduleToFile(Schedule bestSchedule, int generation)
    {
        using (StreamWriter outputFile = new StreamWriter("FinalSchedule.txt"))
        {
            generation -= 1;
            outputFile.WriteLine("Final Output:");
            outputFile.WriteLine("Generation " + generation);
            outputFile.WriteLine("Fitness: " + bestSchedule.Fitness);

            var sortedAssignments = bestSchedule.Assignments.Where(a => DayOrder.Contains(a.Day)).OrderBy(a => Array.IndexOf(DayOrder, a.Day)).ThenBy(a => a.TimeSlot);

            foreach (Assignment assignment in sortedAssignments)
            {
                outputFile.WriteLine($"Activity: {assignment.Activity.Name}, Day: {assignment.Day}, Time: {assignment.TimeSlot}, Room: {assignment.Room.Name}, Facilitator: {assignment.Facilitator.Name}");
            }
        }
    }
}
