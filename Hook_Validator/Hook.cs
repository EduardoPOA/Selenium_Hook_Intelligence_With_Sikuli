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
using WebDriverManager.DriverConfigs.Impl;

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

            // Limpar pastas dos browsers
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
            switch (browserType)
            {
                case "Chrome":
                    new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
                    ChromeOptions optionChrome = new ChromeOptions();
                    optionChrome.AddArgument(headless);
                    if (!string.IsNullOrEmpty(device))
                        optionChrome.EnableMobileEmulation(device);
                    Selenium.driver = new ChromeDriver(GetChromeDriver(), optionChrome);
                    Selenium.driver.Manage().Window.Maximize();
                    break;
                case "Edge":
                    new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
                    EdgeOptions optionEdge = new EdgeOptions();
                    optionEdge.AddArgument(headless);
                    if (!string.IsNullOrEmpty(device))
                        optionEdge.EnableMobileEmulation(device);
                    Selenium.driver = new EdgeDriver(GetEdgeDriver(), optionEdge);
                    Selenium.driver.Manage().Window.Maximize();
                    break;
                case "Firefox":
                    new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
                    FirefoxOptions optionFirefox = new FirefoxOptions();
                    if (headless.Equals("--headless"))
                    {
                        optionFirefox.AddArgument(headless);
                        Selenium.driver = new FirefoxDriver(GetFirefoxDriver(), optionFirefox);
                    }
                    else
                    {
                        Selenium.driver = new FirefoxDriver(GetFirefoxDriver());
                    }
                    Selenium.driver.Manage().Window.Maximize();
                    break;
            }
        }

        private static string GetChromeDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderChrome = Path.Combine(baseDirectory, "Chrome");

            if (!Directory.Exists(folderChrome))
                return baseDirectory;

            // Buscar o executável do driver considerando diferentes sistemas operacionais
            string driverFile = FindDriverFile(folderChrome, "chromedriver*");
            return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
        }

        private static string GetEdgeDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderEdge = Path.Combine(baseDirectory, "Edge");

            if (!Directory.Exists(folderEdge))
                return baseDirectory;

            // Buscar o executável do driver considerando diferentes sistemas operacionais
            string driverFile = FindDriverFile(folderEdge, "msedgedriver*");
            return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
        }

        private static string GetFirefoxDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderFirefox = Path.Combine(baseDirectory, "Firefox");

            if (!Directory.Exists(folderFirefox))
                return baseDirectory;

            // Buscar o executável do driver considerando diferentes sistemas operacionais
            string driverFile = FindDriverFile(folderFirefox, "geckodriver*");
            return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
        }

        private static string FindDriverFile(string directory, string searchPattern)
        {
            try
            {
                var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);

                // Priorizar o executável correto para o sistema operacional
                string osSpecificExtension = GetDriverExtension();
                var osSpecificFile = files.FirstOrDefault(f => f.EndsWith(osSpecificExtension));

                return osSpecificFile ?? files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding driver file: {ex.Message}");
                return null;
            }
        }

        private static string GetDriverExtension()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return ".exe";
                case PlatformID.Unix:
                    return ""; // Linux e macOS
                case PlatformID.MacOSX:
                    return "";
                default:
                    return ".exe"; // fallback
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