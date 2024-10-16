/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Rest;
using System.Diagnostics;

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
        /// Obtém o caminho da imagem especificada.
        /// </summary>
        /// <param name="nomeImagem">Nome da imagem.</param>
        /// <param name="nomePasta">Nome da pasta onde a imagem está localizada.</param>
        /// <returns>Caminho completo da imagem.</returns>
        public static string GetImagePath(string nomeImagem, string nomePasta = "")
        {
            return action.GetImagePath(nomeImagem, nomePasta);
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
        /// Retorna um novo objeto SikuliElement com base no caminho e nome da imagem fornecidos.
        /// </summary>
        /// <param name="getNamePathImage">O caminho e nome da imagem a ser usada para criar o objeto SikuliElement.</param>
        /// <returns>O objeto SikuliElement criado.</returns>
        public static SikuliElement Element(string getNamePathImage)
        {
            return new SikuliElement(getNamePathImage);
        }


        /// <summary>
        /// Encontra um elemento Sikuli na tela e executa uma ação opcional de destaque.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser encontrado.</param>
        public static void Find(SikuliElement pattern, bool highlight = false)
        {
            action.Find(pattern, highlight);
        }


        /// <summary>
        /// Clica no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        /// <param name="kmod">O modificador de tecla a ser usado durante o clique.</param>
        /// <param name="highlight">Se deve ou não destacar o elemento antes de clicar.</param>
        public static void Click(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.Click(pattern, kmod, highlight);
        }


        /// <summary>
        /// Clica no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        public static void Click(SikuliElement pattern, bool highlight)
        {
            action.Click(pattern, highlight);
        }


        /// <summary>
        /// Executa um duplo clique no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli no qual o duplo clique será executado.</param>
        /// <param name="kmod">O modificador de tecla opcional a ser usado durante o clique.</param>
        /// <param name="highlight">Se verdadeiro, o elemento será destacado durante o clique.</param>
        public static void DoubleClick(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.DoubleClick(pattern, kmod, highlight);
        }


        /// <summary>
        /// Executa um duplo clique no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli no qual o duplo clique será executado.</param>
        public static void DoubleClick(SikuliElement pattern, bool highlight)
        {
            action.DoubleClick(pattern, highlight);
        }


        /// <summary>
        /// Clique com o botão direito do mouse em um elemento Sikuli.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        /// <param name="kmod">O modificador de tecla a ser usado durante o clique.</param>
        /// <param name="highlight">Se deve ou não destacar o elemento antes de clicar.</param>
        public static void RightClick(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.RightClick(pattern, kmod, highlight);
        }


        /// <summary>
        /// Clique com o botão direito do mouse em um elemento Sikuli.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser clicado.</param>
        public static void RightClick(SikuliElement pattern, bool highlight)
        {
            action.RightClick(pattern, highlight);
        }


        /// <summary>
        /// Espera até que o elemento Sikuli seja encontrado na tela.
        /// </summary>
        /// <param name="pattern">Padrão do elemento Sikuli a ser encontrado.</param>
        public static void Wait(SikuliElement pattern, double timeout = 15)
        {
            action.Wait(pattern, timeout);
        }


        /// <summary>
        /// Espera até que o elemento Sikuli desapareça da tela.
        /// </summary>
        /// <param name="padrão">O elemento Sikuli a ser verificado.</param>
        public static void WaitVanish(SikuliElement pattern, double timeout = 20)
        {
            action.WaitVanish(pattern, timeout);
        }


        /// <summary>
        /// Verifica se o elemento Sikuli existe na tela.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli a ser verificado.</param>
        /// <param name="timeout">O tempo máximo de espera em segundos. O padrão é 20 segundos.</param>
        public static void Exists(SikuliElement pattern, double timeout = 20)
        {
            action.Exists(pattern, timeout);
        }


        /// <summary>
        /// Escreve o texto fornecido no elemento Sikuli especificado.
        /// </summary>
        /// <param name="pattern">O elemento Sikuli onde o texto será escrito.</param>
        public static void Type(SikuliElement pattern, string text, bool highlight)
        {
            action.Type(pattern, text, highlight);
        }


        /// <summary>
        /// Realiza uma ação de arrastar e soltar entre dois elementos Sikuli.
        /// </summary>
        /// <param name="clickPattern">Padrão do elemento a ser clicado.</param>
        /// <param name="dropPattern">Padrão do elemento de destino.</param>
        public static void DragDrop(SikuliElement clickPattern, SikuliElement dropPattern)
        {
            action.DragDrop(clickPattern, dropPattern);
        }
    }
}