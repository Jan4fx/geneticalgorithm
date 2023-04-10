using System.Collections.Generic;

namespace Data
{
    public class Facilitator
    {
        public string Name { get; set; } = string.Empty;
    }
    
    public static class FacilitatorsData
    {
        public static List<Facilitator> GetFacilitators()
        {
            return new List<Facilitator>
            {
                new Facilitator { Name = "Lock" },
                new Facilitator { Name = "Glen" },
                new Facilitator { Name = "Banks" },
                new Facilitator { Name = "Richards" },
                new Facilitator { Name = "Shaw" },
                new Facilitator { Name = "Singer" },
                new Facilitator { Name = "Uther" },
                new Facilitator { Name = "Numen" },
                new Facilitator { Name = "Zeldin" },
                //no penalties for Dr Tyler
                new Facilitator { Name = "Dr. Tyler" }
            };
        }
    }
}
