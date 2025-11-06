using System;
using System.Collections.ObjectModel;
using System.Text;

namespace GameHost.Logging;

public sealed class NativeLogSink
{
    public ObservableCollection<LogEntry> Entries { get; } = new();

    public void Add(int level, string msg)
    {
        // You can parse "[HH:MM:SS] [LEVEL] LOGGER: message" if you keep Hazel pattern
        // Here we just split a little to fill Logger/Level if present.
        var levelName = level switch
        {
            0 => "Trace",
            1 => "Debug",
            2 => "Info",
            3 => "Warn",
            4 => "Error",
            5 => "Critical",
            6 => "Off",
            _ => "Info"
        };

        Entries.Add(new LogEntry
        {
            Level = levelName,
            Message = msg
        });
    }

    public static string Utf8(IntPtr p)
    {
        if (p == IntPtr.Zero) return string.Empty;
        // Compute length up to null
        int len = 0;
        while (System.Runtime.InteropServices.Marshal.ReadByte(p, len) != 0) len++;
        var buf = new byte[len];
        System.Runtime.InteropServices.Marshal.Copy(p, buf, 0, len);
        return Encoding.UTF8.GetString(buf);
    }
}
