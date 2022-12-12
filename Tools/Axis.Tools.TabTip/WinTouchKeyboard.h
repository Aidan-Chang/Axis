#pragma once

using namespace std;

class WinTouchKeyboard {
  public:
    static WinTouchKeyboard& Instance();
    bool OpenScreenKeyboard();
  private:
    WinTouchKeyboard() = default;
    ~WinTouchKeyboard() = default;
    bool OpenTabTip();
    bool IsInputPaneOpen();
};