/*@author Eduardo Oliveira
*/
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter.Config;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Reqnroll;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using WebDriverManager.DriverConfigs.Impl;

namespace Hook_Validator
{
    public static class Hook
    {
        private static ExtentTest featureName;
        private static AventStack.ExtentReports.ExtentTest scenario;
        private static AventStack.ExtentReports.ExtentReports extent;
        public static IWebDriver getDriver;
        public static string getPathReport { get; set; }
        public static string getPathLocator { get; set; }
        public static ConcurrentDictionary<string, AventStack.ExtentReports.ExtentTest> FeatureDictionary = new ConcurrentDictionary<string, AventStack.ExtentReports.ExtentTest>();

        /// <summary>
        ///  Preparação do relatório e especificação dos caminhos das pastas
        ///  do Report e do Locators
        /// </summary>
        /// <param name="pathReport">Folder Report</param>
        /// <param name="pathLocators">Folder Locators</param>
        [BeforeTestRun]
        public static void BeforeTestRun(string pathReport, string pathLocators)
        {
            // Normaliza os parâmetros e monta os paths relativos
            pathReport = Path.DirectorySeparatorChar + pathReport.Trim(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            pathLocators = Path.DirectorySeparatorChar + pathLocators.Trim(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePathReport = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", pathReport.TrimStart(Path.DirectorySeparatorChar)));
            string filePathLocators = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", pathLocators.TrimStart(Path.DirectorySeparatorChar)));

            // Cria diretórios se não existirem
            Directory.CreateDirectory(filePathReport);
            Directory.CreateDirectory(filePathLocators);

            // Limpa arquivos antigos (opcional: ajusta extensões conforme necessidade)
            try
            {
                foreach (var file in Directory.GetFiles(filePathReport))
                {
                    if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                        || file.EndsWith(".PNG", StringComparison.OrdinalIgnoreCase)
                        || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        || file.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clean report directory: {ex.Message}");
            }

            getPathReport = pathReport;
            getPathLocator = pathLocators;
            string fileLocator = Path.Combine(filePathLocators, "PageObjects.xml");
            Tools.SaveXmlWithFormattingForHook(fileLocator);
            // Caminho do arquivo index.html
            string reportFile = Path.Combine(filePathReport, "index.html");

            // Cria o Spark Reporter
            var spark = new AventStack.ExtentReports.Reporter.ExtentSparkReporter(reportFile);

            // Habilita tema Dark
            spark.Config.Theme = Theme.Dark;

            // (Opcional) Títulos
            //spark.Config.ReportName = "Relatório de Testes";
            //spark.Config.DocumentTitle = "Automation Report Farmacias São João";

            // Cria o ExtentReports principal
            extent = new ExtentReports();

            // Anexa o reporter
            extent.AttachReporter(spark);


            // (Opcional) adiciona informações do sistema no relatório
            extent.AddSystemInfo("Machine", Environment.MachineName);
            extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
            extent.AddSystemInfo("User", Environment.UserName);

            // Mata processos de drivers antigos (mantive sua lógica)
            string[] processNames = GetDriverProcessNames();
            foreach (var processName in processNames)
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not kill process {processName}: {ex.Message}");
                }
            }

            // Limpa pastas dos browsers (se necessário)
            string[] browserFolders = { "Chrome", "Edge", "Firefox" };
            foreach (var browserFolder in browserFolders)
            {
                string browserFolderPath = Path.Combine(baseDirectory, browserFolder);
                try
                {
                    if (Directory.Exists(browserFolderPath))
                        Directory.Delete(browserFolderPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete directory {browserFolderPath}: {ex.Message}");
                }
            }
        }


        /// <summary>
        ///  É gerado um txt da classe tools para fazer o contador de imagens.
        ///  Neste método então ele fecha completamente o relatório pronto com suas evidências e 
        ///  deleta este mesmo txt para deixar tudo limpo
        /// </summary>
        [AfterTestRun]
        public static void AfterTestRun()
        {
            extent.Flush();
        }

        /// <summary>
        ///  Método que pega o nome atual da feature para poder inserir no relatório
        ///  AddTags é o modo de colocar inserir uma tag em cima da feature exemplo
        ///  @LoginBuscadoNoBanco então no relatório será filtrado para melhor visualização
        /// </summary>
        [BeforeFeature]
        public static void BeforeFeature()
        {
            featureName = extent.CreateTest<AventStack.ExtentReports.Gherkin.Model.Feature>(FeatureContext.Current.FeatureInfo.Title);
            FeatureDictionary.TryAdd(FeatureContext.Current.FeatureInfo.Title, featureName);
            AddTags(featureName, FeatureContext.Current.FeatureInfo.Tags);
        }

        /// <summary>
        ///  São tres forma de iniciar o browser:
        ///  Modo visível, Modo invisível, Modo mobile
        ///  AddTags é o modo de colocar inserir uma tag em cima do cenário exemplo
        ///  @Bug ou @Aprovado então no relatório será filtrado para melhor visualização
        /// </summary>
        /// <param name="browser">Qual navegador será executado</param>
        /// <param name="headless">Modo invisível</param>
        /// <param name="device">Modo mobile apenas especificar o modelo mobile</param>
        [BeforeScenario]
        public static void BeforeScenario(string browser, string arg, string device, bool certificate)
        {
            SelectBrowser(browser, arg, device, certificate);
            string InBSName = FeatureContext.Current.FeatureInfo.Title;
            if (FeatureDictionary.ContainsKey(InBSName))
            {
                scenario = FeatureDictionary[InBSName].CreateNode<AventStack.ExtentReports.Gherkin.Model.Scenario>(ScenarioContext.Current.ScenarioInfo.Title);
            }
            AddTags(scenario, ScenarioContext.Current.ScenarioInfo.Tags);
        }

        /// <summary>
        ///  Para cada cenário o selenium fecha e conclui o que foi feito para abrir o próximo browser
        /// </summary>
        [AfterScenario]
        public static void AfterScenario()
        {
            Tools.Shutdown();
            Selenium.TearDown();
        }


        /// <summary>
        ///  São comportamentos de cada step seja positivo ou negativo
        ///  Se caso negativo é inserido o erro do Nunit e o screenshot
        /// </summary>
        [AfterStep]
        public static void AfterStep()
        {
            var step = ScenarioStepContext.Current.StepInfo.StepDefinitionType.ToString();

            if (ScenarioContext.Current.TestError == null)
            {
                switch (step)
                {
                    case "Given":
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Given>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "When":
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.When>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "And":
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.And>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "But":
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.But>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "Then":
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Then>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                }
            }
            else if (ScenarioContext.Current.TestError != null)
            {
                var status = TestContext.CurrentContext.Result.Outcome.Status;
                var errorMessage = TestContext.CurrentContext.Result.Message;
                string screenShotPath = Tools.Screenshot();

                switch (step)
                {
                    case "Given" when status == TestStatus.Failed:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Given>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Given" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Given>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "When" when status == TestStatus.Failed:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.When>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "When" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.When>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "And" when status == TestStatus.Failed:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.And>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "And" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.And>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "But" when status == TestStatus.Failed:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.But>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "But" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.But>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Then" when status == TestStatus.Failed:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Then>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Then" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<AventStack.ExtentReports.Gherkin.Model.Then>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                }
            }
        }

        private static string ConvertFilePathToBase64(string filePath)
        {
            byte[] imageArray = File.ReadAllBytes(filePath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            return base64ImageRepresentation;
        }

        private static void AddTags(ExtentTest testNode, string[] tags)
        {
            if (tags != null)
                testNode.AssignCategory(tags);
        }

        internal static void SelectBrowser(string browserType, string headless, string device, bool certificate)
        {
            bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix ||
                           Environment.OSVersion.Platform == PlatformID.MacOSX;
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            switch (browserType)
            {
                case "Chrome":
                    ChromeOptions optionChrome = new ChromeOptions();

                    optionChrome.AcceptInsecureCertificates = certificate;
                    optionChrome.AddArgument("--lang=pt-BR");
                    optionChrome.AddUserProfilePreference("intl.accept_languages", "pt-BR,pt");
                    // Anti-automation
                    optionChrome.AddExcludedArgument("enable-automation");
                    optionChrome.AddAdditionalOption("useAutomationExtension", false);
                    optionChrome.AddArgument("--disable-blink-features=AutomationControlled");

                    // Performance / estabilidade
                    optionChrome.AddArgument("--disable-background-timer-throttling");
                    optionChrome.AddArgument("--disable-renderer-backgrounding");
                    optionChrome.AddArgument("--disable-ipc-flooding-protection");

                    // Linux hardening
                    if (isLinux)
                    {
                        optionChrome.AddArgument("--no-sandbox");
                        optionChrome.AddArgument("--disable-dev-shm-usage");
                        optionChrome.AddArgument("--disable-gpu");
                        optionChrome.AddArgument("--disable-software-rasterizer");
                    }
                    string binPath = AppContext.BaseDirectory;
                    Directory.CreateDirectory(binPath);

                    optionChrome.AddUserProfilePreference("download.default_directory", binPath);
                    optionChrome.AddUserProfilePreference("download.prompt_for_download", false);
                    optionChrome.AddUserProfilePreference("download.directory_upgrade", true);
                    optionChrome.AddUserProfilePreference("profile.default_content_settings.popups", 0);
                    optionChrome.AddUserProfilePreference("safebrowsing.enabled", true);

                    // Senhas / autofill / notificações
                    optionChrome.AddUserProfilePreference("credentials_enable_service", false);
                    optionChrome.AddUserProfilePreference("profile.password_manager_enabled", false);
                    optionChrome.AddUserProfilePreference("profile.password_manager_leak_detection", false);
                    optionChrome.AddUserProfilePreference("autofill.profile_enabled", false);
                    optionChrome.AddUserProfilePreference("autofill.credit_card_enabled", false);

                    optionChrome.AddArgument("--disable-save-password-bubble");
                    optionChrome.AddArgument("--disable-password-generation");
                    optionChrome.AddArgument("--disable-notifications");
                    optionChrome.AddArgument("--disable-features=PasswordLeakDetection");

                    // Headless
                    if (headless.Equals("--headless"))
                        optionChrome.AddArgument("--headless=new");
                    else if (!string.IsNullOrEmpty(headless))
                        optionChrome.AddArgument(headless);

                    // Mobile
                    if (!string.IsNullOrEmpty(device))
                        optionChrome.EnableMobileEmulation(device);

                    try
                    {
                        ChromeDriverService service;
                        if (isWindows)
                        {
                            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
                            service = ChromeDriverService.CreateDefaultService();
                            Console.WriteLine("ChromeDriver configurado via WebDriverManager (Windows)");
                        }
                        else
                        {
                            service = ChromeDriverService.CreateDefaultService();
                            service.EnableVerboseLogging = false;
                            Console.WriteLine("Usando ChromeDriver do PATH (Linux)");
                        }

                        service.SuppressInitialDiagnosticInformation = true;
                        service.HideCommandPromptWindow = true;

                        Console.WriteLine("Iniciando ChromeDriver...");
                        Selenium.driver = new ChromeDriver(service, optionChrome, TimeSpan.FromSeconds(120));
                        Selenium.driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        Selenium.driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
                        Selenium.driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);

                        if (isLinux)
                            Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                        else
                            Selenium.driver.Manage().Window.Maximize();
                        getDriver = Selenium.driver;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao iniciar ChromeDriver: {ex.Message}");
                        throw;
                    }
                    break;
                case "Edge":
                    EdgeOptions optionEdge = new EdgeOptions();

                    // Aceita certificados inválidos
                    optionEdge.AcceptInsecureCertificates = certificate;
                    optionEdge.AddUserProfilePreference("credentials_enable_service", false);
                    optionEdge.AddUserProfilePreference("profile.password_manager_enabled", false);
                    optionEdge.AddArgument("--disable-features=PasswordLeakDetection");
                    optionEdge.AddArgument("--disable-save-password-bubble");

                    if (isLinux)
                    {
                        optionEdge.AddArgument("--no-sandbox");
                        optionEdge.AddArgument("--disable-dev-shm-usage");
                        optionEdge.AddArgument("--disable-gpu");
                    }

                    if (!string.IsNullOrEmpty(headless))
                        optionEdge.AddArgument(headless);

                    if (!string.IsNullOrEmpty(device))
                        optionEdge.EnableMobileEmulation(device);

                    EdgeDriverService edgeService;
                    if (isWindows)
                    {
                        new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
                        edgeService = EdgeDriverService.CreateDefaultService();
                    }
                    else
                    {
                        edgeService = EdgeDriverService.CreateDefaultService();
                    }

                    Selenium.driver = new EdgeDriver(edgeService, optionEdge, TimeSpan.FromSeconds(120));

                    if (isLinux)
                        Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                    else
                        Selenium.driver.Manage().Window.Maximize();
                    getDriver = Selenium.driver;
                    break;

                case "Firefox":
                    FirefoxOptions optionFirefox = new FirefoxOptions();

                    // Aceita certificados inválidos
                    optionFirefox.AcceptInsecureCertificates = certificate;

                    if (isLinux)
                        optionFirefox.AddArgument("--headless");

                    if (headless.Equals("--headless"))
                        optionFirefox.AddArgument("--headless");

                    FirefoxDriverService firefoxService;
                    if (isWindows)
                    {
                        new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
                        firefoxService = FirefoxDriverService.CreateDefaultService();
                    }
                    else
                    {
                        firefoxService = FirefoxDriverService.CreateDefaultService();
                    }

                    Selenium.driver = new FirefoxDriver(firefoxService, optionFirefox, TimeSpan.FromSeconds(120));

                    if (isLinux)
                        Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                    else
                        Selenium.driver.Manage().Window.Maximize();
                    getDriver = Selenium.driver;
                    break;
            }
        }

        private static string[] GetDriverProcessNames()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new[] { "chromedriver", "msedgedriver", "geckodriver" };
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    return new[] { "chromedriver", "msedgedriver", "geckodriver" };
                default:
                    return new[] { "chromedriver", "msedgedriver", "geckodriver" };
            }
        }

        enum BrowserType
        {
            Chrome,
            Firefox,
            Edge
        }
    }
}