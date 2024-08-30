/*
 * @author Eduardo Oliveira
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Hook_Validator.Util
{
    public class APILauncher
    {
        private Process APIProcess;
        private ProcessStartInfo APIProcessStartInfo;
        public String API_Output;
        private String APIJar;
        private String WorkingDir;
        private String APIPath;
		private String JarReleaseAddress;

        public APILauncher(bool Windowless = false)
        {
            APIJar = "sikulirestapi-1.0.jar";
			JarReleaseAddress = "http://sourceforge.net/projects/sikulirestapi/files/sikulirestapi-1.0.jar/download";
            WorkingDir = Directory.GetCurrentDirectory();
            APIPath = Path.Combine(WorkingDir, APIJar);
            if (Windowless == false)
            {
                APIProcessStartInfo = new ProcessStartInfo("java", "-jar \"" + APIPath + "\"");
            }
            else
            {
                APIProcessStartInfo = new ProcessStartInfo("javaw", "-jar \"" + APIPath + "\"");
            }
            APIProcess = new Process();
            APIProcess.StartInfo = APIProcessStartInfo;
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
			if(File.Exists(APIPath))
			{
				Util.Log.WriteLine("Jar já baixado, iniciando servidor jetty ...");
			}
			else
			{
				Util.Log.WriteLine("Jar não baixado, baixando jar do servidor jetty do SourceForge ...");
				WebClient client = new WebClient();
				client.DownloadFile(JarReleaseAddress,APIPath);
				Util.Log.WriteLine("Arquivo baixado!");
			}
		}
    }
}
