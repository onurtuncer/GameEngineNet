#pragma once
#include <cstdint>

#if defined(_WIN32)
#define HZB_EXPORT __declspec(dllexport)
#define HZB_CALL __cdecl
#else
#define HZB_EXPORT __attribute__((visibility("default")))
#define HZB_CALL
#endif

extern "C" {

	// level: spdlog level enum as int (trace=0, debug=1, info=2, warn=3, err=4, critical=5, off=6)
	typedef void (HZB_CALL* HzLogCallback)(int level, const char* utf8);

	HZB_EXPORT void HZB_CALL hz_init();                         // init Hazel logger(s)
	HZB_EXPORT void HZB_CALL hz_set_log_callback(HzLogCallback cb); // set managed callback
	HZB_EXPORT void HZB_CALL hz_demo_log();                     // send a few test messages
	HZB_EXPORT void HZB_CALL hz_shutdown();                     // optional cleanup

}
