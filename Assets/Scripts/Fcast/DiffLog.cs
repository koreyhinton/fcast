using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Fcast
{
    internal class Log
    {
        private string _log { get; set; } = null;
        public Log(string log)
        {
            _log = log;
        }
        public override string ToString()
        {
            return _log.Replace(Environment.NewLine, "{nl}");
        }
    }
    public enum LogAction
    {
        Write,
        Flush
    }
    public class DiffLog : IExec
    {
        public static bool DebugMode { get; set; } = false;
        public LogAction Action { get; set; } = LogAction.Write;
        public string Key { get; set; } = null;
        public string Value { get; set; } = null;
        private List<Log> _unflushedLogs = new List<Log>();
        private Dictionary<string, int> _hashes { get; set; }
            = new Dictionary<string, int>();
        public void Exec()
        {
            if (Action == LogAction.Flush)
            {
                if (_unflushedLogs.Count == 0)
                    return;
                var cumulativeLog = "";
                foreach (var log in _unflushedLogs)
                {
                    cumulativeLog += ""+log+Environment.NewLine;
                }
                if (DebugMode)
                    Debug.Log(cumulativeLog);
                _unflushedLogs.Clear();
                return;
            }
            bool lookupFound = (Key != null &&
                Value != null &&
                _hashes.ContainsKey(Key) &&
                _hashes[Key] == Value.GetHashCode());
            if (lookupFound)
                return;
            _hashes[Key] = Value.GetHashCode();
            _unflushedLogs.Add(new Log(Key + ": " + Value));
        }
    }
}
