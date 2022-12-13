#include "framework.h"
#include "WinTouchKeyboard.h"

int APIENTRY wWinMain(
  _In_ HINSTANCE hInstance,
  _In_opt_ HINSTANCE hPrevInstance,
  _In_ LPWSTR    lpCmdLine,
  _In_ int       nCmdShow) {
  UNREFERENCED_PARAMETER(hPrevInstance);
  UNREFERENCED_PARAMETER(lpCmdLine);
  WinTouchKeyboard& wk = WinTouchKeyboard::Instance();
  bool status = wk.OpenScreenKeyboard();
  return 0;
}