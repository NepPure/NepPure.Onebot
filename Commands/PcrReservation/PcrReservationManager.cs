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
        private const string DATA_PATH = "data/PcrReservation";
        private const string FILE_NAME = "data.json";
        private static readonly string FilePath = Path.Combine(DATA_PATH, FILE_NAME);
        static PcrReservationManager()
        {
            lock (_lock)
            {
                if (!Directory.Exists(DATA_PATH))
                {
                    Directory.CreateDirectory(DATA_PATH);
                }

                if (File.Exists(FilePath))
                {
                    var sourceStr = File.ReadAllText(FilePath);
                    _data = JsonConvert.DeserializeObject<ConcurrentDictionary<long, ConcurrentQueue<PcrReservationModel>>>(sourceStr);
                }
                else
                {
                    File.WriteAllText(FilePath, "");
                    _data = new ConcurrentDictionary<long, ConcurrentQueue<PcrReservationModel>>();
                }
            }
        }

        private static void DataSync()
        {
            lock (_lock)
            {
                var sourceStr = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(FilePath, sourceStr);
            }
        }

        public static void Enqueue(long groupId, PcrReservationModel user)
        {
            _data.AddOrUpdate(groupId, key =>
            {
                var val = new ConcurrentQueue<PcrReservationModel>();
                val.Enqueue(user);
                return val;
            },
            (key, val) =>
            {
                //此处允许重复预约
                val.Enqueue(user);
                return val;
            });

            DataSync();
            return;
        }

        public static PcrReservationModel Peek(long groupId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return null;
            }

            while (true)
            {
                if (_data[groupId].TryPeek(out PcrReservationModel result))
                {
                    //队列里有
                    if (result.IsCancel)
                    {
                        //已取消，移除队列
                        _data[groupId].TryDequeue(out PcrReservationModel _);
                    }
                    else
                    {
                        // 没取消返回
                        return result;
                    }
                }
                else
                {
                    //队列里没有
                    return null;
                }
            }
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

            return _data[groupId].Where(m => m.IsCancel == false).ToList();
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

        public static PcrReservationModel SetCancel(long groupId, long userId)
        {
            if (!_data.ContainsKey(groupId))
            {
                return null;
            }

            var target = _data[groupId]
                   .Where(m => m.UserId == userId)
                   .Where(m => m.IsCancel == false)
                   .OrderBy(m => m.ReserveTime)
                   .FirstOrDefault();

            if (target != null)
            {
                target.IsCancel = true;
            }
            return target;
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
