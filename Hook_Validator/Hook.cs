/*@author Eduardo Oliveira
*/
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Reqnroll;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Hook_Validator
{
    public static class Hook
    {
        private static ExtentTest featureName;
        private static ExtentTest scenario;
        private static AventStack.ExtentReports.ExtentReports extent;
        public static string getPathReport { get; set; }
        public static string getPathLocator { get; set; }
        public static ConcurrentDictionary<string, ExtentTest> FeatureDictionary = new ConcurrentDictionary<string, ExtentTest>();

        // REMOÇÃO: A maioria dos métodos de busca de driver e GetDriverExtension foram removidos.

        /// <summary>
        ///  Preparação do relatório e especificação dos caminhos das pastas
        ///  do Report e do Locators
        /// </summary>
        /// <param name="pathReport">Folder Report</param>
        /// <param name="pathLocators">Folder Locators</param>
        [BeforeTestRun]
        public static void BeforeTestRun(string pathReport, string pathLocators)
        {
            // Usar Path.Combine para criar caminhos multiplataforma
            pathReport = Path.DirectorySeparatorChar + pathReport + Path.DirectorySeparatorChar;
            pathLocators = Path.DirectorySeparatorChar + pathLocators + Path.DirectorySeparatorChar;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePathReport = Path.Combine(baseDirectory, "..", "..", "..", pathReport.TrimStart(Path.DirectorySeparatorChar));
            string filePathLocators = Path.Combine(baseDirectory, "..", "..", "..", pathLocators.TrimStart(Path.DirectorySeparatorChar));

            // Normalizar os caminhos
            filePathReport = Path.GetFullPath(filePathReport);
            filePathLocators = Path.GetFullPath(filePathLocators);

            // Criar diretórios se não existirem
            Directory.CreateDirectory(filePathReport);
            Directory.CreateDirectory(filePathLocators);

            // Limpar arquivos antigos
            try
            {
                new List<string>(Directory.GetFiles(filePathReport))
                    .ForEach(file =>
                    {
                        if (file.EndsWith(".txt") || file.EndsWith(".PNG") || file.EndsWith(".html"))
                            File.Delete(file);
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not clean report directory: {ex.Message}");
            }

            getPathReport = pathReport;
            getPathLocator = pathLocators;

            var htmlReport = new ExtentHtmlReporter(filePathReport);
            htmlReport.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            extent = new AventStack.ExtentReports.ExtentReports();
            extent.AttachReporter(htmlReport);

            // Processos multiplataforma
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

            // Limpar pastas dos browsers (Não é mais necessário se a lógica de GetDriver for removida, mas mantido por segurança)
            string[] browserFolders = { "Chrome", "Edge", "Firefox" };
            string baseFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var browserFolder in browserFolders)
            {
                string browserFolderPath = Path.Combine(baseFolderPath, browserFolder);
                try
                {
                    if (Directory.Exists(browserFolderPath))
                    {
                        Directory.Delete(browserFolderPath, true);
                    }
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
            featureName = extent.CreateTest<Feature>(FeatureContext.Current.FeatureInfo.Title);
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
        public static void BeforeScenario(string browser, string arg, string device)
        {
            SelectBrowser(browser, arg, device);
            string InBSName = FeatureContext.Current.FeatureInfo.Title;
            if (FeatureDictionary.ContainsKey(InBSName))
            {
                scenario = FeatureDictionary[InBSName].CreateNode<Scenario>(ScenarioContext.Current.ScenarioInfo.Title);
            }
            AddTags(scenario, ScenarioContext.Current.ScenarioInfo.Tags);
        }

        /// <summary>
        ///  Para cada cenário o selenium fecha e conclui o que foi feito para abrir o próximo browser
        /// </summary>
        [AfterScenario]
        public static void AfterScenario()
        {
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
                        scenario.CreateNode<Given>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "When":
                        scenario.CreateNode<When>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "And":
                        scenario.CreateNode<And>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "But":
                        scenario.CreateNode<But>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
                        break;
                    case "Then":
                        scenario.CreateNode<Then>(ScenarioStepContext.Current.StepInfo.Text).Pass("PASSOU");
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
                        scenario.CreateNode<Given>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Given" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<Given>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "When" when status == TestStatus.Failed:
                        scenario.CreateNode<When>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "When" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<When>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "And" when status == TestStatus.Failed:
                        scenario.CreateNode<And>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "And" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<And>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "But" when status == TestStatus.Failed:
                        scenario.CreateNode<But>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "But" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<But>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Then" when status == TestStatus.Failed:
                        scenario.CreateNode<Then>(ScenarioStepContext.Current.StepInfo.Text).Fail("FALHOU" + errorMessage, MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
                        break;
                    case "Then" when status == TestStatus.Inconclusive:
                        scenario.CreateNode<Then>(ScenarioStepContext.Current.StepInfo.Text).Fail("INCONCLUSIVO - o elemento não foi encontrado", MediaEntityBuilder.CreateScreenCaptureFromBase64String(ConvertFilePathToBase64(screenShotPath)).Build());
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

        internal static void SelectBrowser(string browserType, string headless, string device)
        {
            // Detectar o sistema operacional
            bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix ||
                           Environment.OSVersion.Platform == PlatformID.MacOSX;
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            switch (browserType)
            {
                case "Chrome":
                    ChromeOptions optionChrome = new ChromeOptions();

                    // CONFIGURAÇÕES ESPECÍFICAS PARA LINUX
                    if (isLinux)
                    {
                        optionChrome.AddArgument("--no-sandbox");
                        optionChrome.AddArgument("--disable-dev-shm-usage");
                        optionChrome.AddArgument("--disable-gpu");
                        optionChrome.AddArgument("--disable-software-rasterizer");
                        optionChrome.AddArgument("--disable-dev-tools");
                        optionChrome.AddArgument("--no-zygote");
                        optionChrome.AddArgument("--single-process"); // Importante para containers

                        // Desabilita recursos que causam problemas no container
                        optionChrome.AddArgument("--disable-blink-features=AutomationControlled");
                        optionChrome.AddArgument("--disable-extensions");
                        optionChrome.AddArgument("--disable-background-networking");

                        // Configurações de download
                        optionChrome.AddUserProfilePreference("download.default_directory", "/tmp/downloads");
                        optionChrome.AddUserProfilePreference("download.prompt_for_download", false);
                        optionChrome.AddUserProfilePreference("disable-popup-blocking", true);

                        Console.WriteLine("Aplicando configurações específicas para Linux");
                    }

                    // CONFIGURAÇÕES COMUNS
                    optionChrome.AddArgument("--disable-extensions");
                    optionChrome.AddArgument("--disable-background-timer-throttling");
                    optionChrome.AddArgument("--disable-backgrounding-occluded-windows");
                    optionChrome.AddArgument("--disable-renderer-backgrounding");
                    optionChrome.AddArgument("--disable-features=TranslateUI");
                    optionChrome.AddArgument("--disable-ipc-flooding-protection");

                    // Headless mode
                    if (headless.Equals("--headless"))
                    {
                        optionChrome.AddArgument("--headless=new");
                    }
                    else if (!string.IsNullOrEmpty(headless))
                    {
                        optionChrome.AddArgument(headless);
                    }

                    // Mobile emulation
                    if (!string.IsNullOrEmpty(device))
                        optionChrome.EnableMobileEmulation(device);

                    try
                    {
                        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                        service.SuppressInitialDiagnosticInformation = true;

                        // IMPORTANTE: Adicionar variáveis de ambiente
                        service.HideCommandPromptWindow = true;

                        Selenium.driver = new ChromeDriver(service, optionChrome, TimeSpan.FromSeconds(60));

                        // ⚠️ CRÍTICO: Não use Maximize() no Linux, use SetWindowSize
                        if (isLinux)
                        {
                            // Defina tamanho fixo em vez de maximizar
                            Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                            Console.WriteLine("ChromeDriver iniciado com sucesso no Linux (tamanho definido: 1920x1080)");
                        }
                        else
                        {
                            Selenium.driver.Manage().Window.Maximize();
                            Console.WriteLine("ChromeDriver iniciado com sucesso no Windows");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao iniciar ChromeDriver: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        throw;
                    }
                    break;

                case "Edge":
                    EdgeOptions optionEdge = new EdgeOptions();

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

                    Selenium.driver = new EdgeDriver(EdgeDriverService.CreateDefaultService(), optionEdge, TimeSpan.FromSeconds(60));

                    if (isLinux)
                    {
                        Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                        Console.WriteLine("EdgeDriver iniciado com sucesso no Linux");
                    }
                    else
                    {
                        Selenium.driver.Manage().Window.Maximize();
                        Console.WriteLine("EdgeDriver iniciado com sucesso no Windows");
                    }
                    break;

                case "Firefox":
                    FirefoxOptions optionFirefox = new FirefoxOptions();

                    if (isLinux)
                    {
                        optionFirefox.AddArgument("--headless");
                    }

                    if (headless.Equals("--headless"))
                    {
                        optionFirefox.AddArgument("--headless");
                        Selenium.driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), optionFirefox, TimeSpan.FromSeconds(60));
                    }
                    else
                    {
                        Selenium.driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), optionFirefox, TimeSpan.FromSeconds(60));
                    }

                    if (isLinux)
                    {
                        Selenium.driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                        Console.WriteLine("FirefoxDriver iniciado com sucesso no Linux");
                    }
                    else
                    {
                        Selenium.driver.Manage().Window.Maximize();
                        Console.WriteLine("FirefoxDriver iniciado com sucesso no Windows");
                    }
                    break;
            }
        }

        // --- MÉTODOS REMOVIDOS ---
        // GetChromeDriver(), GetEdgeDriver(), GetFirefoxDriver(), FindDriverFile(), GetDriverExtension()

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