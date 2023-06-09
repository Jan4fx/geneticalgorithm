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
                        // Activity is scheduled at the same time on the same day in the same room as another of the activities: -0.5
                        if (schedule.Assignments.Any(a => a != assignment && a.Room == assignment.Room && a.Day == assignment.Day && Math.Abs((a.TimeSlot - assignment.TimeSlot).TotalMinutes) <= 49))
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
                        // Activity-specific adjustments
                        if (assignment.Activity.Name == "SLA101")
                        {
                            //verifies the assignment isn't checking for itself, and that this other assignment is either SLA109 or SLA101
                            foreach (Assignment otherAssignment in schedule.Assignments.Where(a => a != assignment && (a.Activity.Name == "SLA109" || a.Activity.Name == "SLA101")))
                            {
                                    double timeDifference = Math.Abs((assignment.TimeSlot - otherAssignment.TimeSlot).TotalMinutes);

                                    if (timeDifference == 0) // Both sections in the same time slot
                                    {
                                        fitness -= 0.5;
                                    }
                                    else if (otherAssignment.Activity.Name == "SLA101" && timeDifference >= 240) // SLA 101 sections more than 4 hours apart
                                    {
                                        fitness += 0.5;
                                    }
                                    if(otherAssignment.Activity.Name == "SLA109"){
                                        double checkTimeDifference = Math.Abs((assignment.TimeSlot - otherAssignment.TimeSlot).TotalMinutes);

                                        if (checkTimeDifference == 60) // Consecutive time slots
                                        {
                                            fitness += 0.5;

                                            bool isAssignmentInRomanOrBeach = (assignment.Room.Name == "Roman201" || assignment.Room.Name == "Roman216" || assignment.Room.Name == "Beach201" || assignment.Room.Name == "Beach301");
                                            bool isOtherAssignmentInRomanOrBeach = (otherAssignment.Room.Name == "Roman201" || otherAssignment.Room.Name == "Roman216" || otherAssignment.Room.Name == "Beach201" || otherAssignment.Room.Name == "Beach301");

                                            // Check if either of the assignments is in Roman or Beach, and the other isn't
                                            if (isAssignmentInRomanOrBeach != isOtherAssignmentInRomanOrBeach)
                                            {
                                                fitness -= 0.4;
                                            }
                                        }
                                        else if(checkTimeDifference == 0){
                                            fitness -= 0.25;
                                        }
                                        else if(checkTimeDifference == 120){
                                            fitness += 0.25;
                                        }

                                    }
                            }
                        }
                        // Activity-specific adjustments
                        if (assignment.Activity.Name == "SLA191")
                        {
                            //verifies the assignment isn't checking for itself, and that this other assignment is either SLA109
                            foreach (Assignment otherAssignment in schedule.Assignments.Where(a => a != assignment && (a.Activity.Name == "SLA109")))
                            {

                                    double timeDifference = Math.Abs((assignment.TimeSlot - otherAssignment.TimeSlot).TotalMinutes);

                                    if (timeDifference == 0) // Both sections in the same time slot
                                    {
                                        fitness -= 0.5;
                                    }
                                    else if (otherAssignment.Activity.Name == "SLA191" && timeDifference >= 240) // SLA 191 sections more than 4 hours apart
                                    {
                                        fitness += 0.5;
                                    }
                                
                            }
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

                // Check facilitator time conflicts and set a flag if there's at least one conflict for each facilitator
                // Check to see time differences between facilitator assignments and locations
                foreach (Facilitator facilitator in facilitators)
                {
                    bool noConflicts = false;
                    var facilitatorAssignments = schedule.Assignments.Where(a => a.Facilitator == facilitator).ToList();

                    for (int i = 0; i < facilitatorAssignments.Count; i++)
                    {
                        for (int j = i + 1; j < facilitatorAssignments.Count; j++)
                        {
                            if (facilitatorAssignments[i].Day == facilitatorAssignments[j].Day)
                            {
                                double timeDifference = Math.Abs((facilitatorAssignments[i].TimeSlot - facilitatorAssignments[j].TimeSlot).TotalMinutes);

                                // Check if two assignments have the same start time (conflict)
                                if (timeDifference == 0)
                                {
                                    noConflicts = false;
                                    fitness -= 2;
                                }

                                // Check if two assignments are separated by one hour on the same day
                                if (timeDifference == 60)
                                {
                                    fitness += 0.5;
                                    bool isAssignment1InSpecialRoom = facilitatorAssignments[i].Room.Name == "Roman201" || facilitatorAssignments[i].Room.Name == "Roman216" || facilitatorAssignments[i].Room.Name == "Beach201" || facilitatorAssignments[i].Room.Name == "Beach301";
                                    bool isAssignment2InSpecialRoom = facilitatorAssignments[j].Room.Name == "Roman201" || facilitatorAssignments[j].Room.Name == "Roman216" || facilitatorAssignments[j].Room.Name == "Beach201" || facilitatorAssignments[j].Room.Name == "Beach301";

                                    // Check if one of the two assignments is in room Roman or room Beach and the other is in neither
                                    if (isAssignment1InSpecialRoom != isAssignment2InSpecialRoom)
                                    {
                                        fitness -= 0.4;
                                    }
                                }
                            }
                        }
                    }
                    if (noConflicts)
                    {
                        fitness += 0.2;
                    }
                }



                    // Facilitator load penalties and rewards
                    // Facilitator wants only 3 or 4 activities per week
                    foreach (var kvp in facilitatorLoad)
                    {
                        Facilitator facilitator = kvp.Key;
                        int load = kvp.Value;

                        if (facilitator.Name != "Dr. Tyler" && load < 3)
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


    
