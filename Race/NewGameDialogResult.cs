using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Race
{
    public class NewGameDialogResult : PropertyChangedAware
    {
        public ObservableCollection<Car> Cars { get; set; } = new ObservableCollection<Car>(Car.DefaultCars);

        private string _trackFileName = "Tracks/Track1.svg";
        public string TrackFileName
        {
            get { return _trackFileName; }
            set { SetProperty(ref _trackFileName, value); }
        }

        private RaceDirection _raceDirection = RaceDirection.Counterclockwise;
        public RaceDirection RaceDirection
        {
            get { return _raceDirection; }
            set { SetProperty(ref _raceDirection, value); }
        }

        private byte[] _trackSum = new byte[16];
        public byte[] TrackSum
        {
            get { return _trackSum; }
            set { SetProperty(ref _trackSum, value); }
        }
    }
}
