/* @author Eduardo Oliveira
*/
using Authlete.Util;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

namespace Hook_Validator
{
    public class Tools
    {
        private const int TIMEOUT_CONST = 10;
        private const int TIMEOUT_DELAY = 200;

        private static string keyAttribute { get; set; }

        private static string byAttribute { get; set; }

        private static string valueAttribute { get; set; }

        private static string getCount { get; set; }

        private static int count { get; set; }

        private static string getCorrection { get; set; }

        // Paths multiplataforma
        private static string filePath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "..", "..", "..", Hook.getPathReport.TrimStart(Path.DirectorySeparatorChar)));

        private static string pathXml = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "..", "..", "..", Hook.getPathLocator.TrimStart(Path.DirectorySeparatorChar)));

        private static By newByValue { get; set; }


        /// <summary>
        /// Navega para a página da url
        /// </summary>
        public static void OpenPage(string url)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            try
            {
                Selenium.driver.Navigate().GoToUrl(url);
                stopWatch.Stop();
            }
            catch (Exception)
            {
                if (stopWatch.IsRunning)
                    stopWatch.Stop();
            }
        }

        /// <summary>
        /// Recarrega a página
        /// </summary>
        public static void PageRefresh()
        {
            Selenium.driver.Navigate().Refresh();
        }

        /// <summary>
        /// Retorna uma página
        /// </summary>
        public static void PageBack()
        {
            Selenium.driver.Navigate().Back();
        }

        /// <summary>
        /// Avança uma página
        /// </summary>
        public static void PageForward()
        {
            Selenium.driver.Navigate().Forward();
        }

        /// <summary>
        /// Focar na tab do navegador
        /// </summary>
        public static void GetPositionTab(int position)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            try
            {
                Selenium.driver.SwitchTo().Window(Selenium.driver.WindowHandles[position]);
                stopWatch.Stop();
            }
            catch (Exception)
            {
                if (stopWatch.IsRunning)
                    stopWatch.Stop();
            }
        }

        /// <summary>
        /// Focar na tab do navegador atual
        /// </summary>
        public static void GetCurrentTab()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            try
            {
                Selenium.driver.SwitchTo().Window(Selenium.driver.WindowHandles.Last());
                stopWatch.Stop();
            }
            catch (Exception)
            {
                if (stopWatch.IsRunning)
                    stopWatch.Stop();
            }
        }

        /// <summary>
        /// Fecha a última página que foi aberta
        /// </summary>
        public static void CloseLastOpenedWindow()
        {
            string currentWindow = Selenium.driver.CurrentWindowHandle;

            //Close last opened window
            string lastOpenedWindow = Selenium.driver.WindowHandles[Selenium.driver.WindowHandles.Count - 1];
            if (lastOpenedWindow != currentWindow)
                Selenium.driver.SwitchTo().Window(lastOpenedWindow);
            Selenium.driver.Close();

            if (lastOpenedWindow != currentWindow)
            {
                //Switch back to previous current window
                Selenium.driver.SwitchTo().Window(currentWindow);
                Selenium.driver.SwitchTo().DefaultContent();
            }
        }

        /// <summary>
        /// Indica se possui um alerta presente na tela
        /// </summary>
        /// <returns>True, caso exista um alerta sendo exibido</returns>
        public static bool IsAlertPresent()
        {
            IAlert alert = ExpectedConditions.AlertIsPresent().Invoke(Selenium.driver);
            return (alert != null);
        }

        /// <summary>
        /// Texto do alerta, caso esteja sendo exibido
        /// </summary>
        public static string AlertText
        {
            get
            {
                if (IsAlertPresent())
                    return Selenium.driver.SwitchTo().Alert().Text;

                return null;
            }
        }

        /// <summary>
        /// Seleciona o folder da Solution Explorer
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static string SelectFolderSolutionExplorer(string pathFolder)
        {
            pathFolder = Path.DirectorySeparatorChar + pathFolder + Path.DirectorySeparatorChar;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePathReport = Path.Combine(baseDirectory, "..", "..", "..", pathFolder.TrimStart(Path.DirectorySeparatorChar));
            filePathReport = Path.GetFullPath(filePathReport);
            // Criar diretórios se não existirem
            Directory.CreateDirectory(filePathReport);
            return filePathReport;
        }

        /// <summary>
        /// Executa o comando click do Selenium
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ClickOnTheElement(string locator, int timeout = TIMEOUT_CONST)
        {
            try
            {
                IWebElement element = GetLocatorIWebElement(locator);
                ValidateElementVisible(element);
                element.Click();
                return true;
            }
            catch (StaleElementReferenceException)
            {
                // simplesmente tente novamente encontrar o elemento depois de atualizar a árvore de elementos
                IWebElement element = GetLocatorIWebElement(locator);
                ValidateElementVisible(element);
                element.Click();
                return true;
            }
            catch (TimeoutException e)
            {
                throw new TimeoutException("Implicit timeout error - " + e.Message);
            }
        }
        /// <summary>
        /// Executa o comando click utilizando o IWebElement
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool ClickOnTheElement(IWebElement element)
        {
            try
            {
                ValidateElementVisible(element);
                element.Click();
                return true;
            }
            catch (StaleElementReferenceException)
            {
                // simplesmente tente novamente encontrar o elemento depois de atualizar a árvore de elementos
                ValidateElementVisible(element);
                element.Click();
                return true;
            }
            catch (TimeoutException e)
            {
                throw new TimeoutException("Implicit timeout error - " + e.Message);
            }
        }

        /// <summary>
        /// Executa o comando click através de script JavaScript
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void ClickJS(string locator)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            ValidateElementVisible(element);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].click();", element);
        }

        /// <summary>
        /// Executa o comando click no IWebElement através de script JavaScript
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static void ClickJS(IWebElement element)
        {
            ValidateElementVisible(element);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].click();", element);
        }

        /// <summary>
        /// Executa o comando de enviar Key com o By locator através de script JavaScript
        /// </summary>
        /// <param name="locator">Locator do elemento</param>
        /// <param name="word">String que irá receber</param>
        public static void SendKeyJS(string locator, string word)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            ValidateElementVisible(element);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].value = '" + word + "';", element);
        }

        /// <summary>
        /// Executa o comando de enviar Key no IWebElement através de script JavaScript
        /// </summary>
        /// <param name="element">Elemento do Selenium</param>
        /// <param name="word">String que irá receber</param>
        public static void SendKeyJS(IWebElement element, string word)
        {
            ValidateElementVisible(element);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].value = '" + word + "';", element);
        }

        /// <summary>
        /// Executa o comando de enviar utilizando SendKeys Selenium com o By Locator
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool SendKeyToElement(string locator, string word)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            ValidateElementVisible(element);

            // Tabela com TODAS as teclas do Selenium
            Dictionary<string, string> specialKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Enter", Keys.Enter },
        { "Tab", Keys.Tab },
        { "Escape", Keys.Escape },
        { "Space", Keys.Space },
        { "Backspace", Keys.Backspace },
        { "Delete", Keys.Delete },
        { "Insert", Keys.Insert },
        { "Home", Keys.Home },
        { "End", Keys.End },
        { "PageUp", Keys.PageUp },
        { "PageDown", Keys.PageDown },
        { "ArrowUp", Keys.ArrowUp },
        { "ArrowDown", Keys.ArrowDown },
        { "ArrowLeft", Keys.ArrowLeft },
        { "ArrowRight", Keys.ArrowRight },
        { "F1", Keys.F1 },
        { "F2", Keys.F2 },
        { "F3", Keys.F3 },
        { "F4", Keys.F4 },
        { "F5", Keys.F5 },
        { "F6", Keys.F6 },
        { "F7", Keys.F7 },
        { "F8", Keys.F8 },
        { "F9", Keys.F9 },
        { "F10", Keys.F10 },
        { "F11", Keys.F11 },
        { "F12", Keys.F12 },
        { "Ctrl", Keys.Control },
        { "Control", Keys.Control },
        { "Shift", Keys.Shift },
        { "Alt", Keys.Alt }
    };
            //Combinações: Ctrl+A / Shift+Tab / etc
            if (word.Contains("+"))
            {
                string[] keys = word.Split('+');
                string combinedKeys = "";
                foreach (string k in keys)
                {
                    string key = k.Trim();
                    combinedKeys += specialKeys.ContainsKey(key) ? specialKeys[key] : key;
                }
                element.SendKeys(combinedKeys);
                return true;
            }
            if (specialKeys.ContainsKey(word))
            {
                element.SendKeys(specialKeys[word]);
            }
            else
            {
                element.SendKeys(word);
            }

            return true;
        }

        /// <summary>
        /// Executa o comando de enviar utilizando SendKeys Selenium com o IWebElement
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool SendKeyToElement(IWebElement element, string word)
        {
            ValidateElementVisible(element);

            Dictionary<string, string> specialKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Enter", Keys.Enter },
        { "Tab", Keys.Tab },
        { "Escape", Keys.Escape },
        { "Space", Keys.Space },
        { "Backspace", Keys.Backspace },
        { "Delete", Keys.Delete },
        { "Insert", Keys.Insert },
        { "Home", Keys.Home },
        { "End", Keys.End },
        { "PageUp", Keys.PageUp },
        { "PageDown", Keys.PageDown },
        { "ArrowUp", Keys.ArrowUp },
        { "ArrowDown", Keys.ArrowDown },
        { "ArrowLeft", Keys.ArrowLeft },
        { "ArrowRight", Keys.ArrowRight },
        { "F1", Keys.F1 },
        { "F2", Keys.F2 },
        { "F3", Keys.F3 },
        { "F4", Keys.F4 },
        { "F5", Keys.F5 },
        { "F6", Keys.F6 },
        { "F7", Keys.F7 },
        { "F8", Keys.F8 },
        { "F9", Keys.F9 },
        { "F10", Keys.F10 },
        { "F11", Keys.F11 },
        { "F12", Keys.F12 },
        { "Ctrl", Keys.Control },
        { "Control", Keys.Control },
        { "Shift", Keys.Shift },
        { "Alt", Keys.Alt }
    };
            if (word.Contains("+"))
            {
                string[] keys = word.Split('+');
                string combinedKeys = "";

                foreach (string k in keys)
                {
                    string key = k.Trim();
                    combinedKeys += specialKeys.ContainsKey(key) ? specialKeys[key] : key;
                }

                element.SendKeys(combinedKeys);
                return true;
            }
            if (specialKeys.ContainsKey(word))
            {
                element.SendKeys(specialKeys[word]);
            }
            else
            {
                element.SendKeys(word);
            }

            return true;
        }


        /// <summary>
        /// Executa o comando de enviar palavra secreta do properties utilizando a SendKeys Selenium com o By Locator
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool SendSecretToElement(string locator, string word)
        {
            string keyValue = stringSecret(word);
            IWebElement element = GetLocatorIWebElement(locator);
            ValidateElementVisible(element);
            element.SendKeys(keyValue);
            return true;
        }

        /// <summary>
        /// Executa o comando de enviar palavra secreta do properties utilizando SendKeys Selenium com o IWebElement
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool SendSecretToElement(IWebElement element, string word)
        {
            string keyValue = stringSecret(word);
            ValidateElementVisible(element);
            element.SendKeys(keyValue);
            return true;
        }

        /// <summary>
        /// Executa scroll utilizando o eixo y e eixo y
        /// </summary>
        /// <param name="horizon">Posição horizontal</param>
        /// <param name="vertical">Posição vertical</param>
        public static void ScrollElementWithPosition(int horizon, int vertical)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("window.scrollBy('" + horizon + "', '" + vertical + "')", "");
        }

        //public static void ScrollElement(IWebElement element)
        //{
        //    WaitElementByIWebElement(element);
        //    IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
        //    executor.ExecuteScript("arguments[0].scrollIntoView();", element);
        //    ValidateElementVisible(element);
        //}

        /// <summary>
        /// Executa scroll utilizando view do elemento
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>  
        public static void ScrollElement(string locator, int padding = 100)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            string paddingCommand = "";
            if (padding > 0)
                paddingCommand = $"window.scrollBy(0, -{padding})";
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].scrollIntoView(true);" + paddingCommand, element);
            ValidateElementVisible(element);
        }

        /// <summary>
        /// Executa scroll utilizando view do elemento
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static void ScrollElement(IWebElement element, int padding = 100)
        {
            WaitElement(element);
            string paddingCommand = "";
            if (padding > 0)
                paddingCommand = $"window.scrollBy(0, -{padding})";
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].scrollIntoView(true);" + paddingCommand, element);
            ValidateElementVisible(element);
        }

        /// <summary>
        /// Executa scroll utilizando view do elemento
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static void ScrollElementWithTime(IWebElement element, int time)
        {
            WaitElementWithTime(element, time);
            ValidateElementVisible(element);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Selenium.driver;
            executor.ExecuteScript("arguments[0].scrollIntoView();", element);
        }

        /// <summary>
        /// Faz uma pintura verde no elemento significando que ele o elemento existe
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool HighlightElementPass(string locator)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            bool existe = false;
            if (element != null)
            {
                existe = true;
                IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
                javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                        element, "outline: 4px solid #00FF00;");
            }
            if (existe == false)
            {
                Assert.IsTrue(existe, "Favor verificar o locator --> " + locator + " no arquivo xml");
            }
            return true;
        }

        /// <summary>
        /// Faz uma pintura verde no elemento significando que ele o elemento existe
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool HighlightElementPass(IWebElement element)
        {
            WaitElement(element);
            bool existe = false;
            if (element != null)
            {
                existe = true;
                IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
                javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                        element, "outline: 4px solid #00FF00;");
                return true;
            }
            if (existe == false)
            {
                Assert.IsTrue(existe, "Favor verificar o elemento --> " + element + " é diferente ou não está disponível na tela");
            }
            return true;
        }

        /// <summary>
        /// Faz uma pintura vermelha no elemento significando que ele é diferente o esperado
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool HighlightElementFail(string locator)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            bool existe = true;
            if (element != null)
            {
                existe = false;
                IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
                javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                        element, "outline: 4px solid #FF0000;");
            }
            if (existe == false)
            {
                Assert.IsTrue(existe, "Favor verificar o locator --> " + locator + " no arquivo xml");
            }
            return true;
        }

        /// <summary>
        /// Faz uma pintura vermelha no elemento significando que ele é diferente o esperado
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool HighlightElementFail(IWebElement element)
        {
            WaitElement(element);
            bool existe = false;
            if (element != null)
            {
                existe = false;
                IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
                javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                        element, "outline: 4px solid #FF0000;");
            }
            if (existe == false)
            {
                Assert.IsTrue(existe, "Favor verificar o elemento --> " + element + " é diferente ou não está disponível na tela");
            }
            return false;
        }

        /// <summary>
        /// Faz uma pintura verde no elemento significando que ele o elemento existe
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        private static bool HighlightElementPassMethod(string locator)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
            javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                    element, "outline: 4px solid #00FF00;");
            return true;
        }

        /// <summary>
        /// Faz uma pintura verde no elemento significando que ele o elemento existe
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        private static bool HighlightElementPassMethod(IWebElement element)
        {
            WaitElement(element);
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
            javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                    element, "outline: 4px solid #00FF00;");
            return true;
        }

        /// <summary>
        /// Faz uma pintura vermelha no elemento significando que ele é diferente o esperado
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        private static bool HighlightElementFailMethod(string locator)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
            javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                    element, "outline: 4px solid #FF0000;");
            return true;
        }

        /// <summary>
        /// Faz uma pintura vermelha no elemento significando que ele é diferente o esperado
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        private static bool HighlightElementFailMethod(IWebElement element)
        {
            WaitElement(element);
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Selenium.driver;
            javaScriptExecutor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                    element, "outline: 4px solid #FF0000;");
            return true;
        }

        /// <summary>
        /// Obtém o elemento filho dado o índice
        /// </summary>
        /// <param name="locator">Elemento buscado do xml</param>
        /// <param name="index">Índice do filho</param>
        /// <param name="tagName">Caso queira filtrar os filhos por Tag</param>
        /// <param name="throwException">Indica se deve lançar a exceção <see cref="NoSuchElementException"/>, caso não exista filho no índice. Se for false, retornará nulo.</param>
        /// <returns>Elemento filho</returns>
        public string GetChild(string locator, int index, string tagName = null, bool throwException = true)
        {
            List<WebElement> elements = GetLocatorForWebElement(locator);
            elements = GetChildren(tagName);
            if (index >= elements.Count)
            {
                if (throwException)
                    throw new NoSuchElementException();
                else
                    return null;
            }
            string elementIndex = elements[index].ToString().Replace("Element (id = ", "").Replace(")", "");
            return elementIndex;
        }

        private List<WebElement> GetLocatorForWebElement(string locator)
        {
            By locatorList = GetLocator(locator);
            IReadOnlyCollection<WebElement> elements = GetElements(locatorList);
            List<WebElement> list = elements.ToList();
            return list;
        }

        /// <summary>
        /// Obtém todos os elementos filhos
        /// </summary>
        /// <param name="tagName">Caso queira filtrar os filhos por Tag</param>
        /// <returns>Lista de elementos filhos do elemento</returns>
        private List<WebElement> GetChildren(string tagName = null)
        {
            return GetChildren(true, tagName);
        }

        /// <summary>
        /// Obtém todos os elementos dada a estratégia
        /// </summary>
        /// <param name="by">Estratégia de busca</param>
        /// <param name="log">Indica se deve logar a ação</param>
        /// <returns>Elementos</returns>
        readonly WebDriver att;
        private List<WebElement> GetElements(By by, bool log = true)
        {
            var elements = Selenium.driver.FindElements(by);
            return (from child in elements
                    select new WebElement(att, child.Text)).ToList();
        }

        private List<WebElement> GetChildren(bool log, string tagName = null)
        {
            if (tagName != null)
                return GetElements(By.TagName(tagName), log);
            else
                return GetElements(By.XPath("./child::*"), log);
        }

        /// <summary>
        /// Obtém o texto do elemento.
        /// Caso o elemento não possua texto, será utilizado o seu atributo 'innerText'.
        /// </summary>
        /// <returns>string contendo o texto do elemento</returns>
        public string GetInnerText(string locator)
        {
            var element = SmartFindElement(locator);
            string innerText = element.GetAttribute("innerText");
            return innerText;
        }

        public string GetInnerText(IWebElement element)
        {
            string innerText = element.GetAttribute("innerText");
            return innerText;
        }

        /// <summary>
        /// Obtém o texto do elemento.
        /// Caso o elemento não possua texto, será utilizado o seu atributo 'value'.
        /// </summary>
        /// <returns>string contendo o texto do elemento</returns>
        public string GetValueText(string locator)
        {
            var element = SmartFindElement(locator);
            string valueText = element.GetAttribute("value");
            return valueText;
        }

        public string GetValueText(IWebElement element)
        {
            string valueText = element.GetAttribute("value");
            return valueText;
        }


        /// <summary>
        /// Valida se o elemento existe e esteja visivel 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ValidateElementVisible(string locator)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            ValidateElementVisible(element);
            return true;
        }

        /// <summary>
        /// Faz a comparação de igualdade do texto que consta no elemento com uma string 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ValidateInnerTextEquals(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            ValidateElementInnerTextEquals(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação de igualdade do texto que consta no elemento com uma string 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool ValidateInnerTextEquals(IWebElement element, string word)
        {
            WaitElement(element);
            ValidateElementInnerTextEquals(element, word);
            return true;
        }

        /// <summary>
        /// Faz correção pegando o innertext do elemento e sobrescrevendo nos scripts
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void SelfCorrectionBddInnerTextEquals(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            SelfCorrectionElementInnerTextEquals(element, word);
        }

        /// <summary>
        /// Faz correção pegando o innertext do elemento e sobrescrevendo nos scripts
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void SelfCorrectionBddInnerTextEquals(IWebElement element, string word)
        {
            SelfCorrectionElementInnerTextEquals(element, word);
        }

        /// <summary>
        /// Faz a comparação de contains do texto do elemento com uma string 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ValidateInnerTextContains(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            ValidateElementInnerTextContains(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação de contains do texto do elemento com uma string 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool ValidateInnerTextContains(IWebElement element, string word)
        {
            WaitElement(element);
            ValidateElementInnerTextContains(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação do texto de um textbox do elemento com uma string 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ValidateValueTextEquals(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            ValidateElementValueTextEquals(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação do texto de um textbox do elemento com uma string 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool ValidateValueTextEquals(IWebElement element, string word)
        {
            WaitElement(element);
            ValidateElementValueTextEquals(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação do texto de um textbox do elemento com uma string 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static bool ValidateValueTextContains(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            ValidateElementValueTextContains(element, word);
            return true;
        }

        /// <summary>
        /// Faz a comparação do texto de um textbox do elemento com uma string 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool ValidateValueTextContains(IWebElement element, string word)
        {
            WaitElement(element);
            ValidateElementValueTextContains(element, word);
            return true;
        }

        /// <summary>
        /// Faz correção pegando o innertext do elemento e sobrescrevendo nos scripts
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void SelfCorrectionBddElementValueTextEquals(string locator, string word)
        {
            IWebElement element = WaitElementForMethod(Selenium.driver, locator);
            SelfCorrectionElementValueTextEquals(element, word);
        }

        /// <summary>
        /// Faz correção pegando o innertext do elemento e sobrescrevendo nos scripts
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void SelfCorrectionBddElementValueTextEquals(IWebElement element, string word)
        {
            SelfCorrectionElementValueTextEquals(element, word);
        }

        /// <summary>
        /// Seleciona todas as opções de acordo com o texto
        /// </summary>
        /// <param name="option">Texto das opções a serem selecionadas</param>
        /// <param name="matchWholeWord">Booleano indicando se o texto deve ser exato ou apenas estar contido</param>
        public static void SelectComboBoxText(string locator, string option, bool matchWholeWord = true)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByText(option, !matchWholeWord);
        }

        /// <summary>
        /// Seleciona todas as opções de acordo com o texto
        /// </summary>
        /// <param name="option">Texto das opções a serem selecionadas</param>
        /// <param name="matchWholeWord">Booleano indicando se o texto deve ser exato ou apenas estar contido</param>
        public static void SelectComboBoxText(IWebElement element, string option, bool matchWholeWord = true)
        {
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByText(option, !matchWholeWord);
        }

        /// <summary>
        /// Seleciona uma das opções de acordo com o índice
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="index">Índice</param>
        public static void SelectComboBoxIndex(string locator, int index)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByIndex(index);
        }

        /// <summary>
        /// Seleciona uma das opções de acordo com o índice
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        /// <param name="index">Índice</param>
        public static void SelectComboBoxIndex(IWebElement element, int index)
        {
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByIndex(index);
        }

        /// <summary>
        /// Seleciona uma das opções de acordo com o valor
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param> 
        /// <param name="value">Valor</param>
        public static void SelectComboBoxValue(string locator, int value)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByIndex(value);
        }

        /// <summary>
        /// Seleciona uma das opções de acordo com o valor
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        /// <param name="value">Valor</param>
        public static void SelectComboBoxValue(IWebElement element, int value)
        {
            WaitElement(element);
            SelectElement select = new SelectElement(element);
            select.SelectByIndex(value);
        }

        /// <summary>
        /// Invoca o Wait para usar estrutura Thread sleep
        /// </summary>
        /// <param name="timeout">Insere valor inteiro</param>
        public static bool Wait(int timeout = TIMEOUT_DELAY)
        {
            Thread.Sleep(timeout);
            return false;
        }

        /// <summary>
        /// Invoca o WaitElement programado com a constante de 10 segundos
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool WaitElement(IWebElement element, int timeout = TIMEOUT_CONST)
        {
            int count = 0;

            do
            {
                try
                {
                    return element.Displayed && element.Enabled;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                    count++;
                }

            } while (count < timeout * 2);

            return false;
        }

        /// <summary>
        /// Invoca o WaitElement programado com a constante de 10 segundos
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        private static bool WaitElementSelfCorrection(IWebElement element, int timeout)
        {
            int count = 0;
            do
            {
                try
                {
                    return element.Displayed && element.Enabled;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                    count++;
                }
            } while (count < timeout * 2);

            return false;
        }

        /// <summary>
        /// Invoca o WaitElement habilitando o campo para inserir o tempo em segundos 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static bool WaitElementWithTime(IWebElement element, int timeout)
        {
            int count = 0;
            do
            {
                try
                {
                    return element.Displayed && element.Enabled;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                    count++;
                }

            } while (count < timeout * 2);

            return false;
        }

        /// <summary>
        /// Espera um elemento que seja clicável habilitando o campo para inserir o tempo desejável
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="timeout">Inserir o número de segundos de espera</param>
        public static IWebElement WaitElementToBeClickable(string locator, int timeout)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(byLocator));
            return element;
        }

        /// <summary>
        /// Espera um elemento que seja clicável
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        /// <param name="timeout">Inserir o número de segundos de espera</param>
        public static IWebElement WaitElementToBeClickable(IWebElement element, int timeout)
        {
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement elemen = wait.Until(ExpectedConditions.ElementToBeClickable(element));
            return elemen;
        }

        /// <summary>
        /// Espera a invisibilidade do elemento quando ele aparecer
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="timeout">Inserir o número de segundos de espera</param>
        public static bool WaitElementInvisibility(string locator, int timeout)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool element = wait.Until(ExpectedConditions.InvisibilityOfElementLocated(byLocator));
            return element;
        }

        /// <summary>
        /// Espera a invisibilidade do elemento que apresenta um texto quando ele aparecer
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="text">String que vai receber</param>
        public static bool WaitElementInvisibilityWithText(string locator, string text, int timeout)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool element = wait.Until(ExpectedConditions.InvisibilityOfElementWithText(byLocator, text));
            return element;
        }

        /// <summary>
        /// Espera um texto aparecer no elemento 
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="text">String que vai receber</param>
        public static bool WaitElementTextToBePresent(string locator, string text, int timeout)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool elemen = wait.Until(ExpectedConditions.TextToBePresentInElement(element, text));
            return elemen;
        }

        /// <summary>
        /// Espera um texto aparecer no elemento 
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        /// <param name="text">String que vai receber</param>
        public static bool WaitElementTextToBePresent(IWebElement element, string text, int timeout)
        {
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool elemen = wait.Until(ExpectedConditions.TextToBePresentInElement(element, text));
            return elemen;
        }

        /// <summary>
        /// Espera um texto aparecer no elemento tipo textbox  
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="text">String que vai receber</param>
        public static bool WaitElementTextToBePresentInValue(string locator, string text, int timeout)
        {
            IWebElement element = GetLocatorIWebElement(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool elemen = wait.Until(ExpectedConditions.TextToBePresentInElementValue(element, text));
            return elemen;
        }

        /// <summary>
        /// Espera um texto aparecer no elemento tipo textbox  
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        /// <param name="text">String que vai receber</param>
        public static bool WaitElementTextToBePresentInValue(IWebElement element, string text, int timeout)
        {
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            bool elemen = wait.Until(ExpectedConditions.TextToBePresentInElementValue(element, text));
            return elemen;
        }

        /// <summary>
        ///  Executa o WaiElement com parametro string do locator com padrão da constante 10 segundos
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IWebElement WaitElement(string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            return element;
        }

        /// <summary>
        ///  Executa o WaiElement com parametro string do locator habilitado o campo para inserir o número em segundos
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IWebElement WaitElementWithTime(string locator, int timeout)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            return element;
        }

        /// <summary>
        ///  Cria evidencia inserindo um count
        ///  Exemplo: Foto1, Foto2, Foto3....
        ///  Evita de sobrescrever as antigas
        /// </summary>
        public static string Screenshot()
        {
            string strLog = "Erro";
            string filePathPng = Path.Combine(filePath, strLog + ".PNG");
            bool existe = false;

            // Garantir que o diretório existe
            Directory.CreateDirectory(filePath);

            while (File.Exists(filePathPng))
            {
                existe = true;
                writeFileTxt();
                readFileTxt();
                string fileCount = string.Format("{0}({1})", strLog, getCount);
                filePathPng = Path.Combine(filePath, fileCount + ".PNG");
                ((ITakesScreenshot)Selenium.driver).GetScreenshot().SaveAsFile(filePathPng, ScreenshotImageFormat.Png);
                break;
            }
            if (existe == false)
            {
                ((ITakesScreenshot)Selenium.driver).GetScreenshot().SaveAsFile(filePathPng, ScreenshotImageFormat.Png);
            }
            return filePathPng;
        }

        /// <summary>
        ///  Antes de começar o projeto crie um método static void e execute este método.
        ///  Ele irá gerar as seguintes pastas:
        ///   - Pages
        ///   - Locators
        ///   - Locators
        ///   - Report
        ///   - Resources
        ///   - Features
        ///   - Steps
        /// </summary>
        public static void createTemplate(string folderName)
        {
            var solutionName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string splitName = solutionName.Split('.')[0];

            // Path multiplataforma para o arquivo .csproj
            string baseDir = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", ".."));
            string filename = Path.Combine(baseDir, splitName + ".csproj");
            string createFolder = baseDir + Path.DirectorySeparatorChar;

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
            docFrag.InnerXml = "<ItemGroup> </ItemGroup>";
            doc.DocumentElement.AppendChild(docFrag);
            XmlNodeList nosItemGroup = doc.GetElementsByTagName("ItemGroup");
            foreach (XmlNode no in nosItemGroup)
            {
                if (no.FirstChild.Name == "#whitespace")
                {
                    List<string> folders = new List<string>();
                    folders.Add("_Pages");
                    folders.Add("_Locators");
                    folders.Add("_Report");
                    folders.Add("_Resources");
                    folders.Add("_Features");
                    folders.Add("_Steps");
                    foreach (string item in folders)
                    {
                        XmlNode newNo = doc.CreateElement("Folder", no.NamespaceURI);
                        XmlAttribute newAttrib = doc.CreateAttribute("Include");
                        newAttrib.Value = "" + folderName + item + Path.DirectorySeparatorChar;
                        newNo.Attributes.Append(newAttrib);
                        no.AppendChild(newNo);
                        Directory.CreateDirectory(Path.Combine(createFolder, folderName + item));
                    }
                }
            }
            doc.Save(filename);
            List<string> listCsproj = File.ReadAllLines(filename).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
            List<string> replaceOutputType = listCsproj.Select(x => x.Replace("WinExe", "Library")).ToList();
            File.WriteAllLines(filename, replaceOutputType);
        }

        /// <summary>
        ///  Executa o WaiElement com parametro string do locator com padrão da constante 10 segundos
        ///  Poderá executar qualquer método Selenium acompanhado.
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IWebElement SmartFindElement(string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            ValidateElementVisible(element);
            return element;
        }

        /// <summary>
        ///  Executa o WaiElement de listas com parametro string do locator com padrão da constante 10 segundos
        ///  Poderá executar qualquer método Selenium acompanhado
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IList<IWebElement> SmartFindElements(string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            IList<IWebElement> returnElementList = null;
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));

            try
            {
                wait.Until(d =>
                {
                    var elements = d.FindElements(byLocator);
                    if (elements.Count > 0) returnElementList = elements;
                    return elements.Count > 0;
                    //isso esperará até encontrar pelo menos 1 elemento, caso contrário, deve retornar nulo, não falhar
                });
            }
            catch { }

            return returnElementList;
        }

        /// <summary>
        ///  Método Action do Selenium para movimentar até o elemento sem clicar
        /// </summary>
        /// <param name="element">Elemento onde sera apenas ativado quando o action executar</param>
        public static void ActionDragAndDrop(IWebElement element)
        {
            WaitElement(element);
            Actions actions = new Actions(Selenium.driver);
            actions.MoveToElement(element).Perform();
        }

        /// <summary>
        ///  Método Action do Selenium para movimentar até o elemento sem clicar
        /// </summary>
        /// <param name="element">Elemento onde sera apenas ativado quando o action executar</param>
        public static void ActionDragAndDrop(string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            Actions actions = new Actions(Selenium.driver);
            actions.MoveToElement(element).Perform();
        }

        /// <summary>
        ///  Método Action do Selenium para arrastar o elemento e soltar no destino final
        /// </summary>
        /// <param name="elementInitial">Elemento inicial que deverá se mover para o final</param>
        /// <param name="elementFinal">Elemento final que irá receber o elemento inicial</param>
        public static void ActionDragAndDrop(IWebElement elementInitial, IWebElement elementFinal)
        {
            WaitElement(elementInitial);
            Actions action = new Actions(Selenium.driver);
            ValidateElementVisible(elementInitial);
            action.ClickAndHold(elementInitial);
            ValidateElementVisible(elementFinal);
            action.MoveToElement(elementFinal)
                .Release(elementFinal)
                .Build()
                .Perform();
        }

        /// <summary>
        ///  Método Action do Selenium para arrastar o elemento e soltar no destino final
        /// </summary>
        /// <param name="locatorInitial">Recebe string locator inicial que deverá se mover para o final</param>
        /// <param name="locatorFinal">Locator string final que irá receber a string locator inicial</param>
        public static void ActionDragAndDrop(string locatorInitial, string locatorFinal, int timeout = TIMEOUT_CONST)
        {
            By byLocatorInitial = GetLocator(locatorInitial);
            By byLocatorFinal = GetLocator(locatorFinal);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement elementInitial = wait.Until(ExpectedConditions.ElementIsVisible(byLocatorInitial));
            IWebElement elementFinal = wait.Until(ExpectedConditions.ElementIsVisible(byLocatorFinal));
            Actions action = new Actions(Selenium.driver);
            ValidateElementVisible(elementInitial);
            action.ClickAndHold(elementInitial);
            ValidateElementVisible(elementFinal);
            action.MoveToElement(elementFinal)
                .Release(elementFinal)
                .Build()
                .Perform();
        }

        /// <summary>
        ///  Método Action do Selenium que executa dois cliques no elemento
        /// </summary>
        /// <param name="element">Element referente o elemento instanciado pelo Selenium</param>
        public static void ActionDoubleClick(IWebElement element)
        {
            WaitElement(element);
            Actions action = new Actions(Selenium.driver);
            ValidateElementVisible(element);
            action.DoubleClick(element)
                .Build()
                .Perform();
        }

        /// <summary>
        ///  Método Action do Selenium que executa dois cliques no elemento
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static void ActionDoubleClick(string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            Actions action = new Actions(Selenium.driver);
            ValidateElementVisible(element);
            action.DoubleClick(element)
                .Build()
                .Perform();
        }

        /// <summary>
        ///  Método em que o IWebElement pode receber convertido o objeto By
        ///  Exemplo:
        ///  IWebElement teste = GetLocatorIWebElement(string locator);
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IWebElement GetLocatorIWebElement(string locator)
        {
            IWebElement elementLocator = Selenium.driver.FindElement(GetLocator(locator));
            IWebElement element = elementLocator;
            return element;
        }

        /// <summary>
        ///  Método em que a IList de IWebElement pode receber convertido o objeto By
        ///  Exemplo:
        ///  IList <IWebElement> teste = GetLocatorList(string locator);
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static IList<IWebElement> GetLocatorList(string locator)
        {
            IReadOnlyCollection<IWebElement> elementsList = Selenium.driver.FindElements(GetLocator(locator));
            IList<IWebElement> list = elementsList.ToList();
            return list;
        }

        /// <summary>
        ///  Método que entra no arquivo xml e busca o locator através da string
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        public static By GetLocator(string locator)
        {
            string featureName = FeatureContext.Current.FeatureInfo.Title;

            // Garantir que o diretório existe
            Directory.CreateDirectory(pathXml);

            foreach (string xmlName in Directory.GetFiles(pathXml, "*.xml", SearchOption.TopDirectoryOnly)
                .Select(Path
                .GetFileName)
                .ToArray())
            {
                List<XmlNode> xmlNodeList = new List<XmlNode>();
                string[] filePaths = Directory.GetFiles(pathXml);
                for (int i = 0, j = filePaths.Length; i < j; i++)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePaths[i]);
                    XmlNode rootNode = doc.SelectSingleNode("locators");
                    foreach (XmlNode node in rootNode.SelectNodes("element"))
                        xmlNodeList.Add(node);
                }
                keyAttribute = null;
                valueAttribute = null;
                byAttribute = null;
                foreach (XmlNode node in xmlNodeList)
                {
                    var teste = node.Attributes["key"].Value.ToString().Trim();
                    if (node.Attributes["key"].Value.ToString().Trim().Equals(locator))
                    {
                        keyAttribute = node.Attributes["key"].Value.ToString().Trim();
                        byAttribute = node.Attributes["by"].Value.ToString().Trim();
                        valueAttribute = node.Attributes["value"].Value.ToString().Trim();
                        break;
                    }
                }
            }
            By b = null;
            if (valueAttribute != null)
            {

                switch (byAttribute)
                {
                    case "id":
                        b = By.Id(valueAttribute);
                        break;
                    case "class":
                        b = By.ClassName(valueAttribute);
                        break;
                    case "css":
                        b = By.CssSelector(valueAttribute);
                        break;
                    case "xpath":
                        b = By.XPath(valueAttribute);
                        break;
                    case "name":
                        b = By.Name(valueAttribute);
                        break;
                    case "tag":
                        b = By.TagName(valueAttribute);
                        break;
                    case "link":
                        b = By.LinkText(valueAttribute);
                        break;
                    default:
                        break;
                }
            }
            try
            {
                IWebElement element = Selenium.driver.FindElement(b);
                WaitElementSelfCorrection(element, 15);
                IntelligentUpdateXml(locator, element);
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                IntelligentUpdateXml(locator);
            }
            if (b == null) throw new NotFoundException("Não foi localizado o locator ( " + locator + " )");
            b = newByValue;
            return b;
        }

        /// <summary>
        ///  Método inteligencia artificial de utilizar o xpath absoluto para sempre atualizar os locators
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="element">Converter o locator para IWebElement</param>
        private static void IntelligentUpdateXml(string locator, IWebElement element)
        {
            foreach (string xmlName in Directory.GetFiles(pathXml, "*.xml", SearchOption.TopDirectoryOnly)
                .Select(Path
                .GetFileName)
                .ToArray())
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(Path.Combine(pathXml, xmlName));
                XmlNode root = xml.DocumentElement;
                IEnumerator ie = root.SelectNodes("element").GetEnumerator();
                string byActual = null;
                string valueActual = null;
                while (ie.MoveNext())
                {
                    if ((ie.Current as XmlNode).Attributes["key"].Value == locator)
                    {
                        byActual = (ie.Current as XmlNode).Attributes["by"].Value;
                        valueActual = (ie.Current as XmlNode).Attributes["value"].Value;
                        if (string.IsNullOrEmpty((ie.Current as XmlNode).Attributes["baseValue"].Value))
                        {
                            getCorrection = getElementAbsoluteXpath(element);
                        }
                        else
                        {
                            getCorrection = (ie.Current as XmlNode).Attributes["baseValue"].Value;
                        }
                    }
                }
                xml.Save(Path.Combine(pathXml, xmlName));
                GetCurrentTab();
                string idElement = getElementId();
                // string cssElement = getElementCssSelector();
                string xpathElement = getElementRelativeXpath();
                string by = null, value = null;

                if (string.IsNullOrEmpty(idElement))
                {
                    by = "xpath";
                    value = xpathElement;
                    newByValue = By.XPath(value);
                }
                else
                {
                    by = "id";
                    value = idElement;
                    newByValue = By.Id(value);
                }
                XmlDocument xmlUpdate = new XmlDocument();
                xmlUpdate.Load(Path.Combine(pathXml, xmlName));
                XmlNode roott = xmlUpdate.DocumentElement;
                IEnumerator iee = roott.SelectNodes("element").GetEnumerator();
                while (iee.MoveNext())
                {
                    if ((iee.Current as XmlNode).Attributes["key"].Value == locator)
                    {
                        (iee.Current as XmlNode).Attributes["by"].Value = by;
                        (iee.Current as XmlNode).Attributes["value"].Value = value;
                        (iee.Current as XmlNode).Attributes["baseValue"].Value = getCorrection;
                    }
                }
                xmlUpdate.Save(Path.Combine(pathXml, xmlName));
                List<string> list = File.ReadAllLines(Path.Combine(pathXml, xmlName)).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
                List<string> listReplace = list.Select(x => x.Replace("&gt;", ">")
                .Replace("&lt", "<")
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&"))
                    .ToList();
                File.WriteAllLines(Path.Combine(pathXml, xmlName), listReplace);
            }
        }

        /// <summary>
        ///  Método inteligencia artificial de utilizar o xpath absoluto para sempre atualizar os locators
        /// </summary>
        /// <param name="locator">Locator referente o elemento do arquivo xml</param>
        /// <param name="element">Converter o locator para IWebElement</param>
        private static void IntelligentUpdateXml(string locator)
        {
            foreach (string xmlName in Directory.GetFiles(pathXml, "*.xml", SearchOption.TopDirectoryOnly)
              .Select(Path
              .GetFileName)
              .ToArray())
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(Path.Combine(pathXml, xmlName));
                XmlNode root = xml.DocumentElement;
                IEnumerator ie = root.SelectNodes("element").GetEnumerator();
                while (ie.MoveNext())
                {
                    if ((ie.Current as XmlNode).Attributes["key"].Value == locator)
                    {
                        getCorrection = (ie.Current as XmlNode).Attributes["baseValue"].Value;
                    }
                }
                xml.Save(Path.Combine(pathXml, xmlName));
                GetCurrentTab();
                string idElement = getElementId();
                // string cssElement = getElementCssSelector();
                string xpathElement = getElementRelativeXpath();
                string by = null, value = null;

                if (string.IsNullOrEmpty(idElement))
                {
                    by = "xpath";
                    value = xpathElement;
                    newByValue = By.XPath(value);
                }
                else
                {
                    by = "id";
                    value = idElement;
                    newByValue = By.Id(value);
                }
                XmlDocument xmlUpdate = new XmlDocument();
                xmlUpdate.Load(Path.Combine(pathXml, xmlName));
                XmlNode roott = xmlUpdate.DocumentElement;
                IEnumerator iee = roott.SelectNodes("element").GetEnumerator();
                while (iee.MoveNext())
                {
                    if ((iee.Current as XmlNode).Attributes["key"].Value == locator)
                    {
                        (iee.Current as XmlNode).Attributes["by"].Value = by;
                        (iee.Current as XmlNode).Attributes["value"].Value = value;
                    }
                }
                xmlUpdate.Save(Path.Combine(pathXml, xmlName));
                List<string> list = File.ReadAllLines(Path.Combine(pathXml, xmlName)).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
                List<string> listReplace = list.Select(x => x.Replace("&gt;", ">")
                .Replace("&lt", "<")
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&"))
                    .ToList();
                File.WriteAllLines(Path.Combine(pathXml, xmlName), listReplace);
            }
        }

        /// <summary>
        ///  Método de extração do xpath absoluto através do IJavaScript Selenium com parametro IWebElement
        /// </summary>
        /// <param name="element">Parametro IWebElement que será feita a extração</param>
        private static string getElementAbsoluteXpath(IWebElement element)
        {
            Thread.Sleep(200);
            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
            string _result = (string)jsExec.ExecuteScript(
@"
function getPathTo(element) {
    if (element === document.body)
        return '/html/' + element.tagName.toLowerCase();

    var ix = 0;
    var siblings = element.parentNode.childNodes;
    for (var i = 0; i < siblings.length; i++) {
        var sibling = siblings[i];
        if (sibling === element)
        {
            return getPathTo(element.parentNode) + '/' + element.tagName.toLowerCase() + '[' + (ix + 1) + ']';
        }
        if (sibling.nodeType === 1 && sibling.tagName === element.tagName)
            ix++;
    }
}
var element = arguments[0];
var xpath = '';
xpath = getPathTo(element);
return xpath;
", element);
            return _result;
        }

        /// <summary>
        ///  Método de extração do ID através do IJavaScript Selenium com parametro IWebElement
        /// </summary>
        private static string getElementId()
        {
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(15));
            WebElement webElement = (WebElement)wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(getCorrection)));
            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
            string _result = QuoteLocator((string)jsExec.ExecuteScript(
@"
var ELEMENT_NODE = 1;
function getId(element) {
         var selector = ''; 
         if (element instanceof Element && element.nodeType === ELEMENT_NODE && element.id) {
             selector = element.id;
         }
         return selector;
     }
var element = arguments[0];
var id = '';
id = getId(element);
return id;
", webElement));
            return _result;
        }

        //        /// <summary>
        //        ///  Método de extração do CssSelector absoluto através do IJavaScript Selenium com parametro IWebElement
        //        /// </summary>
        //        private static string getElementCssSelector()
        //        {
        //            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(15));
        //            IWebElement webElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(getCorrection)));
        //            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
        //            string _result = QuoteLocator((string)jsExec.ExecuteScript(
        //@"
        //var ELEMENT_NODE = 1;
        //function getCss(element) {
        //         if (!(element instanceof Element))
        //             return;
        //         var path = [];
        //         while (element.nodeType === ELEMENT_NODE) {
        //             var selector = element.nodeName.toLowerCase();
        //             if (element.id) {
        //                 if (element.id.indexOf('-') > -1) {
        //                     selector += '[id = ""' + element.id + '""]';
        //                 } else {
        //                     selector += '#' + element.id;
        //                 }
        //                 path.unshift(selector);
        //                 break;
        //             } else {
        //                 var element_sibling = element;
        //                 var sibling_cnt = 1;
        //                 while (element_sibling = element_sibling.previousElementSibling) {
        //                     if (element_sibling.nodeName.toLowerCase() == selector)
        //                         sibling_cnt++;
        //                 }
        //                 if (sibling_cnt != 1)
        //                     selector += ':nth-of-type(' + sibling_cnt + ')';
        //             }
        //             path.unshift(selector);
        //             element = element.parentNode;
        //         }
        //         return path.join(' > ');
        //}
        //var element = arguments[0];
        //var css = '';
        //css = getCss(element);
        //return css;
        //", webElement));
        //            return _result;
        //        }

        /// <summary>
        ///  Método de extração do CssSelector absoluto através do IJavaScript Selenium com parametro IWebElement
        /// </summary>
        public static string getElementCssSelector()
        {
            IWebElement webElement = Selenium.driver.FindElement(By.XPath(getCorrection));
            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
            string _result = QuoteLocator((string)jsExec.ExecuteScript(
    @"
function getCss(element) {
         return `${element.nodeName}${element.id ? '#'+element.id : ''}${element.getAttribute('class') ? '.'+element.getAttribute('class').split(' ').join('.') : ''}`;
}
var element = arguments[0];
var css = '';
css = getCss(element);
return css;
", webElement));
            return _result;
        }

        //        /// <summary>
        //        ///  Método de extração do xpath relativo através do IJavaScript Selenium com parametro IWebElement
        //        /// </summary>
        //        private static string getElementRelativeXpath()
        //        {
        //            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(15));
        //            IWebElement webElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(getCorrection)));
        //            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
        //            string _result = QuoteLocator((string)jsExec.ExecuteScript(
        //@"
        //var ELEMENT_NODE = 1;
        //function getRelativeXpath(element) {
        //    var element_sibling, siblingTagName, siblings, cnt, sibling_count;
        //        var elementTagName = element.tagName.toLowerCase();
        //        if (element.id != '') {
        //            return 'id(""' + element.id + '"")';
        //            // alternative : 
        //            // return '*[@id=""' + element.id + '""]';
        //        } else if (element.name && document.getElementsByName(element.name).length === 1) {
        //            return '//' + elementTagName + '[@name=""' + element.name + '""]';
        //        }
        //        if (element === document.body) {
        //            return '/html/' + elementTagName;
        //        }
        //        sibling_count = 0;
        //        siblings = element.parentNode.childNodes;
        //        siblings_length = siblings.length;
        //        for (cnt = 0; cnt < siblings_length; cnt++) {
        //            var element_sibling = siblings[cnt];
        //            if (element_sibling.nodeType !== ELEMENT_NODE) { // not ELEMENT_NODE
        //                continue;
        //            }
        //            if (element_sibling === element) {
        //                return getRelativeXpath(element.parentNode) + '/' + elementTagName + '[' + (sibling_count + 1) + ']';
        //            }
        //            if (element_sibling.nodeType === 1 && element_sibling.tagName.toLowerCase() === elementTagName) {
        //                sibling_count++;
        //            }
        //        }
        //    }
        //var element = arguments[0];
        //var xpath = '';
        //xpath = getRelativeXpath(element);
        //return xpath;
        //", webElement));
        //            return _result;
        //        }

        /// <summary>
        ///  Método de extração do xpath relativo através do IJavaScript Selenium com parametro IWebElement
        /// </summary>
        public static string getElementRelativeXpath()
        {
            IWebElement webElement = Selenium.driver.FindElement(By.XPath(getCorrection));
            IJavaScriptExecutor jsExec = Selenium.driver as IJavaScriptExecutor;
            string _result = QuoteLocator((string)jsExec.ExecuteScript(
            @"
    function getPathTo(element) {
        let tagName = element.tagName.toLowerCase();
        let attributes = [];
        
        if (element.id) {
            attributes.push(`@id=""${element.id}""`);
        } else if (element.name) {
            attributes.push(`@name=""${element.name}""`);
        } else if (element.className) {
            let classList = element.className.trim().split(/\s+/).slice(0, 2).join(' ');
            attributes.push(`contains(@class, ""${classList}"")`);
        }
        
        if (element.textContent.trim().length > 0 && element.textContent.trim().length < 30) {
            attributes.push(`text()=""${element.textContent.trim()}""`);
        }
        
        let attributeString = attributes.length ? `[${attributes.join(' and ')}]` : '';
        let xpath = `//${tagName}${attributeString}`;
        
        return xpath;
    }
    
    var element = arguments[0];
    return getPathTo(element);
    ", webElement));

            return _result;
        }


        private static string QuoteLocator(string locator)
        {
            locator = locator.Replace("\\", "").Replace("\"", "'").Replace("''", "'").Trim('\'');
            locator = locator.Replace("{", "{{");
            locator = locator.Replace("}", "}}");
            locator = locator.Replace("/\" /> ", "\" /> ").Trim();
            return locator;
        }

        public static void Encrypt()
        {
            string baseDir = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", ".."));

            string[] directories = System.IO.Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            var teste = directories.Where(x => x.EndsWith("properties") || x.EndsWith("propertie") || x.EndsWith("propriedades"));
            string file = null;
            foreach (string directory in teste) { file = directory; break; }
            IDictionary<string, string> properties;
            using (TextReader reader = new StreamReader(file))
            {
                properties = PropertiesLoader.Load(reader);
            }
            string key = "E546C8DF278CD5931069B522E695D4F2";
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                using (StreamWriter tw = new StreamWriter(fs))

                    foreach (KeyValuePair<string, string> kvp in properties)
                    {
                        string encryptedString = Encrypt(kvp.Value, key);
                        string format = string.Format("{0} = {1}", kvp.Key, kvp.Value.Replace(kvp.Value, encryptedString).Trim());
                        tw.WriteLine(format);
                    }
            }
        }

        public static void Decrypt()
        {
            string baseDir = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", ".."));

            string[] directories = System.IO.Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            var teste = directories.Where(x => x.EndsWith("properties") || x.EndsWith("propertie") || x.EndsWith("propriedades"));
            string file = null;
            foreach (string directory in teste) { file = directory; break; }
            IDictionary<string, string> properties;
            using (TextReader reader = new StreamReader(file))
            {
                properties = PropertiesLoader.Load(reader);
            }
            string key = "E546C8DF278CD5931069B522E695D4F2";
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                using (StreamWriter tw = new StreamWriter(fs))

                    foreach (KeyValuePair<string, string> kvp in properties)
                    {
                        string decryptedString = Decrypt(kvp.Value, key);
                        string format = string.Format("{0} = {1}", kvp.Key, kvp.Value.Replace(kvp.Value, decryptedString));
                        tw.WriteLine(format);
                    }
            }
            List<string> list = System.IO.File.ReadAllLines(file).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
            list.RemoveAll(x => x.EndsWith("=") || x.Contains("+"));
            System.IO.File.WriteAllLines(file, list);
        }

        private static string Encrypt(string data, string key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes("abcede0123456789");
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = initializationVector;
                var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream,
symmetricEncryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream as Stream))
                        {
                            streamWriter.Write(data);
                        }
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        private static string Decrypt(string cipherText, string key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes("abcede0123456789");
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = initializationVector;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream,
decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream as Stream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string stringSecret(string value)
        {
            string baseDir = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", ".."));

            string[] directories = System.IO.Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            var teste = directories.Where(x => x.EndsWith("properties") || x.EndsWith("propertie") || x.EndsWith("propriedades"));
            string file = null;
            foreach (string directory in teste) { file = directory; break; }
            IDictionary<string, string> properties;
            using (TextReader reader = new StreamReader(file))
            {
                properties = PropertiesLoader.Load(reader);
            }
            string key = "E546C8DF278CD5931069B522E695D4F2";
            foreach (KeyValuePair<string, string> kvp in properties)
            {
                if (kvp.Key.Equals(value))
                {
                    string decryptedString = Decrypt(kvp.Value, key);
                    value = decryptedString;
                    break;
                }
            }
            return value;
        }

        private static IWebElement WaitElementForMethod(IWebDriver driver, string locator, int timeout = TIMEOUT_CONST)
        {
            By byLocator = GetLocator(locator);
            WebDriverWait wait = new WebDriverWait(Selenium.driver, TimeSpan.FromSeconds(timeout));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(byLocator));
            return element;
        }

        public static bool ValidateElementVisible(IWebElement element)
        {
            try
            {
                if (element.Displayed)
                {
                    HighlightElementPassMethod(element);
                    return true;
                }
                else
                {
                    HighlightElementFailMethod(element);
                    bool _result = HighlightElementFailMethod(element);
                    Assert.IsTrue(_result, "O elemento não está visível ", null);
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private static bool ValidateElementInnerTextEquals(IWebElement element, string expectedResult)
        {
            if (element.Text.Equals(expectedResult))
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else if (element.Text != expectedResult)
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
            else
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
        }

        private static bool SelfCorrectionElementInnerTextEquals(IWebElement element, string expectedResult)
        {
            if (element.Text.Equals(expectedResult))
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else if (element.Text != expectedResult)
            {
                HighlightElementFailMethod(element);
                SelfCorrectionParameters(expectedResult, element.Text);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
            else
            {
                HighlightElementFailMethod(element);
                SelfCorrectionParameters(expectedResult, element.Text);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
        }

        private static bool SelfCorrectionElementValueTextEquals(IWebElement element, string expectedResult)
        {
            if (element.GetAttribute("value").Equals(expectedResult))
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else
            {
                HighlightElementFailMethod(element);
                string elementValue = element.GetAttribute("value");
                SelfCorrectionParameters(expectedResult, elementValue);
                Assert.IsTrue(false, "O nome do elemento --> " + element.GetAttribute("value") + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
        }

        private static bool ValidateElementInnerTextContains(IWebElement element, string expectedResult)
        {
            if (element.Text.Contains(expectedResult) && expectedResult != "")
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else if (element.Text != expectedResult)
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
            else
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.Text + " é diferente que este resultado --> " + expectedResult + "", null);
                return false;
            }
        }

        private static bool ValidateElementValueTextEquals(IWebElement element, string expectedResult)
        {
            if (element.GetAttribute("value").Equals(expectedResult))
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.GetAttribute("value") + " é diferente que o resultado --> " + expectedResult + "", null);

                return false;
            }
        }

        private static bool ValidateElementValueTextContains(IWebElement element, string expectedResult)
        {
            if (element.GetAttribute("value").Contains(expectedResult))
            {
                HighlightElementPassMethod(element);
                return true;
            }
            else
            {
                HighlightElementFailMethod(element);
                Assert.IsTrue(false, "O nome do elemento --> " + element.GetAttribute("value") + " é diferente que o resultado --> " + expectedResult + "", null);
                return false;
            }
        }

        private static void SelfCorrectionParameters(string beforeError, string updateNewValue)
        {
            string baseDir = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..", "..", ".."));

            string[] directories = System.IO.Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            foreach (string file in directories)
            {
                switch (file)
                {
                    case string x when file.EndsWith("feature"):
                        string text = System.IO.File.ReadAllText(file);
                        text = text.Replace(beforeError, updateNewValue);
                        System.IO.File.WriteAllText(file, text);
                        break;
                        //case string x when file.EndsWith("step.cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith("steps.cs", StringComparison.OrdinalIgnoreCase):
                        //    text = System.IO.File.ReadAllText(file);
                        //    text = text.Replace(beforeError, updateNewValue);
                        //    System.IO.File.WriteAllText(file, text);
                        //    break;
                        //case string x when file.EndsWith("page.cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith("pages.cs", StringComparison.OrdinalIgnoreCase):
                        //    text = System.IO.File.ReadAllText(file);
                        //    text = text.Replace(beforeError, updateNewValue);
                        //    System.IO.File.WriteAllText(file, text);
                        //    break;
                        //case string x when file.EndsWith("action.cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith("actions.cs", StringComparison.OrdinalIgnoreCase):
                        //    text = System.IO.File.ReadAllText(file);
                        //    text = text.Replace(beforeError, updateNewValue);
                        //    System.IO.File.WriteAllText(file, text);
                        //    break;
                }
            }
        }

        private static void writeFileTxt()
        {
            string countFilePath = Path.Combine(filePath, "CountPng.txt");
            using (StreamWriter save = new StreamWriter(countFilePath))
            {
                if (string.IsNullOrEmpty(getCount))
                {
                    count = 0;
                    save.WriteLine(count);
                    save.Close();
                }
                else if (getCount.Equals(getCount))
                {
                    int number = Convert.ToInt32(getCount);
                    number++;
                    save.WriteLine(number);
                    save.Close();
                }
            }
        }

        private static void readFileTxt()
        {
            string countFilePath = Path.Combine(filePath, "CountPng.txt");
            using (StreamReader load = new StreamReader(countFilePath))
            {
                getCount = load.ReadLine();
            }
        }
    }
}