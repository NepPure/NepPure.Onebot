using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepPure.Onebot.Commands.PcrReservation
{
    public static class PcrReservationManager
    {
        private static readonly object _lock = new();
        private static ConcurrentDictionary<long, ConcurrentQueue<PcrReservationModel>> _data;
        private const string DATA_PATH = "data/PcrReservation/PcrReservation.json";

        static PcrReservationManager()
        {
            lock (_lock)
            {
                if (File.Exists(DATA_PATH))
                {
                    var sourceStr = File.ReadAllText(DATA_PATH);
                    _data = JsonConvert.DeserializeObject<ConcurrentDictionary<long, ConcurrentQueue<PcrReservationModel>>>(sourceStr);
                }
                else
                {
                    _data = new ConcurrentDictionary<long, ConcurrentQueue<PcrReservationModel>>();
                }
            }
        }

        private static void DataSync()
        {
            lock (_lock)
            {
                var sourceStr = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(DATA_PATH, sourceStr);
            }
        }

        public static bool Enqueue(long groupId, PcrReservationModel user)
        {
            bool alreadyExist = false;

            _data.AddOrUpdate(groupId, key =>
            {
                var val = new ConcurrentQueue<PcrReservationModel>();
                val.Enqueue(user);
                return val;
            },
            (key, val) =>
            {
                if (val.Any(m => m.UserId == user.UserId))
                {
                    alreadyExist = true;
                }
                else
                {
                    val.Enqueue(user);
                }

                return val;
            });

            if (alreadyExist)
            {
                return false;
            }

            DataSync();
            return true;
        }

        public static PcrReservationModel Peek(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return null;
            }

            if (_data[groupId].TryPeek(out PcrReservationModel result))
            {
                return result;
            }

            return null;
        }

        public static List<PcrReservationModel> PeekAll(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return new List<PcrReservationModel>();
            }

            if (_data[groupId].IsEmpty)
            {
                return new List<PcrReservationModel>();
            }

            return _data[groupId].ToList();
        }

        public static PcrReservationModel Dequeue(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return null;
            }

            if (_data[groupId].TryDequeue(out PcrReservationModel result))
            {
                DataSync();
                return result;
            }

            return null;
        }

        public static int GetQueueLength(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return 0;
            }

            return _data[groupId].Count;
        }

        public static void ClearQueue(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return;
            }

            if (_data[groupId].IsEmpty)
            {
                return;
            }

            _data[groupId].Clear();
            DataSync();
        }
    }
}
