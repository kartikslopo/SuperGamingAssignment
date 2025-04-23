using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace IpBroker.Models
{
    public class ProviderStats
    {
        private readonly object _lock = new();
        private readonly List<(DateTime Timestamp, bool IsError, long ResponseTimeMs)> _responses = new();
        private int _requestCountLastMinute;
        private readonly Random _random = new Random();

        public string ProviderName { get; }
        public Uri BaseUrl { get; }
        public int MaxRequestsPerMinute { get; }
        public string EndpointFormat { get; }
        public int ArtificialDelayMs { get; }
        public int SimulateErrorRate { get; } // 0-100 percentage

        public ProviderStats(string providerName, string baseUrl, int maxRequestsPerMinute,
                            string endpointFormat, int artificialDelayMs = 0, int simulateErrorRate = 0)
        {
            ProviderName = providerName;
            BaseUrl = new Uri(baseUrl);
            MaxRequestsPerMinute = maxRequestsPerMinute;
            EndpointFormat = endpointFormat;
            ArtificialDelayMs = artificialDelayMs;
            SimulateErrorRate = Math.Clamp(simulateErrorRate, 0, 100);
        }

        public void RecordResponse(bool isError, long responseTimeMs)
        {
            lock (_lock)
            {
                _responses.Add((DateTime.UtcNow, isError, responseTimeMs));
                _requestCountLastMinute++;
                CleanupOldRecords();
            }
        }

        public bool CanAcceptRequest()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                return _requestCountLastMinute < MaxRequestsPerMinute;
            }
        }

        public bool ShouldSimulateError()
        {
            return _random.Next(100) < SimulateErrorRate;
        }

        public double GetAvgResponseTimeLast5Min()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                var last5Min = DateTime.UtcNow.AddMinutes(-5);
                var items = _responses.Where(x => x.Timestamp >= last5Min).ToList();
                return items.Count > 0 ? items.Average(x => x.ResponseTimeMs) : double.MaxValue;
            }
        }

        public int GetErrorCountLast5Min()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                var last5Min = DateTime.UtcNow.AddMinutes(-5);
                return _responses.Count(x => x.Timestamp >= last5Min && x.IsError);
            }
        }

        public int GetRequestCountLastMinute()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                return _requestCountLastMinute;
            }
        }

        public int GetSuccessCountLastMinute()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                return _responses.Count(x => x.Timestamp >= oneMinuteAgo && !x.IsError);
            }
        }

        public double GetErrorRateLastMinute()
        {
            lock (_lock)
            {
                CleanupOldRecords();
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentRequests = _responses.Where(x => x.Timestamp >= oneMinuteAgo).ToList();

                if (recentRequests.Count == 0)
                    return 0;

                return (double)recentRequests.Count(x => x.IsError) / recentRequests.Count;
            }
        }

        private void CleanupOldRecords()
        {
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            _responses.RemoveAll(x => x.Timestamp < fiveMinutesAgo);
            _requestCountLastMinute = _responses.Count(x => x.Timestamp >= oneMinuteAgo);
        }
    }
}