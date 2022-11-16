using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xunit.Abstractions;

namespace Axis.Data.Test;

public class DataReceiveFromWebTest {

  private readonly ITestOutputHelper _output;

  public DataReceiveFromWebTest(ITestOutputHelper output) {
    _output = output;
  }

  [Fact]
  public void Get_Web_Data() {
    // web request function
    Func<string, string, Dictionary<string, string>?, string> get_data = (url, method, post_data) => {
      HttpClient client = new HttpClient();
      HttpResponseMessage response;
      HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);
      if (method == "POST") {
        MultipartFormDataContent content = new();
        if (post_data != null) {
          foreach (var data in post_data)
            content.Add(new StringContent(data.Value), data.Key);
        }
        request.Content = content;
      }
      response = client.Send(request);
      using (MemoryStream ms = new MemoryStream())
      using (var stream = response.Content.ReadAsStream()) {
        stream.CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
      }
    };
    // try to get the download_url
    string http_content = get_data.Invoke("https://b5.caspio.com/dp.asp?AppKey=0c4a300099659d7167dc4dc2a17e", "GET", null);
    string pattern = "<a data-cb-name=\"DataDownloadButton\" title=\"Download Data\" href=\"(\\S*)\"";
    Match match = Regex.Match(http_content, pattern);
    if (match.Success && match.Groups.Count > 1) {
      string download_url = $"{HttpUtility.HtmlDecode(match.Groups[1].Value)}&rnd={(long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds}";
      // try to get xml_content
      string xml_content = get_data.Invoke(download_url, "POST", new Dictionary<string, string>() { { "downloadFormat", "excel" } });
      string pattern_row = "<Row>([\\n\\s]*<Cell ss:StyleID=\"(.*)\"><Data ss:Type=\"(.*)\">(.*)</Data></Cell>)*[\\n\\s]*</Row>";
      // create data table
      DataTable table = new DataTable();
      // fill data
      MatchCollection match_rows = Regex.Matches(xml_content, pattern_row);
      for (int i = 0; i < match_rows.Count; i++) {
        if (match_rows[i].Groups.Count >= 5) {
          bool is_header = false;
          string[] values = new string[match_rows[i].Groups[2].Captures.Count];
          for (int j = 0; j < values.Length; j++) {
            string style_id = match_rows[i].Groups[2].Captures[j].Value;
            switch (style_id) {
              case "sHeader":
                table.Columns.Add(match_rows[i].Groups[4].Captures[j].Value);
                is_header = true;
                break;
              case "sText":
              case "s6":
                values[j] = match_rows[i].Groups[4].Captures[j].Value;
                break;
            }
          }
          if (is_header) {
            continue;
          }
          table.Rows.Add(values);
          _output.WriteLine(string.Join(',', values));
          Assert.True(table.Columns.Count > 0);
          Assert.True(table.Rows.Count > 0);
        }
      }
    }
  }

}