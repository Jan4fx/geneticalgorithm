using System.Collections.Generic;

namespace Data
{

    public class Room
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
    public static class RoomsData
    {
        public static List<Room> GetRooms()
        {
            return new List<Room>
            {
                new Room { Name = "Slater003", Capacity = 45 },
                new Room { Name = "Roman216", Capacity = 30 },
                new Room { Name = "Loft206", Capacity = 75 },
                new Room { Name = "Roman201", Capacity = 50 },
                new Room { Name = "Loft310", Capacity = 108 },
                new Room { Name = "Beach201", Capacity = 60 },
                new Room { Name = "Beach301", Capacity = 75 },
                new Room { Name = "Logos325", Capacity = 450 },
                new Room { Name = "Frank119", Capacity = 60 },
            };
        }
    }
}
