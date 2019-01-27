using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Farazpardazan.ParkingBot.Parking
{
    public class ParkingLottery
    {
        private const string DatabaseName = "PARKING_DATA";
        private readonly Database _database;

        public ParkingLottery(Database database)
        {
            _database = database;
            if (_database.GetData<ParkingLotteryData>(DatabaseName) == null)
            {
                _database.SetData(DatabaseName, new ParkingLotteryData());
            }
        }

        public async Task AddVolunteer(string id, string name)
        {
            var db = _database.GetData<ParkingLotteryData>(DatabaseName);
            var volunteer = db.Volunteers.FirstOrDefault(x => x.Id == id);
            if (volunteer == null)
            {
                volunteer = new Volunteer
                {
                    AddTime = DateTime.Now,
                    Id = id,
                    WonCount = db.Volunteers.Count > 0 ? db.Volunteers.Min(x => x.WonCount) : 0

                };
                db.Volunteers.Add(volunteer);
            }

            volunteer.Name = name;
            _database.SetData(DatabaseName,db);
            await _database.Save();
        }

        public Task<IEnumerable<Lottery>> GetLotteries()
        {
            var db = _database.GetData<ParkingLotteryData>(DatabaseName);
            return Task.FromResult<IEnumerable<Lottery>>(db.Lotteries.ToImmutableArray());
        }

        public Task<IEnumerable<Volunteer>> GetCurrentParticipatingVolunteers()
        {
            var db = _database.GetData<ParkingLotteryData>(DatabaseName);
            var minWonCount = db.Volunteers.Count > 0 ? db.Volunteers.Min(x => x.WonCount) : 0;
            return Task.FromResult(db.Volunteers
                .Where(x => x.WonCount == minWonCount));
        }

        public async Task<Volunteer> Draw()
        {
            var participatingVolunteers = (await GetCurrentParticipatingVolunteers()).ToList();
            var random = new Random((int)DateTime.Now.Ticks);
            var winner = participatingVolunteers[random.Next(participatingVolunteers.Count)];
            winner.WonCount++;

            var db = _database.GetData<ParkingLotteryData>(DatabaseName);
            db.Lotteries.Add(new Lottery
            {
                WonCount = winner.WonCount,
                Time = DateTime.Now,
                WinnerId = winner.Id,
                WinnerName = winner.Name
            });
            _database.SetData(DatabaseName,db);
            await _database.Save();

            return winner;
        }
    }
}