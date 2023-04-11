using System.Collections.Generic;
using Data;
using GeneticAlgorithmSpaceUtilization;

public class FitnessEvaluator
{
        public static float EvaluateFitness(List<Schedule> population, List<Facilitator> facilitators)
        {
            float totalFitness = 0;

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

                        // Activity is scheduled at the same time on the same day with the same facilitator: -0.5
                        if (schedule.Assignments.Any(a => a != assignment && a.Day == assignment.Day && a.TimeSlot == assignment.TimeSlot && a.Facilitator == assignment.Facilitator))
                        {
                            fitness -= 0.5;
                        }
                        // Update the facilitator load count
                        facilitatorLoad[assignment.Facilitator] += 1;
                    }

                    // Facilitator load penalties and rewards
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
                    
                schedule.Fitness = fitness;
                totalFitness += (float)fitness;
            }

            float averageFitness = totalFitness / population.Count;
            return averageFitness;
        }
    
}


    
