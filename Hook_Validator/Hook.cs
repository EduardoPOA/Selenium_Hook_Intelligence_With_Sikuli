/*
 * @author Eduardo Oliveira
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
            // CORREÇÃO: Usar Path.DirectorySeparatorChar para ser compatível com Windows e Linux
            string directorySeparator = Path.DirectorySeparatorChar.ToString();
            pathReport = directorySeparator + pathReport + directorySeparator;
            pathLocators = directorySeparator + pathLocators + directorySeparator;

            // CORREÇÃO: Usar Directory.GetCurrentDirectory() em vez de Assembly.Location
            string baseDirectory = Directory.GetCurrentDirectory();
            string filePathReport = Path.Combine(baseDirectory, pathReport.TrimStart(Path.DirectorySeparatorChar));
            string filePathLocators = Path.Combine(baseDirectory, pathLocators.TrimStart(Path.DirectorySeparatorChar));

            // CORREÇÃO: Criar diretório se não existir
            Directory.CreateDirectory(filePathReport);
            Directory.CreateDirectory(filePathLocators);

            // Limpar arquivos antigos apenas se o diretório existir
            if (Directory.Exists(filePathReport))
            {
                try
                {
                    var filesToDelete = Directory.GetFiles(filePathReport)
                        .Where(file => file.EndsWith(".txt") || file.EndsWith(".PNG") || file.EndsWith(".html"))
                        .ToList();

                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Não foi possível deletar {file}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao limpar arquivos: {ex.Message}");
                }
            }

            getPathReport = pathReport;
            getPathLocator = pathLocators;

            var htmlReport = new ExtentHtmlReporter(filePathReport);
            htmlReport.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            extent = new AventStack.ExtentReports.ExtentReports();
            extent.AttachReporter(htmlReport);

            // CORREÇÃO: Matar processos de driver de forma compatível
            foreach (var processName in new[] { "chromedriver", "msedgedriver", "geckodriver", "chromedriver.exe", "msedgedriver.exe", "geckodriver.exe" })
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName(processName.Replace(".exe", "")))
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao matar processo {processName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao acessar processos {processName}: {ex.Message}");
                }
            }

            // CORREÇÃO: Deletar pastas de browsers de forma segura
            string[] browserFolders = { "Chrome", "Edge", "Firefox" };
            string baseFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var browserFolder in browserFolders)
            {
                string browserFolderPath = Path.Combine(baseFolderPath, browserFolder);
                if (Directory.Exists(browserFolderPath))
                {
                    try
                    {
                        Directory.Delete(browserFolderPath, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Não foi possível deletar {browserFolderPath}: {ex.Message}");
                    }
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
            extent?.Flush();
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
                    optionChrome.EnableMobileEmulation(device);
                    Selenium.driver = new ChromeDriver(GetChromeDriver(), optionChrome);
                    Selenium.driver.Manage().Window.Maximize();
                    break;
                case "Edge":
                    new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
                    EdgeOptions optionEdge = new EdgeOptions();
                    optionEdge.AddArgument(headless);
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

        // CORREÇÃO: Métodos para obter drivers de forma compatível com Windows e Linux
        private static string GetChromeDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderChrome = Path.Combine(baseDirectory, "Chrome");

            if (!Directory.Exists(folderChrome))
                return baseDirectory;

            try
            {
                string driverName = IsWindows() ? "chromedriver.exe" : "chromedriver";
                var driverFile = Directory.GetFiles(folderChrome, driverName, SearchOption.AllDirectories)
                    .FirstOrDefault();

                return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
            }
            catch
            {
                return baseDirectory;
            }
        }
        public static string GetEdgeDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderEdge = Path.Combine(baseDirectory, "Edge");

            if (!Directory.Exists(folderEdge))
                return baseDirectory;

            try
            {
                string driverName = IsWindows() ? "msedgedriver.exe" : "msedgedriver";
                var driverFile = Directory.GetFiles(folderEdge, driverName, SearchOption.AllDirectories)
                    .FirstOrDefault();

                return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
            }
            catch
            {
                return baseDirectory;
            }
        }
        private static string GetFirefoxDriver()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderFirefox = Path.Combine(baseDirectory, "Firefox");

            if (!Directory.Exists(folderFirefox))
                return baseDirectory;

            try
            {
                string driverName = IsWindows() ? "geckodriver.exe" : "geckodriver";
                var driverFile = Directory.GetFiles(folderFirefox, driverName, SearchOption.AllDirectories)
                    .FirstOrDefault();

                return driverFile != null ? Path.GetDirectoryName(driverFile) : baseDirectory;
            }
            catch
            {
                return baseDirectory;
            }
        }

        // CORREÇÃO: Método para detectar o sistema operacional
        private static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
        enum BrowserType
        {
            Chrome,
            Firefox,
            Edge
        }
    }
}