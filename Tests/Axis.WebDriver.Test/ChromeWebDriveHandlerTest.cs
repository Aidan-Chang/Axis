using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Axis.WebDriver.Test;

public class ChromeWebDriveHandlerTest {

  [Fact]
  public void Vote_And_Submit_To_Web() {
    while (true) {
      new DriverManager().SetUpDriver(new ChromeConfig());
      var driver = new ChromeDriver();
      driver.Navigate().GoToUrl("https://www.surveycake.com/s/RP2oM?fbclid=IwAR1NCiycaVSZcNUUyS5gUrrxzQm1EZ3V-q40Ya0giprRcjApiApDjFYg240");
      bool[] found = { false, false, false, false, false, false, false, false, false, false };
      // 开始
      while (!found[0]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1fg2ctd"));
          button.Click();
          found[0] = true;
        }
        catch { }
      }
      // A 组
      while (!found[1]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1hbugce"));
          button.Click();
          found[1] = true;
        }
        catch { }
      }
      // B 组
      while (!found[2]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1hbugce"));
          button.Click();
          found[2] = true;
        }
        catch { }
      }
      // C 组
      while (!found[3]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement radio1;
        IWebElement radio2;
        IWebElement button;
        try {
          radio1 = driver.FindElements(By.ClassName("css-3lsiao"))[4];
          radio2 = driver.FindElements(By.ClassName("css-3lsiao"))[5];
          new Actions(driver).ScrollToElement(radio2).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));
          radio1.Click();

          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1hbugce"));
          Thread.Sleep(new Random().Next(1000, 2500));
          button.Click();
          found[3] = true;
        }
        catch { }
      }
      // D 组
      while (!found[4]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1hbugce"));
          button.Click();
          found[4] = true;
        }
        catch { }
      }
      // E 组
      while (!found[5]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1hbugce"));
          button.Click();
          found[5] = true;
        }
        catch { }
      }
      // 送出
      while (!found[6]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1fmh1mk"));
          button.Click();
          found[6] = true;
        }
        catch { }
      }
      // 确认送出
      while (!found[7]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElement(By.ClassName("css-1ok07mx"));
          button.Click();
          found[7] = true;
        }
        catch { }
      }
      // 查看结果
      while (!found[8]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement icon;
        IWebElement button;
        try {
          icon = driver.FindElement(By.ClassName("css-8y7p42"));
          new Actions(driver).ScrollToElement(icon).Perform();
          Thread.Sleep(new Random().Next(1000, 2500));

          button = driver.FindElements(By.ClassName("css-1cwmrwm"))[1];
          button.Click();
          found[8] = true;
        }
        catch { }
      }
      // 查看
      while (!found[9]) {
        Thread.Sleep(new Random().Next(1000, 2500));
        IWebElement chart;
        try {
          chart = driver.FindElements(By.ClassName("css-bl5djx"))[3];
          new Actions(driver).ScrollToElement(chart).Perform();
          Thread.Sleep(new Random().Next(2000, 5000));
          found[9] = true;
        }
        catch { }
      }
      driver.Quit();
    }
  }

}