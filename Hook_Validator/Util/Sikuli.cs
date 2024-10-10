using Hook_Validator.Rest;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Diagnostics;

namespace Hook_Validator.Util
{
    public class Sikuli
    {
        private static SikuliDriver launcher;
        private static SikuliAction action = new SikuliAction();

        public static void OpenExeApplication(string exePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        public static string GetImageSolutionExplorer(string nomeImagem, string nomePasta = "")
        {
            return action.GetImageSolutionExplorer(nomeImagem, nomePasta);
        }

        public static void Start()
        {
            launcher = new SikuliDriver(true);
            launcher.Start();
        }

        public static void Stop()
        {
            launcher.Stop();
        }

        public static SikuliElement Element(string getNamePathImage)
        {
            return new SikuliElement(getNamePathImage);
        }

        public static void Find(SikuliElement pattern, bool highlight = false)
        {
            action.Find(pattern, highlight);
        }

        public static void Click(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.Click(pattern, kmod, highlight);
        }

        public static void Click(SikuliElement pattern, bool highlight)
        {
            action.Click(pattern, highlight);
        }

        public static void DoubleClick(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.DoubleClick(pattern, kmod, highlight);
        }

        public static void DoubleClick(SikuliElement pattern, bool highlight)
        {
            action.DoubleClick(pattern, highlight);
        }

        public static void RightClick(SikuliElement pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            action.RightClick(pattern, kmod, highlight);
        }

        public static void RightClick(SikuliElement pattern, bool highlight)
        {
            action.RightClick(pattern, highlight);
        }

        public static void Wait(SikuliElement pattern, double timeout = 15)
        {
            action.Wait(pattern, timeout);
        }

        public static void WaitVanish(SikuliElement pattern, double timeout = 20)
        {
            action.WaitVanish(pattern, timeout);
        }

        public static void Exists(SikuliElement pattern, double timeout = 20)
        {
            action.Exists(pattern, timeout);
        }

        public static void Type(SikuliElement pattern, string text, bool highlight)
        {
            action.Type(pattern, text, highlight);
        }

        public static void DragDrop(SikuliElement clickPattern, SikuliElement dropPattern)
        {
            action.DragDrop(clickPattern, dropPattern);
        }
    }
}
