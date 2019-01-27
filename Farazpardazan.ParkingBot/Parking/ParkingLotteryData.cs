using System.Collections.Generic;

namespace Farazpardazan.ParkingBot.Parking
{
    public class ParkingLotteryData
    {
        public ICollection<Volunteer> Volunteers { get; set; } = new List<Volunteer>();
        public ICollection<Lottery> Lotteries { get; set; } = new List<Lottery>();
    }
}