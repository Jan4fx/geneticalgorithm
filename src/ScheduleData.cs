using System.Collections.Generic;

namespace Data
{
    public class Assignment
    {
        public Activity Activity { get; set; } = new Activity();
        public Room Room { get; set; } = new Room();
        public TimeSpan TimeSlot { get; set; }
        public Facilitator Facilitator { get; set; } = new Facilitator();
        public DayOfWeek Day { get; set; }
    }
    public class Schedule
    {
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public double Fitness { get; set; }
    }
}
