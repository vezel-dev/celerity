#pragma once

#include <assert.h>
#include <stddef.h>
#include <stdint.h>

#define nonnull _Nonnull
#define nullable _Nullable
#define nullptr NULL

#if defined(_WIN32)
#   define CELERITY_API [[gnu::dllexport]]
#else
#   define CELERITY_API [[gnu::visibility("default")]]
#endif
