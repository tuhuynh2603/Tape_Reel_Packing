#pragma once

//#include <string>
//#include <vector>

using namespace std;
#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)

// CEUApp
// See EU.cpp for the implementation of this class
//

EXTERN_DLL_EXPORT void OutputDebugLogString(LPCTSTR lpOutputString);
EXTERN_DLL_EXPORT void OutputDebugLogRect(LPCTSTR lpOutputString, CRect& rect);
EXTERN_DLL_EXPORT void OutputDebugLogPoint(LPCTSTR lpOutputString, CPoint& point);
EXTERN_DLL_EXPORT void OutputDebugLog(LPSTR lpFormat, ...);
EXTERN_DLL_EXPORT void OutputDebugLogStringTo(LPCTSTR lpOutputString, BOOL bTimeStamp = TRUE, int nIdx = 0);
EXTERN_DLL_EXPORT  void OutputDebugLogRectTo(int nIdx, BOOL bTimeStamp, LPCTSTR lpOutputString, CRect& rect);
EXTERN_DLL_EXPORT void OutputDebugLogPointTo(int nIdx, BOOL bTimeStamp, LPCTSTR lpOutputString, CPoint& point);
EXTERN_DLL_EXPORT void OutputDebugLogTo(int nIdx, BOOL bTimeStamp, LPSTR lpFormat, ...);
EXTERN_DLL_EXPORT void OutputDebugLogClear(int nIdx);
EXTERN_DLL_EXPORT void OutputDebugLogClearAll();
EXTERN_DLL_EXPORT void OutputDebugLogToFile(BOOL bState);
EXTERN_DLL_EXPORT void LogMessage(int nTab, LPCTSTR pMessageText, ...);
EXTERN_DLL_EXPORT void LogDebugMessage(LPCTSTR pMessageText, BYTE* pBitmap, int nWidth, int nHeight, int nPitch, int nBitCount, int nTab, int nInfoType);