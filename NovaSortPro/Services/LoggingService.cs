using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NovaSortPro.Services
{
    public class LoggingService
    {
        private readonly List<string> _inMemoryLogs = new();
        private readonly string _logFilePath;

        public LoggingService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(appData, "NovaSortPro");
            Directory.CreateDirectory(appDir);
            _logFilePath = Path.Combine(appDir, "app_execution.log");
        }

        public void Log(string message, string level = "INFO")
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (_inMemoryLogs)
            {
                _inMemoryLogs.Add(logEntry);
            }

            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Fallback if file is locked
            }
        }

        public List<string> GetLogs()
        {
            lock (_inMemoryLogs)
            {
                return new List<string>(_inMemoryLogs);
            }
        }

        public string GetLogFilePath() => _logFilePath;

        public void ExportToTxt(string targetPath)
        {
            try
            {
                File.Copy(_logFilePath, targetPath, overwrite: true);
            }
            catch (Exception ex)
            {
                Log($"Failed to export TXT logs: {ex.Message}", "ERROR");
                throw;
            }
        }

        public void ExportToCsv(string targetPath)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Timestamp,Level,Message");

                var lines = File.Exists(_logFilePath) ? File.ReadAllLines(_logFilePath) : Array.Empty<string>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // Parse: [Timestamp] [Level] Message
                    try
                    {
                        var parts = line.Split(']', 3);
                        if (parts.Length >= 3)
                        {
                            var time = parts[0].TrimStart('[');
                            var level = parts[1].Trim('[').Trim();
                            var msg = parts[2].Trim();
                            
                            // Escape quotes for CSV
                            msg = msg.Replace("\"", "\"\"");
                            sb.AppendLine($"\"{time}\",\"{level}\",\"{msg}\"");
                        }
                    }
                    catch
                    {
                        // Write raw line if parsing fails
                        sb.AppendLine($"\"\", \"\", \"{line.Replace("\"", "\"\"")}\"");
                    }
                }

                File.WriteAllText(targetPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log($"Failed to export CSV logs: {ex.Message}", "ERROR");
                throw;
            }
        }

        public void ExportToPdf(string targetPath)
        {
            // Since we target native offline offline with no bulky NuGets,
            // we will write a beautiful styled HTML report that the user can print to PDF
            // or view as a high-fidelity document.
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<head>");
                sb.AppendLine("<title>NovaSort Pro Operation Log</title>");
                sb.AppendLine("<style>");
                sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; background-color: #f9f9f9; color: #333; }");
                sb.AppendLine("h1 { color: #0078d4; border-bottom: 2px solid #0078d4; padding-bottom: 10px; }");
                sb.AppendLine(".meta { font-size: 0.9em; color: #666; margin-bottom: 20px; }");
                sb.AppendLine("table { width: 100%; border-collapse: collapse; background-color: #fff; margin-top: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
                sb.AppendLine("th, td { padding: 12px; border: 1px solid #ddd; text-align: left; }");
                sb.AppendLine("th { background-color: #0078d4; color: white; }");
                sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
                sb.AppendLine(".INFO { color: #0078d4; font-weight: bold; }");
                sb.AppendLine(".WARN { color: #b77c00; font-weight: bold; }");
                sb.AppendLine(".ERROR { color: #d13438; font-weight: bold; }");
                sb.AppendLine("</style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                sb.AppendLine("<h1>NovaSort Pro - Operation Log Report</h1>");
                sb.AppendLine($"<div class='meta'>Generated on: {DateTime.Now} | Scope: Local Archive</div>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th style='width: 20%;'>Timestamp</th><th style='width: 15%;'>Level</th><th>Message</th></tr>");

                var lines = File.Exists(_logFilePath) ? File.ReadAllLines(_logFilePath) : Array.Empty<string>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var parts = line.Split(']', 3);
                        if (parts.Length >= 3)
                        {
                            var time = parts[0].TrimStart('[');
                            var level = parts[1].Trim('[').Trim();
                            var msg = parts[2].Trim();

                            sb.AppendLine($"<tr><td>{time}</td><td class='{level}'>{level}</td><td>{msg}</td></tr>");
                        }
                    }
                    catch
                    {
                        sb.AppendLine($"<tr><td>-</td><td>RAW</td><td>{line}</td></tr>");
                    }
                }

                sb.AppendLine("</table>");
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");

                File.WriteAllText(targetPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log($"Failed to export PDF/HTML logs: {ex.Message}", "ERROR");
                throw;
            }
        }
    }
}
