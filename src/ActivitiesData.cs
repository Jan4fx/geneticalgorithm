using System.Collections.Generic;

namespace Data
{
    public class Activity
    {
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<Preferred> Preferred { get; set; } = new List<Preferred>();
        public List<Other> Other { get; set; } = new List<Other>();
        public int ExpectedEnrollment { get; set; }
        public DayOfWeek Day { get; set; }

        public Activity Clone()
        {
            return new Activity
            {
                Name = this.Name,
                StartTime = this.StartTime,
                Preferred = this.Preferred.Select(p => p.Clone()).ToList(),
                Other = this.Other.Select(o => o.Clone()).ToList(),
                ExpectedEnrollment = this.ExpectedEnrollment,
                Day = this.Day,
            };
        }
    }
    public class Preferred
    {
        public string Name { get; set; }

        public Preferred Clone()
        {
            return new Preferred
            {
                Name = this.Name,
            };
        }
    }

    public class Other
    {
        public string Name { get; set; }

        public Other Clone()
        {
            return new Other
            {
                Name = this.Name,
            };
        }
    }
    public static class ActivitiesData
    {
        public static List<Activity> GetActivities()
        {
            return new List<Activity>
            {
                // Add activities here
                new Activity { Name = "SLA100A", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Lock" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }}},
                new Activity { Name = "SLA100B", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Lock" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }}},
                new Activity { Name = "SLA191A", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Lock" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }}},
                new Activity { Name = "SLA191B", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Lock" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }}},
                new Activity { Name = "SLA201", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" }, new Preferred { Name = "Shaw" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }, new Other { Name = "Singer" }}},
                new Activity { Name = "SLA291", ExpectedEnrollment = 50, Preferred = new List<Preferred> { new Preferred { Name = "Lock" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Zeldin" }, new Preferred { Name = "Singer" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Richards" }, new Other { Name = "Shaw" }, new Other { Name = "Tyler" }}},
                new Activity { Name = "SLA303", ExpectedEnrollment = 60, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Zeldin" }, new Preferred { Name = "Banks" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Singer" }, new Other { Name = "Shaw" }}},
                new Activity { Name = "SLA304", ExpectedEnrollment = 25, Preferred = new List<Preferred> { new Preferred { Name = "Glen" }, new Preferred { Name = "Banks" }, new Preferred { Name = "Tyler" } }, Other = new List<Other> { new Other { Name = "Numen" }, new Other { Name = "Singer" }, new Other { Name = "Shaw" }, new Other { Name = "Richards" }, new Other { Name = "Uther" }, new Other { Name = "Zeldin" }}},
                new Activity { Name = "SLA394", ExpectedEnrollment = 20, Preferred = new List<Preferred> { new Preferred { Name = "Tyler" }, new Preferred { Name = "Singer" } }, Other = new List<Other> { new Other { Name = "Richards" }, new Other { Name = "Zeldin" }}},
                new Activity { Name = "SLA449", ExpectedEnrollment = 60, Preferred = new List<Preferred> { new Preferred { Name = "Tyler" }, new Preferred { Name = "Singer" }, new Preferred { Name = "Shaw" } }, Other = new List<Other> { new Other { Name = "Zeldin" }, new Other { Name = "Uther" }}},
                new Activity { Name = "SLA451", ExpectedEnrollment = 100, Preferred = new List<Preferred> { new Preferred { Name = "Tyler" }, new Preferred { Name = "Singer" }, new Preferred { Name = "Shaw" } }, Other = new List<Other> { new Other { Name = "Zeldin" }, new Other { Name = "Uther" }, new Other { Name = "Richards" }, new Other { Name = "Banks" }}}

            };
        }
    }
}
