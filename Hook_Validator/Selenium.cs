/*
 * @author Eduardo Oliveira
 */
using OpenQA.Selenium;
using System;

namespace Hook_Validator
{
    public static class Selenium
    {
        /// <summary>
        ///  Instancia do driver para ser utilizado em qualquer classe
        /// </summary>
        public static IWebDriver driver;
        public static IWebDriver Driver
        {
            get
            {
                return driver;
            }
            set
            {
                driver = value;
            }
        }
        /// <summary>
        ///  Chamados do Selenium para fechar e sair do processo
        /// </summary>
        public static void TearDown()
        {
            try
            {
                driver.Close();
                driver.Quit();
            }
            catch (Exception) { }
        }
    }
}
