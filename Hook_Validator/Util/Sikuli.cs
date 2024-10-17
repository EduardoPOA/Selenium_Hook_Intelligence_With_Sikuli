/*
 * @author Eduardo Oliveira
 */
using AngleSharp.Dom;
using Hook_Validator.Rest;
using System.Diagnostics;
using System.Xml.Linq;

namespace Hook_Validator.Util
{
    public class Sikuli
    {
        private static SikuliDriver launcher;
        private static SikuliAction action = new SikuliAction();
        private Process process;


        /// <summary>
        /// Abre um aplicativo .exe especificado pelo caminho do arquivo.
        /// </summary>
        public void OpenExeApplication(string exePath)
        {
            process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }


        /// <summary>
        /// Fecha a aplicação do arquivo executável.
        /// </summary>
        public void CloseExeApplication()
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Obtém o caminho da imagem especificada na Solution Explorer.
        /// </summary>
        /// <param name="nomeImagem">Nome da imagem.</param>
        /// <param name="nomePasta">Nome da pasta onde a imagem está localizada.</param>
        /// <returns>Caminho completo da imagem.</returns>
        public static SikuliElement GetImagePathFromSolutionExplorer(string nomeImagem, string nomePasta = "")
        {
            return new SikuliElement(action.GetImagePathFromSolutionExplorer(nomeImagem, nomePasta));
        }

        /// <summary>
        /// Obtém o caminho da imagem especificada no caminho completo depois do bin.
        /// </summary>
        /// <param name="nomeImagem">Nome da imagem.</param>
        /// <param name="nomePasta">Nome da pasta onde a imagem está localizada.</param>
        /// <returns>Caminho completo da imagem.</returns>
        public static SikuliElement GetImagePathFromBin(string nomeImagem, string nomePasta = "")
        {
            return new SikuliElement(action.GetImagePathFromBin(nomeImagem, nomePasta));
        }

        /// <summary>
        /// Inicia o driver Sikuli e define o valor inicial para a variável launcher.
        /// </summary>
        public static void Start()
        {
            launcher = new SikuliDriver(true);
            launcher.Start();
        }


        /// <summary>
        /// Para a execução do programa.
        /// </summary>
        public static void Stop()
        {
            launcher.Stop();
        }

        /// <summary>
        /// Encontra um elemento Sikuli na tela e executa uma ação opcional de destaque.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser encontrado.</param>
        public static SikuliElement Element(string getNamePathImage)
        {
            return new SikuliElement(getNamePathImage);
        }

        /// <summary>
        /// Encontra um elemento Sikuli na tela e executa uma ação opcional de destaque.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser encontrado.</param>
        public static void Find(string element, bool highlight = false)
        {
            action.Find(Element(element), highlight);
        }


        /// <summary>
        /// Clica no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        /// <param name="kmod">O modificador de tecla a ser usado durante o clique.</param>
        /// <param name="highlight">Se deve ou não destacar o elemento antes de clicar.</param>
        public static void Click(string element, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.Click(Element(element), kmod, highlight);
        }


        /// <summary>
        /// Clica no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        public static void Click(string element, bool highlight)
        {
            action.Click(Element(element), highlight);
        }


        /// <summary>
        /// Executa um duplo clique no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli no qual o duplo clique será executado.</param>
        /// <param name="kmod">O modificador de tecla opcional a ser usado durante o clique.</param>
        /// <param name="highlight">Se verdadeiro, o elemento será destacado durante o clique.</param>
        public static void DoubleClick(string element, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.DoubleClick(Element(element), kmod, highlight);
        }


        /// <summary>
        /// Executa um duplo clique no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli no qual o duplo clique será executado.</param>
        public static void DoubleClick(string element, bool highlight)
        {
            action.DoubleClick(Element(element), highlight);
        }


        /// <summary>
        /// Clique com o botão direito do mouse em um elemento Sikuli.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        /// <param name="kmod">O modificador de tecla a ser usado durante o clique.</param>
        /// <param name="highlight">Se deve ou não destacar o elemento antes de clicar.</param>
        public static void RightClick(string element, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.RightClick(Element(element), kmod, highlight);
        }


        /// <summary>
        /// Clique com o botão direito do mouse em um elemento Sikuli.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        public static void RightClick(string element, bool highlight)
        {
            action.RightClick(Element(element), highlight);
        }


        /// <summary>
        /// Espera até que o elemento Sikuli seja encontrado na tela.
        /// </summary>
        /// <param name="pattern">Padrão do elemento Sikuli a ser encontrado.</param>
        public static void Wait(string element, double timeout = 15)
        {
            action.Wait(Element(element), timeout);
        }


        /// <summary>
        /// Espera até que o elemento Sikuli desapareça da tela.
        /// </summary>
        /// <param name="padrão">O elemento Sikuli a ser verificado.</param>
        public static void WaitVanish(string element, double timeout = 20)
        {
            action.WaitVanish(Element(element), timeout);
        }


        /// <summary>
        /// Verifica se o elemento Sikuli existe na tela.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser verificado.</param>
        /// <param name="timeout">O tempo máximo de espera em segundos. O padrão é 20 segundos.</param>
        public static void Exists(string element, double timeout = 20)
        {
            action.Exists(Element(element), timeout);
        }


        /// <summary>
        /// Escreve o texto fornecido no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli onde o texto será escrito.</param>
        public static void Type(string element, string text, bool highlight = false)
        {
            action.Type(Element(element), text, highlight);
        }


        /// <summary>
        /// Realiza uma ação de arrastar e soltar entre dois elementos Sikuli.
        /// </summary>
        /// <param name="clickPattern">Padrão do elemento a ser clicado.</param>
        /// <param name="dropPattern">Padrão do elemento de destino.</param>
        public static void DragDrop(string element, SikuliElement dropPattern)
        {
            action.DragDrop(Element(element), dropPattern);
        }
    }
}