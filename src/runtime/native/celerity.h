#pragma once

#include <assert.h>
#include <stddef.h>
#include <stdint.h>

#define nonnull _Nonnull
#define nullable _Nullable

#if defined(ZIG_OS_WINDOWS)
#   define CELERITY_API [[gnu::dllexport]]
#else
#   define CELERITY_API [[gnu::visibility("default")]]
#endif
