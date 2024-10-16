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
        private String JarReleaseAddress;
        private String JdkRepoAddress;

        public SikuliDriver(bool Windowless = false)
        {
            APIJar = "sikulirestapi-1.0.jar";
            JarReleaseAddress = "http://sourceforge.net/projects/sikulirestapi/files/sikulirestapi-1.0.jar/download";
            JdkRepoAddress = "https://raw.githubusercontent.com/EduardoPOA/JdkForSikuli/main/Jdk/"; // URL dos executáveis do JDK
            WorkingDir = Directory.GetCurrentDirectory();
            APIPath = Path.Combine(WorkingDir, APIJar);

            // Define o executável java com base na opção Windowless
            string javaExecutable = Windowless ? "javaw.exe" : "java.exe";
            string jdkPath = Path.Combine(WorkingDir, "Jdk", javaExecutable); // Caminho para o executável na pasta Jdk

            // Certifique-se de que o caminho está correto
            if (!File.Exists(jdkPath))
            {
                DownloadJdkFiles(); // Chama o método para baixar os arquivos JDK
                if (!File.Exists(jdkPath))
                {
                    throw new FileNotFoundException($"O executável {javaExecutable} não foi encontrado em: {jdkPath}");
                }
            }

            // Configura o ProcessStartInfo para usar o java na pasta Jdk
            APIProcessStartInfo = new ProcessStartInfo(jdkPath, "-jar \"" + APIPath + "\"");
            APIProcess = new Process();
            APIProcess.StartInfo = APIProcessStartInfo;
        }

        private void DownloadJdkFiles()
        {
            // Cria a pasta Jdk se não existir
            string jdkDirectory = Path.Combine(WorkingDir, "Jdk");
            Directory.CreateDirectory(jdkDirectory);

            // Baixa os arquivos JDK
            string[] executables = { "java.exe", "javaw.exe" }; // Adicione outros executáveis se necessário

            foreach (var exe in executables)
            {
                string fileUrl = JdkRepoAddress + exe;
                string filePath = Path.Combine(jdkDirectory, exe);
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(fileUrl, filePath);
                    Util.Log.WriteLine($"Arquivo baixado: {filePath}");
                }
            }
        }

        public void Start()
        {
            VerifyJarExists();
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

        public void VerifyJarExists()
        {
            if (File.Exists(APIPath))
            {
                Util.Log.WriteLine("Jar já baixado, iniciando servidor jetty ...");
            }
            else
            {
                Util.Log.WriteLine("Jar não baixado, baixando jar do servidor jetty do SourceForge ...");
                WebClient client = new WebClient();
                client.DownloadFile(JarReleaseAddress, APIPath);
                Util.Log.WriteLine("Arquivo baixado!");
            }
        }
    }
}
