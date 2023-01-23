#pragma once

#include <stddef.h>
#include <stdint.h>

#define nonnull _Nonnull
#define nullable _Nullable

#if defined(_WIN32)
#   define CELERITY_API __declspec(dllexport)
#else
#   define CELERITY_API __attribute__((__visibility__("default")))
#endif
