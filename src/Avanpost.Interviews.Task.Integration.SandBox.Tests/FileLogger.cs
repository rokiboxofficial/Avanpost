using Avanpost.Interviews.Task.Integration.Data.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Tests
{
    public sealed class FileLogger : ILogger
    {
        private string _fileName;
        private string _connectorName;

        public FileLogger(string fileName, string connectorName)
        {
            _fileName = fileName;
            _connectorName = connectorName;
        }

        public void Debug(string message) => Append($"{DateTime.Now}:{_connectorName}:DEBUG:{message}");

        public void Error(string message) => Append($"{DateTime.Now}:{_connectorName}:ERROR:{message}");

        public void Warn(string message) => Append($"{DateTime.Now}:{_connectorName}:WARNING{message}");

        private void Append(string text)
        {
            using var sw = File.AppendText(_fileName);
            sw.WriteLine(text);
        }
    }
}