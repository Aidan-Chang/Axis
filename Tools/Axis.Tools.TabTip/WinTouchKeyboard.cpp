#include "WinTouchKeyboard.h"

#include <Objbase.h>
//#include <atlbase.h>
#include <initguid.h>
//#include <tlhelp32.h>
//#include <windows.h>
#include <Shobjidl.h>

//#include <algorithm>
//#include <experimental/filesystem>
#include <iostream>
//#include <sstream>
//#include <string>

//#pragma hdrstop
//#pragma warning(disable : 4996)

using namespace std;

// 4ce576fa-83dc-4F88-951c-9d0782b4e376
DEFINE_GUID(CLSID_UIHostNoLaunch, 0x4CE576FA, 0x83DC, 0x4f88, 0x95, 0x1C, 0x9D, 0x07, 0x82, 0xB4, 0xE3, 0x76);

// 37c994e7_432b_4834_a2f7_dce1f13b834b
DEFINE_GUID(IID_ITipInvocation, 0x37c994e7, 0x432b, 0x4834, 0xa2, 0xf7, 0xdc, 0xe1, 0xf1, 0x3b, 0x83, 0x4b);

struct ITipInvocation : IUnknown {
  virtual HRESULT STDMETHODCALLTYPE Toggle(HWND wnd) = 0;
};

//namespace {
//  const char* KeyboardWindowClass = "IPTip_Main_Window";
//  const char* WindowParentClass = "ApplicationFrameWindow";
//  const char* WindowClass = "Windows.UI.Core.CoreWindow";
//  const char* WindowCaption = "Microsoft Text Input Application";
//}

WinTouchKeyboard& WinTouchKeyboard::Instance() {
  static WinTouchKeyboard instance;
  return instance;
}

bool WinTouchKeyboard::OpenScreenKeyboard() { return OpenTabTip(); }

//bool WinTouchKeyboard::OpenOSK() {
//  PVOID oldValue = NULL;
//  BOOL bRet = Wow64DisableWow64FsRedirection(&oldValue);
//  ShellExecuteA(NULL, "open", "osk.exe", NULL, NULL, SW_HIDE);
//  if (bRet) {
//    Wow64RevertWow64FsRedirection(oldValue);
//    return true;
//  }
//  else {
//    return false;
//  }
//}

bool WinTouchKeyboard::OpenTabTip() {
  //if (IsWin10KeyboardVisable() || IsWin7KeyboardVisable()) {
  //  return true;
  //}
  //string tabTipPath = "C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe";
  //error_code err;
  //if (!experimental::filesystem::exists(tabTipPath, err)) {
  //  return false;
  //}
  //if (IsNewVersion()) {
  //  int pid = 0;
  //  pid = FindTabTipProcess();
  //  if (pid == 0) {
  //    ShellExecuteA(NULL, "open", tabTipPath.c_str(), NULL, NULL, SW_HIDE);
  //    Sleep(600);
  //  }
  //  //if (IsWin10KeyboardVisable() || IsWin7KeyboardVisable()) {
  //  //  return true;
  //  //}
  //  CoInitialize(nullptr);
  //  CComPtr<ITipInvocation> _tip;
  //  CoCreateInstance(CLSID_UIHostNoLaunch, 0,
  //    CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER,
  //    IID_ITipInvocation, (void**)&_tip);
  //  if (!_tip) {
  //    CoUninitialize();
  //    return false;
  //  }
  //  else {
  //    _tip->Toggle(GetDesktopWindow());
  //  }
  //}
  //else {
  //  ShellExecuteA(NULL, "open", tabTipPath.c_str(), NULL, NULL, SW_HIDE);
  //}
  //CoUninitialize();
  //return true;
  HRESULT hr{ CoInitialize(NULL) };
  if (FAILED(hr)) {
    wcerr << L"Failed to initialize COM." << endl;
    return false;
  }
  if (IsInputPaneOpen()) {
    return true;
  }
  ITipInvocation* tip{ nullptr };
  hr = CoCreateInstance(CLSID_UIHostNoLaunch, NULL, CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER, IID_ITipInvocation, (void**)&tip);
  if (hr == REGDB_E_CLASSNOTREG) {
    INT_PTR result = (INT_PTR)ShellExecuteW(NULL, NULL, L"C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe", NULL, NULL, SW_SHOWNORMAL);
    if (result > 32) {
      wcout << L"Started TabTip.exe to open Touch Keyboard." << endl;
    }
    else {
      wcerr << L"Failed to start TabTip.exe. Error: " << result << endl;
    }
  }
  else if (SUCCEEDED(hr)) {
    HWND desktopWindow = GetDesktopWindow();
    hr = tip->Toggle(desktopWindow);
    if (SUCCEEDED(hr)) {
      wcout << L"Toggled the touch keyboard via ITipInvocation.Toggle()." << endl;
    }
    else {
      wcerr << L"Failed to toggle the Touch Keyboard via ITipInvocation.Toggle()." << endl;
    }
    tip->Release();
  }
  CoUninitialize();
  return true;
}

//bool WinTouchKeyboard::IsWin10KeyboardVisable() {
//  HWND parent = FindWindowExA(NULL, NULL, WindowParentClass, NULL);
//  if (!parent) {
//    return false;
//  }
//  HWND wnd = FindWindowExA(parent, NULL, WindowClass, WindowCaption);
//  if (!wnd) {
//    return false;
//  }
//  return true;
//}
//
//bool WinTouchKeyboard::IsWin7KeyboardVisable() {
//  HWND touchhWnd = FindWindowA(KeyboardWindowClass, NULL);
//  if (!touchhWnd) {
//    return false;
//  }
//  unsigned long style = GetWindowLong(touchhWnd, GWL_STYLE);
//  return (style & WS_CLIPSIBLINGS) && (style & WS_VISIBLE) && (style & WS_POPUP) && !(style & WS_DISABLED);
//}

//bool WinTouchKeyboard::IsNewVersion() {
//  string version = GetCurrentVersion();
//  auto versionList = Split(version, ".");
//  int compareVersion[] = { 10, 0, 14393 };
//  for (int i = 0; i < versionList.size(); ++i) {
//    switch (i) {
//    case 0:
//    case 1:
//    case 2:
//      if (stoi(versionList[i]) > compareVersion[i]) {
//        return true;
//      }
//      else if (stoi(versionList[i]) < compareVersion[i]) {
//        return false;
//      }
//      break;
//    case 3:
//      return true;
//    }
//  }
//  return true;
//}

//string WinTouchKeyboard::GetCurrentVersion() {
//  stringstream version;
//  RTL_OSVERSIONINFOW osv;
//  osv.dwOSVersionInfoSize = sizeof(osv);
//  NTSTATUS(WINAPI * pRtlGetVersion)(PRTL_OSVERSIONINFOW) = NULL;
//  HMODULE hMod = GetModuleHandleW(L"ntdll.dll");
//  if (hMod != NULL) {
//    pRtlGetVersion = (NTSTATUS(WINAPI*)(PRTL_OSVERSIONINFOW))GetProcAddress(hMod, "RtlGetVersion");
//    if (pRtlGetVersion != nullptr) {
//      pRtlGetVersion(&osv);
//      version << osv.dwMajorVersion << "." << osv.dwMinorVersion << "." << osv.dwBuildNumber;
//    }
//  }
//  if (pRtlGetVersion == NULL) {
//    if (GetVersionExW(&osv)) {
//      version << osv.dwMajorVersion << "." << osv.dwMinorVersion << "." << osv.dwBuildNumber;
//    }
//  }
//  return version.str();
//}

//vector<string> WinTouchKeyboard::Split(string text, string delimeter) {
//  vector<string> splittedString;
//  int startIndex = 0;
//  int endIndex = 0;
//  while ((endIndex = text.find(delimeter, startIndex)) < text.size()) {
//    string val = text.substr(startIndex, endIndex - startIndex);
//    splittedString.push_back(val);
//    startIndex = endIndex + delimeter.size();
//  }
//  if (startIndex < text.size()) {
//    string val = text.substr(startIndex);
//    splittedString.push_back(val);
//  }
//  return splittedString;
//}

//int WinTouchKeyboard::FindTabTipProcess() {
//  const wchar_t* name = L"TabTip.exe";
//  HANDLE snapshot;
//  snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
//  if (INVALID_HANDLE_VALUE == snapshot) {
//    return 0;
//  }
//  PROCESSENTRY32W process;
//  ZeroMemory(&process, sizeof(process));
//  process.dwSize = sizeof(process);
//  BOOL hResult;
//  hResult = Process32FirstW(snapshot, &process);
//  int pid = 0;
//  while (hResult) {
//    if (_wcsicmp(process.szExeFile, name) == 0) {
//      pid = process.th32ProcessID;
//      break;
//    }
//    hResult = Process32NextW(snapshot, &process);
//  }
//  CloseHandle(snapshot);
//  return pid;
//}

bool WinTouchKeyboard::IsInputPaneOpen() {
  RECT rect;
  ZeroMemory(&rect, sizeof(rect));
  IFrameworkInputPane* frameworkInputPane{ nullptr };
  HRESULT hr{ CoCreateInstance(CLSID_FrameworkInputPane, NULL, CLSCTX_INPROC_SERVER, IID_IFrameworkInputPane, (void**)&frameworkInputPane) };
  if (SUCCEEDED(hr)) {
    hr = frameworkInputPane->Location(&rect);
    if (SUCCEEDED(hr)) {
      hr = IsRectEmpty(&rect) ? S_FALSE : S_OK;
    }
    frameworkInputPane->Release();
  }
  if (hr == S_OK)
    return true;
  return false;
}