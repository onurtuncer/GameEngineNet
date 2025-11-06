using System;
using System.Runtime.InteropServices;
using GameHost.Logging;

namespace GameHost.Interop;

internal static class HazelNative
{
	private const string Dll = "HazelBridge"; // HazelBridge.dll must be next to GameHost.exe

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void LogCallback(int level, IntPtr utf8Msg);

	[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
	private static extern void hz_init();

	[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
	private static extern void hz_set_log_callback(LogCallback cb);

	[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
	private static extern void hz_demo_log();

	[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
	private static extern void hz_shutdown();

	private static LogCallback? _cbKeepAlive; // prevent GC
	private static NativeLogSink? _sink;

	public static void Initialize(NativeLogSink sink)
	{
		_sink = sink;
		_cbKeepAlive = OnNativeLog;
		hz_init();
		hz_set_log_callback(_cbKeepAlive);
	}

	public static void DemoLog() => hz_demo_log();

	public static void Shutdown() => hz_shutdown();

	private static void OnNativeLog(int level, IntPtr msg)
	{
		var text = NativeLogSink.Utf8(msg);
		// Marshal to UI thread if needed
		if (App.Current?.Dispatcher is { } d && !d.CheckAccess())
		{
			d.Invoke(() => _sink?.Add(level, text));
		}
		else
		{
			_sink?.Add(level, text);
		}
	}
}
