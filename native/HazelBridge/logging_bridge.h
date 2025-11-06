#pragma once
#include <memory>
#include <spdlog/spdlog.h>
#include <spdlog/sinks/base_sink.h>
#include <mutex>

class ManagedCallbackSink : public spdlog::sinks::base_sink<std::mutex> {
public:
    using Callback = void(*)(int, const char*);
    explicit ManagedCallbackSink(Callback cb) : callback_(cb) {}
    void set_callback(Callback cb) { callback_ = cb; }

protected:
    void sink_it_(const spdlog::details::log_msg& msg) override;
    void flush_() override {}

private:
    Callback callback_{ nullptr };
    std::string buffer_;
};
