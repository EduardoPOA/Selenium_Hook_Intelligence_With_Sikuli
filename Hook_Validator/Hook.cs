/* @author Eduardo Oliveira
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
            // OBTENÇÃO DO DIRETÓRIO BASE (Mantido)
            // Esta lógica complexa obtém o diretório raiz do projeto.
            string baseDirectory = System.IO.Directory.GetParent(
                System.IO.Directory.GetParent(
                    System.IO.Directory.GetParent(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                    ).FullName
                ).FullName
            ).FullName;

            // AJUSTE CRÍTICO: Uso de Path.Combine() para criar caminhos multiplataforma.
            // As linhas originais 41 e 42 foram REMOVIDAS, pois forçavam o separador '\'.
            string filePathReport = Path.Combine(baseDirectory, pathReport);
            string filePathLocators = Path.Combine(baseDirectory, pathLocators);

            // AJUSTE: Criação do diretório usando o caminho ABSOLUTO (filePathReport)
            // O código original usava o caminho relativo (pathReport), o que poderia falhar.
            Directory.CreateDirectory(filePathReport);

            // Limpeza de arquivos antigos (mantido, mas usando o caminho ABSOLUTO)
            new List<string>(Directory.GetFiles(filePathReport)).ForEach(file => { if (file.EndsWith(".txt") || file.EndsWith(".PNG") || file.EndsWith(".html")) File.Delete(file); });

            // Armazenamento dos nomes dos diretórios (mantido)
            getPathReport = pathReport;
            getPathLocator = pathLocators;

            // Configuração do Extent Report (mantido)
            var htmlReport = new ExtentHtmlReporter(filePathReport);
            htmlReport.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            extent = new AventStack.ExtentReports.ExtentReports();
            extent.AttachReporter(htmlReport);

            // este trecho deleta os folders dos browsers vindo do webmanager (Mantido)
            foreach (var processName in new[] { "chromedriver", "msedgedriver", "geckodriver" })
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            string[] browserFolders = { "Chrome", "Edge", "Firefox" };
            string baseFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var browserFolder in browserFolders)
            {
                string browserFolderPath = Path.Combine(baseFolderPath, browserFolder);

                if (Directory.Exists(browserFolderPath))
                {
                    Directory.Delete(browserFolderPath, true);
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
        public static void BeforeScenario(string browser, string headless, string device)
        {
            SelectBrowser(browser, headless, device);
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

        private static string GetChromeDriver()
        {
            return new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
        }

        private static string GetEdgeDriver()
        {
            return new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
        }

        private static string GetFirefoxDriver()
        {
            return new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
        }
    }
}
