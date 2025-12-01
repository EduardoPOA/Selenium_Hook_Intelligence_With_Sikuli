/*
 * @author Eduardo Oliveira
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Hook_Validator.Util
{
    public class SikuliDriver
    {
        private Process APIProcess;
        private ProcessStartInfo APIProcessStartInfo;
        public String API_Output;
        private String APIJar;
        private String WorkingDir;
        private String APIPath;
        private String JdkRepoAddress;
        private String SikuliRepoAddress;

        public SikuliDriver(bool Windowless = false)
        {
            APIJar = "sikulirestapi-1.0.jar"; // Nome do JAR do Sikuli
            WorkingDir = Directory.GetCurrentDirectory();

            // Caminho do JAR na pasta Jdk
            APIPath = Path.Combine(WorkingDir, "Jdk", APIJar);

            // URL para baixar o JDK e o JAR do Sikuli caso não estejam presentes
            JdkRepoAddress = "https://raw.githubusercontent.com/EduardoPOA/JdkForSikuli/main/Jdk/"; // Endereço para os executáveis do JDK
            SikuliRepoAddress = "https://raw.githubusercontent.com/EduardoPOA/JdkForSikuli/main/Jdk/sikulirestapi-1.0.jar"; // URL para o JAR do Sikuli

            // Define o executável java com base na opção Windowless
            string javaExecutable = Windowless ? "javaw.exe" : "java.exe";
            string jdkPath = Path.Combine(WorkingDir, "Jdk", javaExecutable); // Caminho para o executável na pasta Jdk

            // Certifique-se de que o executável Java está presente
            if (!File.Exists(jdkPath))
            {
                DownloadJdkFiles(); // Baixa os arquivos JDK se não existirem
                if (!File.Exists(jdkPath))
                {
                    throw new FileNotFoundException($"O executável {javaExecutable} não foi encontrado em: {jdkPath}");
                }
            }

            // Certifique-se de que o JAR do Sikuli está presente
            if (!File.Exists(APIPath))
            {
                DownloadSikuliJar(); // Baixa o JAR se ele não estiver presente
                if (!File.Exists(APIPath))
                {
                    throw new FileNotFoundException($"O arquivo {APIJar} não foi encontrado na pasta Jdk: {APIPath}");
                }
            }

            // Configura o ProcessStartInfo para usar o java na pasta Jdk e o JAR já presente
            APIProcessStartInfo = new ProcessStartInfo(jdkPath, "-jar \"" + APIPath + "\"");
            APIProcess = new Process();
            APIProcess.StartInfo = APIProcessStartInfo;
        }

        private void DownloadJdkFiles()
        {
            // Cria a pasta Jdk se não existir
            string jdkDirectory = Path.Combine(WorkingDir, "Jdk");
            Directory.CreateDirectory(jdkDirectory);

            // Baixa os arquivos JDK (java.exe e javaw.exe)
            string[] executables = { "java.exe", "javaw.exe" };

            foreach (var exe in executables)
            {
                string fileUrl = JdkRepoAddress + exe;
                string filePath = Path.Combine(jdkDirectory, exe);
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(fileUrl, filePath);
                        Util.Log.WriteLine($"Arquivo baixado: {filePath}");
                    }
                    catch (WebException ex)
                    {
                        Util.Log.WriteLine($"Erro ao baixar {exe}: {ex.Message}");
                    }
                }
            }
        }

        private void DownloadSikuliJar()
        {
            // Baixa o JAR do Sikuli se ele não estiver presente
            string filePath = APIPath; // Caminho onde o JAR será salvo
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(SikuliRepoAddress, filePath);
                    Util.Log.WriteLine($"Arquivo JAR do Sikuli baixado: {filePath}");
                }
                catch (WebException ex)
                {
                    Util.Log.WriteLine($"Erro ao baixar o JAR do Sikuli: {ex.Message}");
                }
            }
        }

        public void Start()
        {
            Util.Log.WriteLine("Iniciando servidor jetty ...");
            APIProcess.Start();
        }

        public void Stop()
        {
            Util.Log.WriteLine("Parando servidor jetty ...");
            APIProcess.Kill();
            Util.Log.WriteLine("Servidor Jetty parado!");
            Util.Log.WriteLine("O registro do cliente para esta execução de teste pode ser localizado em: " + Util.Log.LogPath);
            Util.Log.WriteLine("Saindo ...");
        }
    }
}
