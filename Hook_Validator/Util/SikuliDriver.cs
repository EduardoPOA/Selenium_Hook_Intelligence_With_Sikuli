using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Hook_Validator.Util
{
    public class SikuliDriver
    {
        private Process APIProcess;
        private ProcessStartInfo APIProcessStartInfo;
        private String APIJar;
        private String WorkingDir;
        private String APIPath;
        private String ZipUrl;

        public SikuliDriver(bool Windowless = false)
        {
            APIJar = "sikulirestapi-1.0.jar";
            WorkingDir = Directory.GetCurrentDirectory();
            APIPath = Path.Combine(WorkingDir, "Jdk", APIJar);

            // URL RAW correta
            ZipUrl = "https://raw.githubusercontent.com/EduardoPOA/JdkForSikuli/main/Jdk/Jdk.zip";

            string javaExecutable = Windowless ? "javaw.exe" : "java.exe";
            string jdkPath = Path.Combine(WorkingDir, "Jdk", javaExecutable);

            // se não existe, baixa o zip e extrai
            if (!File.Exists(jdkPath))
            {
                DownloadAndExtractJdk();
            }

            if (!File.Exists(jdkPath))
                throw new FileNotFoundException($"Java não encontrado em {jdkPath}");

            if (!File.Exists(APIPath))
                throw new FileNotFoundException($"Sikuli JAR não encontrado em {APIPath}");

            APIProcessStartInfo =
                new ProcessStartInfo(jdkPath, "-jar \"" + APIPath + "\"");

            APIProcess = new Process();
            APIProcess.StartInfo = APIProcessStartInfo;
        }

        private void DownloadAndExtractJdk()
        {
            string zipPath = Path.Combine(WorkingDir, "Jdk.zip");
            string extractPath = Path.Combine(WorkingDir, "Jdk");

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            using (WebClient client = new WebClient())
            {
                Util.Log.WriteLine("Baixando JDK ZIP ...");
                client.DownloadFile(ZipUrl, zipPath);
            }

            Util.Log.WriteLine("Extraindo JDK ...");
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            File.Delete(zipPath);

            Util.Log.WriteLine("JDK instalada em: " + extractPath);
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
        }
    }
}
