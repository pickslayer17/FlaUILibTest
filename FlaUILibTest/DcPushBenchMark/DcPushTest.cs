using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FlaUILibTest.DcPushBenchMark;

public class DcPushTest
{
    public string ObjectName;
    public string TextFieldName;
    public string EntryName;
    public string TextFieldValue;

    private HttpClient _http;
    private string _aft;

    private string clientUrl = "";
    private string appIdUrl = "";
    private string email = "";
    private string password = "";
    private int clientId = 0;
    private int _listId;

    private AutomationElement GetElement(AutomationElement root, ConditionBase condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var found = root.FindFirstDescendant(condition);
                if (found != null) return found;
            }
            catch { }
            Thread.Sleep(500);
        }
        throw new TimeoutException("Element not found.");
    }

    private void WaitForSingleTreeItem(AutomationElement parent, ConditionFactory cf)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (parent.FindAllDescendants(cf.ByControlType(ControlType.TreeItem)).Length == 1)
                    return;
            }
            catch { }
            Thread.Sleep(200);
        }
    }

    public void RunTest()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        var application = Application.Launch(processStartInfo);
        var automation = new UIA3Automation();
        var window = application.GetMainWindow(automation);
        var cf = automation.ConditionFactory;

        var stopwatch = new Stopwatch();
        var notSentStatus = "(Not Sent)";
        var sentStatus = "(Sent)";
        var dcPushFormula = "=@DCPush({0},\"{1}\",\"{2}\",\"{3}\")";
        var cellA1Name = "A1";
        var cellB1Name = "B1";
        var firstCellValue = $"PushVal_{Guid.NewGuid().ToString("N")[..5]}";

        var titleBar = GetElement(window, cf.ByControlType(ControlType.TitleBar).And(cf.ByName("Excel")));
        titleBar.Click();

        var fileTab = GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("File Tab")));
        fileTab.Click();

        var blankWorkbook = GetElement(window, cf.ByControlType(ControlType.ListItem).And(cf.ByName("Blank workbook")));
        blankWorkbook.Click();

        var dealCloudTab = GetElement(window, cf.ByControlType(ControlType.TabItem).And(cf.ByName("DealCloud")));
        dealCloudTab.Click();

        var loginButton = GetElement(window, cf.ByName("DealCloud Login").And(cf.ByControlType(ControlType.Button)));
        loginButton.Patterns.Invoke.Pattern.Invoke();

        var emailInput = GetElement(window, cf.ByControlType(ControlType.Edit).And(cf.ByName("Email")));
        emailInput.AsTextBox().Text = email;

        var nextButton = GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("Next")));
        nextButton.Patterns.Invoke.Pattern.Invoke();

        var passwordInput = GetElement(window, cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("Password")));
        passwordInput.AsTextBox().Text = password;

        var logInButton = GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("Log in")));
        logInButton.Patterns.Invoke.Pattern.Invoke();

        GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("DealCloud Logout")));

        stopwatch.Start();
        var cellA1 = GetElement(window, cf.ByControlType(ControlType.DataItem).And(cf.ByName(cellA1Name)));
        stopwatch.Stop();
        Console.WriteLine($">>> Grid ready: {stopwatch.ElapsedMilliseconds}ms");

        cellA1.Click();
        Keyboard.Type(firstCellValue);
        Keyboard.Type(VirtualKeyShort.ENTER);

        var cellB1 = GetElement(window, cf.ByControlType(ControlType.DataItem).And(cf.ByName(cellB1Name)));
        cellB1.Click();

        dealCloudTab = GetElement(window, cf.ByControlType(ControlType.TabItem).And(cf.ByName("DealCloud")));
        dealCloudTab.Click();

        stopwatch.Restart();
        var dcPushButton = GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("DCPush")));
        dcPushButton.Patterns.Invoke.Pattern.Invoke();
        stopwatch.Stop();
        Console.WriteLine($">>> DCPush button: {stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        var valueInput = GetElement(window, cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("valueInput")));
        valueInput.AsTextBox().Text = cellA1Name;

        var objectCombo = GetElement(window, cf.ByControlType(ControlType.ComboBox).And(cf.ByAutomationId("cbxLists")));
        objectCombo.FindFirstChild(cf.ByControlType(ControlType.Button).And(cf.ByName("Open"))).Click();
        var objectDropdown = GetElement(window, cf.ByControlType(ControlType.Menu).And(cf.ByName("DropDown")));
        objectDropdown.FindFirstDescendant(cf.ByControlType(ControlType.Edit)).AsTextBox().Text = ObjectName;
        GetElement(objectDropdown, cf.ByControlType(ControlType.TreeItem).And(cf.ByName(ObjectName))).Click();

        var entryCombo = GetElement(window, cf.ByControlType(ControlType.ComboBox).And(cf.ByAutomationId("cbxEntries")));
        entryCombo.FindFirstChild(cf.ByControlType(ControlType.Button).And(cf.ByName("Open"))).Click();
        var entryDropdown = GetElement(window, cf.ByControlType(ControlType.Menu).And(cf.ByName("DropDown")));
        entryDropdown.FindFirstDescendant(cf.ByControlType(ControlType.Edit)).AsTextBox().Text = EntryName;
        GetElement(entryDropdown, cf.ByControlType(ControlType.TreeItem).And(cf.ByName(EntryName))).Click();

        var fieldCombo = GetElement(window, cf.ByControlType(ControlType.ComboBox).And(cf.ByAutomationId("cbxFields")));
        fieldCombo.FindFirstChild(cf.ByControlType(ControlType.Button).And(cf.ByName("Open"))).Click();
        var fieldDropdown = GetElement(window, cf.ByControlType(ControlType.Menu).And(cf.ByName("DropDown")));
        fieldDropdown.FindFirstDescendant(cf.ByControlType(ControlType.Edit)).AsTextBox().Text = TextFieldName;
        GetElement(fieldDropdown, cf.ByControlType(ControlType.TreeItem).And(cf.ByName(TextFieldName))).Click();

        stopwatch.Stop();
        Console.WriteLine($">>> Dialog filled: {stopwatch.ElapsedMilliseconds}ms");

        var runButton = GetElement(window, cf.ByControlType(ControlType.Button).And(cf.ByName("RUN")));
        runButton.Click();

        stopwatch.Restart();
        var formulaBar = GetElement(window, cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("FormulaBar")));
        cellB1 = GetElement(window, cf.ByControlType(ControlType.DataItem).And(cf.ByName(cellB1Name)));
        cellB1.Click();
        var formulaText = formulaBar.AsTextBox().Text;
        var expectedFormula = string.Format(dcPushFormula, cellA1Name, ObjectName, EntryName, TextFieldName);
        stopwatch.Stop();
        Console.WriteLine($">>> Formula check: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"    Expected: {expectedFormula}");
        Console.WriteLine($"    Actual:   {formulaText}");
        Console.WriteLine($"    Match: {formulaText == expectedFormula}");

        stopwatch.Restart();
        var expectedNotSent = $"{firstCellValue} {notSentStatus}";
        var deadline = DateTime.UtcNow.AddSeconds(30);
        var cellValue = "";
        while (DateTime.UtcNow < deadline)
        {
            cellB1.Click();
            cellValue = cellB1.AsTextBox().Text;
            if (cellValue.Contains(notSentStatus)) break;
        }
        stopwatch.Stop();
        Console.WriteLine($">>> Not Sent status: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"    Expected: {expectedNotSent}");
        Console.WriteLine($"    Actual:   {cellValue}");

        stopwatch.Restart();
        var sendMenu = GetElement(window, cf.ByControlType(ControlType.MenuItem).And(cf.ByName("Send")));
        sendMenu.Click();
        var selectionItem = GetElement(window, cf.ByControlType(ControlType.MenuItem).And(cf.ByName("Selection")));
        selectionItem.Patterns.Invoke.Pattern.Invoke();
        stopwatch.Stop();
        Console.WriteLine($">>> Send Selection: {stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        var expectedSent = $"{firstCellValue} {sentStatus}";
        deadline = DateTime.UtcNow.AddSeconds(30);
        cellValue = "";
        cellB1 = GetElement(window, cf.ByControlType(ControlType.DataItem).And(cf.ByName(cellB1Name)));
        while (DateTime.UtcNow < deadline)
        {
            cellValue = cellB1.AsTextBox().Text;
            if (cellValue.Contains(sentStatus)) break;
        }
        stopwatch.Stop();
        Console.WriteLine($">>> Sent status: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"    Expected: {expectedSent}");
        Console.WriteLine($"    Actual:   {cellValue}");
        Console.WriteLine($"    Match: {cellValue == expectedSent}");

        Console.WriteLine("\n=== BENCHMARK COMPLETE ===");
        Console.ReadLine();
        automation.Dispose();
    }

    public void RunPreconditions()
    {
        var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };

        // AUTH
        var loginPage = _http.GetStringAsync($"{appIdUrl}/Account/Login").Result;
        var rvt = Regex.Match(loginPage, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""").Groups[1].Value;

        var loginHtml = _http.PostAsync($"{appIdUrl}/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", email }, { "Password", password }, { "LoginType", "" },
            { "ForgotPassword", "/Account/ForgotPassword" }, { "IpToken", "" },
            { "SkipTwoFactorToken", "" }, { "DeviceId", "" },
            { "__RequestVerificationToken", rvt }
        })).Result.Content.ReadAsStringAsync().Result;

        if (loginHtml.Contains("Client Selection"))
        {
            var selectRvt = Regex.Match(loginHtml, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""").Groups[1].Value;
            loginHtml = _http.PostAsync(appIdUrl, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "__RequestVerificationToken", selectRvt }, { "clientId", clientId.ToString() }
            })).Result.Content.ReadAsStringAsync().Result;
        }

        var samlToken = Regex.Match(loginHtml, @"name=""SAMLResponse""[^>]*value=""([^""]+)""").Groups[1].Value;
        _http.PostAsync($"{clientUrl}/Saml/AssertionConsumerService", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "SAMLResponse", samlToken }
        })).Wait();

        var mainPage = _http.GetStringAsync(clientUrl).Result;
        _aft = Regex.Match(mainPage, @"sessionStorage\.setItem\('EncodedRequestVerificationToken', '([^']+)'\)").Groups[1].Value;
        if (string.IsNullOrEmpty(_aft))
        {
            var raw = Regex.Match(mainPage, @"sessionStorage\.setItem\('RequestVerificationToken', '([^']+)'\)").Groups[1].Value;
            _aft = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }
        Console.WriteLine($"Authorized. AFT: {_aft[..20]}...");

        // CREATE OBJECT
        ObjectName = $"Object_{Guid.NewGuid().ToString("N")[..8]}";
        TextFieldName = $"TextField_{Guid.NewGuid().ToString("N")[..8]}";

        var categories = ApiGet("/api/listManagement/lists/categories")["model"] as JArray;
        var entityCategoryId = categories.First(c => c["entryListType"].Value<int>() == 1)["id"].Value<int>();

        var templateFields = ApiGet($"/api/listManagement/lists/new?templateId={entityCategoryId}&fieldNameType=0")["model"]["fields"] as JArray;

        var changedFields = templateFields.DeepClone() as JArray;
        var nameFieldIdFromTemplate = changedFields.First(f => f["isName"]?.Value<bool>() == true)["id"].Value<int>();

        changedFields.Add(JObject.FromObject(new
        {
            id = -(changedFields.Count + 1),
            name = TextFieldName,
            isName = false,
            isRequired = false,
            fieldType = 1,
            formatTypeId = 1,
            templateId = 1,
            isDataEditable = true,
            isChanged = true
        }));

        var formData = SerializationHelper.DeserializeToDictionary(new
        {
            name = ObjectName,
            singularName = ObjectName,
            pluralName = ObjectName,
            sourceEntryListId = entityCategoryId,
            changedFields,
            entryForms = new[]
            {
                new
                {
                    name = "Entry Form",
                    id = -1,
                    choiceId = 0,
                    rank = 1,
                    isDefault = true,
                    isActive = true,
                    visibilityFlags = new[] { 1, 2, 4 },
                    userGroups = new[] { 0 },
                    tabs = new[]
                    {
                        new
                        {
                            id = 0,
                            name = $"{ObjectName } Details",
                            rows = new[]
                            {
                                new
                                {
                                    layoutConfig = new[]
                                    {
                                        new { fieldId = nameFieldIdFromTemplate, startPosition = 0, width = 6, isBranchShown = false }
                                    },
                                    guid = Guid.NewGuid().ToString()
                                }
                            }
                        }
                    }
                }
            }
        });

        var createListResult = ApiPostForm("/api/listManagement/lists/save", formData);
        Console.WriteLine($"List created: {ObjectName} (status: {createListResult["statusCode"]})");

        var allLists = ApiGet("/api/categories/lists")["model"] as JArray;
        _listId = allLists.First(l => HttpUtility.HtmlDecode(l["name"].Value<string>()) == ObjectName)["id"].Value<int>();
        var listFields = ApiGet($"/api/listManagement/lists/{_listId}/edit")["model"]["fields"] as JArray;
        var nameFieldId = listFields.First(f => f["isName"].Value<bool>())["id"].Value<int>();
        var textFieldId = listFields.First(f => f["name"].Value<string>() == TextFieldName)["id"].Value<int>();

        EntryName = $"Entry_{Guid.NewGuid().ToString("N")[..8]}";
        TextFieldValue = $"Value_{Guid.NewGuid().ToString("N")[..8]}";

        var entryBody = new StringContent(JsonConvert.SerializeObject(new[]
        {
            new { id = nameFieldId, name = "Name", isName = true, value = (object)EntryName, formFieldType = 0, isMultiple = false },
            new { id = textFieldId, name = TextFieldName, isName = false, value = (object)TextFieldValue, formFieldType = 0, isMultiple = false }
        }), Encoding.UTF8, "application/json");
        entryBody.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "\"fields\"", FileName = "\"blob\"" };

        var entryResult = ApiPostMultipart($"/api/entryFeed/lists/{_listId}/entries/-1/form/false", new MultipartFormDataContent(DateTime.Now.Ticks.ToString()) { entryBody });
        Console.WriteLine($"Entry created: {EntryName} (status: {entryResult["statusCode"]})");
    }
    JObject ApiGet(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{clientUrl}{path}");
        request.Headers.TryAddWithoutValidation("RequestVerificationToken", _aft);
        return JObject.Parse(_http.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
    }

    JObject ApiPost(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{clientUrl}{path}");
        request.Headers.TryAddWithoutValidation("RequestVerificationToken", _aft);
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        return JObject.Parse(_http.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
    }

    JObject ApiPostMultipart(string path, MultipartFormDataContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{clientUrl}{path}");
        request.Headers.TryAddWithoutValidation("RequestVerificationToken", _aft);
        request.Content = content;
        return JObject.Parse(_http.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
    }

    JObject ApiPostForm(string path, Dictionary<string, string> formData)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{clientUrl}{path}");
        request.Headers.TryAddWithoutValidation("RequestVerificationToken", _aft);
        request.Content = new FormUrlEncodedContent(formData);
        return JObject.Parse(_http.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
    }

    public void CleanUp()
    {
        Console.WriteLine("\nENTER to cleanup...");
        Console.ReadLine();
        ApiPost($"/api/listManagement/lists/{_listId}/delete", null);
        Console.WriteLine("Done.");
    }
}