#include <Hazel.h>                  // whatever Hazel’s umbrella header is
#include "hazel_bridge.h"           // export macros
#include <memory>

static std::unique_ptr<Hazel::Application> g_App;

extern "C" {

    HZB_EXPORT void HZB_CALL hz_init()
    {
        // Initialize Hazel logging once
        Hazel::Log::Init();
        HZ_CORE_WARN("Hazel logging initialized (from C# host)");

        // Create application instance (user-defined factory)
        g_App.reset(Hazel::CreateApplication());
        HZ_INFO("Hazel application created");
    }

    HZB_EXPORT void HZB_CALL hz_run()
    {
        if (!g_App)
        {
            HZ_CORE_ERROR("Hazel app not initialized, call hz_init() first!");
            return;
        }

        try
        {
            HZ_INFO("Running Hazel app loop");
            g_App->Run();
        }
        catch (const std::exception& e)
        {
            HZ_CORE_CRITICAL("Unhandled exception in Hazel::Run(): {}", e.what());
        }
    }

    HZB_EXPORT void HZB_CALL hz_shutdown()
    {
        HZ_INFO("Shutting down Hazel app");
        g_App.reset();
        spdlog::shutdown();
    }

} // extern "C"
