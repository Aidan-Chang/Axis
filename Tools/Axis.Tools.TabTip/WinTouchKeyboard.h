#pragma once

#include <string>
#include <vector>

using namespace std;

class WinTouchKeyboard {
  public:
    static WinTouchKeyboard& Instance();
    bool OpenScreenKeyboard();
  private:
    WinTouchKeyboard() = default;
    ~WinTouchKeyboard() = default;
    bool OpenOSK();
    bool OpenTabTip();
    bool IsWin10KeyboardVisable();
    bool IsWin7KeyboardVisable();
    bool IsNewVersion();
    vector<string> Split(string text, string delimeter);
    string GetCurrentVersion();
    int FindTabTipProcess();
};