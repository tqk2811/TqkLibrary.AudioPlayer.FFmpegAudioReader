#ifndef _H_platform_H_
#define _H_platform_H_

// Cross-platform export macro. Consumers (managed code) never include this
// header, so we only ever need the export side.
#ifdef _WIN32
#define DLL_EXPORT extern "C" __declspec(dllexport)
#else
#define DLL_EXPORT extern "C" __attribute__((visibility("default")))
#endif

#endif // _H_platform_H_
