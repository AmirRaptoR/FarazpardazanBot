using System;

namespace Farazpardazan.ParkingBot.Parking
{
    public class Volunteer
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int WonCount { get; set; }
        public DateTime AddTime { get; set; }
        public bool IsActive { get; set; }
    }
}