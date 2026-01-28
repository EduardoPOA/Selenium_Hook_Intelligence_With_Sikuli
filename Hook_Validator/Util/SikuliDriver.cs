/*
* @author Eduardo Oliveira
* Versão usando JAR do seu repositório
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace Hook_Validator.Util
{
    public class SikuliDriver
    {
        private Process _process;
        private readonly string _baseDir;
        private readonly string _jdkFolder;
        private string _javaExe;
        private string _sikuliJar;
        private readonly int _port;
        private string _javaHome;

        // URLs para download do SEU repositório
        private const string SIKULI_REST_JAR_URL = "https://github.com/EduardoPOA/JdkForSikuli/raw/refs/heads/main/Jdk/sikulirestapi-1.0.jar";
        private const string WINDOWS_JDK_URL = "https://github.com/EduardoPOA/JdkForSikuli/raw/refs/heads/main/Jdk/jdk-8u202-windows-x64.exe";
        private const string LINUX_JDK_URL = "https://github.com/EduardoPOA/JdkForSikuli/raw/refs/heads/main/Jdk/jdk-8u202-linux-x64.tar.gz";

        // URLs de fallback
        private const string SIKULI_REST_JAR_MEDIA_URL = "https://media.githubusercontent.com/media/EduardoPOA/JdkForSikuli/main/Jdk/sikulirestapi-1.0.jar";
        private const string WINDOWS_JDK_MEDIA_URL = "https://media.githubusercontent.com/media/EduardoPOA/JdkForSikuli/main/Jdk/jdk-8u202-windows-x64.exe";
        private const string LINUX_JDK_MEDIA_URL = "https://media.githubusercontent.com/media/EduardoPOA/JdkForSikuli/main/Jdk/jdk-8u202-linux-x64.tar.gz";

        // URLs das DLLs nativas
        private const string WINUTIL_DLL_URL = "https://github.com/EduardoPOA/JdkForSikuli/raw/refs/heads/main/Jdk/WinUtil.dll";
        private const string VISIONPROXY_DLL_URL = "https://github.com/EduardoPOA/JdkForSikuli/raw/refs/heads/main/Jdk/VisionProxy.dll";

        // URLs de fallback para as DLLs
        private const string WINUTIL_DLL_MEDIA_URL = "https://media.githubusercontent.com/media/EduardoPOA/JdkForSikuli/main/Jdk/WinUtil.dll";
        private const string VISIONPROXY_DLL_MEDIA_URL = "https://media.githubusercontent.com/media/EduardoPOA/JdkForSikuli/main/Jdk/VisionProxy.dll";

        // Propriedades do sistema
        private readonly bool _isWindows;
        private readonly bool _isLinux;
        private readonly string _platform;

        public SikuliDriver(bool windowless = true, int port = 8080)
        {
            KillProcessOnPort(port);
            _port = port;

            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            _platform = _isWindows ? "Windows" : _isLinux ? "Linux" : "Unknown";

            if (!_isWindows && !_isLinux)
            {
                throw new PlatformNotSupportedException($"Sistema operacional não suportado: {_platform}");
            }

            _baseDir = AppContext.BaseDirectory;
            _jdkFolder = Path.Combine(_baseDir, "Jdk");
            Directory.CreateDirectory(_jdkFolder);

            SetupEnvironment();

            // Adiciona a pasta libs ao PATH para o processo Java
            string libsFolder = Path.Combine(_jdkFolder, "libs");
            string javaPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            string newPath = $"{libsFolder};{Path.Combine(_javaHome, "bin")};{javaPath}";

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _javaExe,
                    Arguments = $"-Djava.library.path=\"{libsFolder}\" -jar \"{_sikuliJar}\" --port={_port}",
                    WorkingDirectory = _jdkFolder,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    EnvironmentVariables =
        {
            ["JAVA_HOME"] = _javaHome,
            ["PATH"] = newPath,
            ["SIKULIX_LIB"] = libsFolder
        }
                }
            };
        }
        private string GetJdkInstallerPath()
        {
            if (_isWindows)
            {
                return Path.Combine(_jdkFolder, "jdk-8u202-windows-x64.exe");
            }
            else if (_isLinux)
            {
                return Path.Combine(_jdkFolder, "jdk-8u202-linux-x64.tar.gz");
            }
            else
            {
                throw new PlatformNotSupportedException($"Sistema operacional não suportado: {_platform}");
            }
        }

        private void DownloadFileWithRetry(string url, string destination, string fileName, string fallbackUrl = null, int maxRetries = 3)
        {
            string currentUrl = url;

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    Console.WriteLine($"\n  Tentativa {retry}/{maxRetries} - Baixando {fileName}...");
                    Console.WriteLine($"  URL: {currentUrl}");

                    if (File.Exists(destination))
                    {
                        Console.WriteLine($"  Removendo arquivo anterior...");
                        File.Delete(destination);
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    ServicePointManager.DefaultConnectionLimit = 9999;
                    ServicePointManager.Expect100Continue = false;

                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                        client.Headers.Add("Accept", "*/*");

                        DateTime lastProgressTime = DateTime.Now;
                        long lastBytesReceived = 0;
                        bool progressShown = false;

                        client.DownloadProgressChanged += (sender, e) =>
                        {
                            progressShown = true;
                            DateTime now = DateTime.Now;

                            if (e.ProgressPercentage % 5 == 0 || (now - lastProgressTime).TotalSeconds >= 2)
                            {
                                double mbReceived = e.BytesReceived / 1024.0 / 1024.0;
                                double mbTotal = e.TotalBytesToReceive / 1024.0 / 1024.0;

                                if (mbTotal > 0)
                                {
                                    double speed = 0;
                                    if (lastBytesReceived > 0 && (now - lastProgressTime).TotalSeconds > 0)
                                    {
                                        speed = (e.BytesReceived - lastBytesReceived) / 1024.0 / 1024.0 / (now - lastProgressTime).TotalSeconds;
                                    }

                                    Console.Write($"\r  Progresso: {e.ProgressPercentage}% ({mbReceived:F2} MB / {mbTotal:F2} MB) - {speed:F2} MB/s");
                                }
                                else
                                {
                                    Console.Write($"\r  Baixado: {mbReceived:F2} MB");
                                }

                                lastProgressTime = now;
                                lastBytesReceived = e.BytesReceived;
                            }
                        };

                        client.DownloadFileCompleted += (sender, e) =>
                        {
                            if (progressShown)
                                Console.WriteLine();

                            if (e.Error != null)
                            {
                                Console.WriteLine($"  ❌ Erro no download: {e.Error.Message}");
                            }
                            else if (e.Cancelled)
                            {
                                Console.WriteLine($"  ⚠ Download cancelado");
                            }
                            else
                            {
                                Console.WriteLine($"  ✓ Download concluído!");
                            }
                        };

                        Console.WriteLine($"  Iniciando download...");
                        client.DownloadFile(new Uri(currentUrl), destination);

                        Thread.Sleep(2000);
                    }

                    // Verifica se o download foi bem-sucedido
                    if (File.Exists(destination))
                    {
                        var fileInfo = new FileInfo(destination);
                        double fileSizeMB = fileInfo.Length / 1024.0 / 1024.0;

                        Console.WriteLine($"\n  Verificando arquivo baixado...");
                        Console.WriteLine($"  Tamanho: {fileSizeMB:F2} MB");

                        // Verifica se é um arquivo Git LFS pointer
                        if (fileInfo.Length < 200)
                        {
                            string content = File.ReadAllText(destination);

                            if (content.Contains("version https://git-lfs.github.com/spec"))
                            {
                                Console.WriteLine($"  ⚠ Detectado arquivo Git LFS pointer - não é o arquivo real!");
                                File.Delete(destination);

                                if (!string.IsNullOrEmpty(fallbackUrl) && currentUrl != fallbackUrl)
                                {
                                    Console.WriteLine($"  → Tentando URL alternativa (media.githubusercontent.com)...");
                                    currentUrl = fallbackUrl;
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine($"\n  💡 SOLUÇÃO MANUAL:");
                                    Console.WriteLine($"     1. Acesse: {url.Replace("/raw/", "/blob/")}");
                                    Console.WriteLine($"     2. Clique em 'Download' no botão direito");
                                    Console.WriteLine($"     3. Salve em: {destination}");
                                    throw new Exception("Arquivo LFS não pode ser baixado automaticamente");
                                }
                            }
                        }

                        // Verifica se é HTML (página de erro)
                        if (fileInfo.Length < 10000)
                        {
                            try
                            {
                                string firstContent = "";
                                using (var reader = new StreamReader(destination))
                                {
                                    firstContent = reader.ReadToEnd();
                                }

                                if (firstContent.Contains("<!DOCTYPE") || firstContent.Contains("<html"))
                                {
                                    Console.WriteLine($"  ⚠ Arquivo baixado é HTML (página de erro do GitHub)");
                                    File.Delete(destination);

                                    if (!string.IsNullOrEmpty(fallbackUrl) && currentUrl != fallbackUrl)
                                    {
                                        Console.WriteLine($"  → Tentando URL alternativa...");
                                        currentUrl = fallbackUrl;
                                        continue;
                                    }
                                }
                            }
                            catch { }
                        }

                        // Verifica se o arquivo tem tamanho mínimo esperado
                        long expectedMinSize = GetExpectedMinSize(fileName);

                        if (fileInfo.Length < expectedMinSize)
                        {
                            Console.WriteLine($"  ⚠ Arquivo muito pequeno! Esperado: >{expectedMinSize / 1024 / 1024} MB, Obtido: {fileSizeMB:F2} MB");
                            File.Delete(destination);

                            if (!string.IsNullOrEmpty(fallbackUrl) && currentUrl != fallbackUrl)
                            {
                                Console.WriteLine($"  → Tentando URL alternativa...");
                                currentUrl = fallbackUrl;
                                continue;
                            }

                            if (retry < maxRetries)
                            {
                                Console.WriteLine($"  → Tentando novamente...");
                                continue;
                            }
                        }

                        // Arquivo válido!
                        if (File.Exists(destination) && new FileInfo(destination).Length > expectedMinSize)
                        {
                            Console.WriteLine($"  ✓ Arquivo verificado com sucesso!");
                            Console.WriteLine($"  ✓ Tamanho final: {fileSizeMB:F2} MB");
                            Console.WriteLine($"  ✓ Caminho: {destination}\n");
                            return; // Sucesso!
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ⚠ Arquivo não foi criado após o download!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n  ❌ Erro durante download: {ex.Message}");

                    if (File.Exists(destination))
                    {
                        try { File.Delete(destination); } catch { }
                    }

                    // Tenta URL alternativa na primeira falha
                    if (retry == 1 && !string.IsNullOrEmpty(fallbackUrl) && currentUrl != fallbackUrl)
                    {
                        Console.WriteLine($"  → Tentando URL alternativa (media.githubusercontent.com)...");
                        currentUrl = fallbackUrl;
                        continue;
                    }

                    if (retry == maxRetries)
                    {
                        Console.WriteLine($"\n  ❌ FALHA APÓS {maxRetries} TENTATIVAS\n");
                        Console.WriteLine($"  💡 DOWNLOAD MANUAL:");
                        Console.WriteLine($"     1. Baixe manualmente de: {url}");
                        Console.WriteLine($"     2. Salve o arquivo em: {destination}");
                        Console.WriteLine($"     3. Execute o programa novamente\n");

                        throw new Exception($"Falha ao baixar {fileName}: {ex.Message}");
                    }
                }

                int waitSeconds = 3 * retry;
                Console.WriteLine($"  Aguardando {waitSeconds} segundos antes da próxima tentativa...\n");
                Thread.Sleep(waitSeconds * 1000);
            }

            throw new Exception($"Falha ao baixar {fileName} após {maxRetries} tentativas");
        }

        private long GetExpectedMinSize(string fileName)
        {
            if (fileName.Contains("JDK"))
                return 100 * 1024 * 1024; // 100MB para JDK
            else if (fileName.Contains("sikulixide"))
                return 50 * 1024 * 1024; // ~60MB para sikulixide.jar
            else if (fileName.Contains("sikulirestapi"))
                return 1024 * 1024; // 1MB para sikulirestapi.jar
            else if (fileName.Contains(".dll"))
                return 10 * 1024; // 10KB para DLLs
            else
                return 1024 * 10; // 10KB padrão
        }
        private string InstallJdkWindows(string installerPath)
        {
            if (!File.Exists(installerPath))
                throw new FileNotFoundException($"Instalador não encontrado: {installerPath}");

            var fileInfo = new FileInfo(installerPath);
            if (fileInfo.Length == 0)
            {
                throw new Exception($"Instalador está vazio (0 bytes). Baixe manualmente e coloque em: {installerPath}");
            }

            Console.WriteLine($"  Executando instalador JDK 8 no Windows...");
            Console.WriteLine($"  Tamanho do instalador: {fileInfo.Length / 1024 / 1024:F2} MB");

            try
            {
                string installDir = @"C:\Program Files\Java\jdk1.8.0_202";

                if (Directory.Exists(installDir))
                {
                    Console.WriteLine($"  Removendo JDK existente em: {installDir}");
                    try
                    {
                        Directory.Delete(installDir, true);
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ⚠ Não foi possível remover JDK antigo: {ex.Message}");
                    }
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = $"/s INSTALLDIR=\"{installDir}\" /L \"{Path.Combine(_jdkFolder, "jdk_install.log")}\"",
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };

                Console.WriteLine($"  Instalando em: {installDir}");
                Console.WriteLine($"  Aguarde, isso pode levar alguns minutos...");

                process.Start();
                process.WaitForExit();

                Console.WriteLine($"  Código de saída: {process.ExitCode}");
                Thread.Sleep(15000);

                string javaExe = Path.Combine(installDir, "bin", "java.exe");

                if (!File.Exists(javaExe))
                {
                    string[] possiblePaths = {
                        @"C:\Program Files\Java\jdk1.8.0_202\bin\java.exe",
                        @"C:\Program Files (x86)\Java\jdk1.8.0_202\bin\java.exe"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            javaExe = path;
                            installDir = Path.GetDirectoryName(Path.GetDirectoryName(path));
                            break;
                        }
                    }
                }

                if (File.Exists(javaExe))
                {
                    Console.WriteLine($"  ✓ JDK 8 instalado!");
                    Console.WriteLine($"  • Diretório: {installDir}");
                    return installDir;
                }
                else
                {
                    throw new Exception($"JDK não foi instalado. java.exe não encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Erro na instalação: {ex.Message}");
                throw;
            }
        }

        private string InstallJdkLinux(string tarPath)
        {
            if (!File.Exists(tarPath))
                throw new FileNotFoundException($"Arquivo tar.gz não encontrado: {tarPath}");

            Console.WriteLine($"  Instalando JDK 8 no Linux...");

            try
            {
                string installDir = "/usr/lib/jvm/jdk1.8.0_202";

                if (!Directory.Exists("/usr/lib/jvm"))
                {
                    ExecuteShellCommand("sudo mkdir -p /usr/lib/jvm");
                }

                if (Directory.Exists(installDir))
                {
                    Console.WriteLine($"  Removendo JDK existente...");
                    ExecuteShellCommand($"sudo rm -rf \"{installDir}\"");
                }

                Console.WriteLine("  Extraindo JDK 8...");
                ExecuteShellCommand($"sudo tar -xzf \"{tarPath}\" -C /usr/lib/jvm/");

                string extractedDir = installDir;
                if (!Directory.Exists(extractedDir))
                {
                    var dirs = Directory.GetDirectories("/usr/lib/jvm", "jdk1.8*");
                    if (dirs.Length > 0)
                    {
                        extractedDir = dirs[0];
                        if (extractedDir != installDir)
                        {
                            ExecuteShellCommand($"sudo mv \"{extractedDir}\" \"{installDir}\"");
                            extractedDir = installDir;
                        }
                    }
                }

                if (!Directory.Exists(extractedDir))
                    throw new Exception($"Diretório do JDK não encontrado após extração");

                Console.WriteLine("  Configurando alternativas do sistema...");

                string javaPath = Path.Combine(extractedDir, "bin", "java");
                string javacPath = Path.Combine(extractedDir, "bin", "javac");

                ExecuteShellCommand($"sudo update-alternatives --install /usr/bin/java java {javaPath} 1080");
                ExecuteShellCommand($"sudo update-alternatives --install /usr/bin/javac javac {javacPath} 1080");
                ExecuteShellCommand($"sudo update-alternatives --set java {javaPath}");
                ExecuteShellCommand($"sudo update-alternatives --set javac {javacPath}");

                Console.WriteLine("  Configurando JAVA_HOME...");
                string profilePath = "/etc/profile.d/jdk8.sh";

                ExecuteShellCommand($"echo 'export JAVA_HOME={extractedDir}' | sudo tee {profilePath}");
                ExecuteShellCommand($"echo 'export PATH=$JAVA_HOME/bin:$PATH' | sudo tee -a {profilePath}");
                ExecuteShellCommand($"sudo chmod +x {profilePath}");

                Environment.SetEnvironmentVariable("JAVA_HOME", extractedDir);
                Environment.SetEnvironmentVariable("PATH",
                    $"{Path.Combine(extractedDir, "bin")}:{Environment.GetEnvironmentVariable("PATH")}",
                    EnvironmentVariableTarget.Process);

                Console.WriteLine($"  ✓ JDK 8 instalado!");
                return extractedDir;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na instalação do JDK 8 no Linux: {ex.Message}");
            }
        }

        private void ExecuteShellCommand(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output) && output.Trim().Length > 0)
                    Console.WriteLine($"    {output.Trim()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Erro ao executar comando: {ex.Message}");
                throw;
            }
        }

        private string FindRealJava8Windows()
        {
            Console.WriteLine("  Procurando JDK 8 real (não apenas JRE)...");

            string[] jdkPaths =
            {
                @"C:\Program Files\Java\jdk1.8.0_202\bin\java.exe",
                @"C:\Program Files\Java\jdk1.8.0_341\bin\java.exe",
                @"C:\Program Files\Java\jdk1.8.0\bin\java.exe",
                @"C:\Program Files (x86)\Java\jdk1.8.0_202\bin\java.exe",
                @"C:\Program Files\Java\jdk8\bin\java.exe"
            };

            foreach (var path in jdkPaths)
            {
                if (File.Exists(path) && CheckJavaVersion(path, "1.8"))
                {
                    Console.WriteLine($"  ✓ JDK encontrado: {path}");
                    return path;
                }
            }

            string[] searchRoots = {
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java"
            };

            foreach (var root in searchRoots)
            {
                if (!Directory.Exists(root)) continue;

                try
                {
                    var jdkDirs = Directory.GetDirectories(root, "jdk1.8*");
                    foreach (var dir in jdkDirs)
                    {
                        string java = Path.Combine(dir, "bin", "java.exe");
                        if (File.Exists(java) && CheckJavaVersion(java, "1.8"))
                        {
                            Console.WriteLine($"  ✓ JDK encontrado: {java}");
                            return java;
                        }
                    }

                    jdkDirs = Directory.GetDirectories(root, "jdk*");
                    foreach (var dir in jdkDirs)
                    {
                        string java = Path.Combine(dir, "bin", "java.exe");
                        if (File.Exists(java) && CheckJavaVersion(java, "1.8"))
                        {
                            Console.WriteLine($"  ✓ JDK encontrado: {java}");
                            return java;
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        private bool CheckJavaVersion(string javaExe, string requiredVersion)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = javaExe,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string versionOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return versionOutput.Contains($"version \"{requiredVersion}") ||
                       versionOutput.Contains($"version \"{requiredVersion}.");
            }
            catch
            {
                return false;
            }
        }

        private string GetJavaHomeFromJavaExe(string javaExe)
        {
            if (string.IsNullOrEmpty(javaExe) || !File.Exists(javaExe))
                return null;

            try
            {
                string binDir = Path.GetDirectoryName(javaExe);
                if (!string.IsNullOrEmpty(binDir))
                {
                    string javaHome = Path.GetDirectoryName(binDir);
                    if (Directory.Exists(javaHome))
                    {
                        return javaHome;
                    }
                }
            }
            catch { }

            return null;
        }

        private string FindJava8WithRetry()
        {
            Console.WriteLine("  Aguardando instalação do JDK...");

            for (int i = 1; i <= 10; i++)
            {
                var java = FindRealJava8Windows();
                if (java != null)
                {
                    Console.WriteLine($"  ✓ Java 8 JDK encontrado após {i} tentativa(s)");
                    return java;
                }

                Console.Write($"\r  Tentativa {i}/10...");
                Thread.Sleep(3000);
            }

            Console.WriteLine($"\n  ❌ Java 8 JDK não encontrado após 10 tentativas");
            return null;
        }

        private void UpdateEnvironmentVariables()
        {
            if (!string.IsNullOrEmpty(_javaHome))
            {
                try
                {
                    Environment.SetEnvironmentVariable("JAVA_HOME", _javaHome, EnvironmentVariableTarget.Process);

                    string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                    string javaBinPath = Path.Combine(_javaHome, "bin");

                    if (!currentPath.Contains(javaBinPath))
                    {
                        string newPath = _isWindows
                            ? $"{javaBinPath};{currentPath}"
                            : $"{javaBinPath}:{currentPath}";

                        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Não foi possível atualizar variáveis de ambiente: {ex.Message}");
                }
            }
        }

        private static void KillProcessOnPortWindows(int port)
        {
            try
            {
                Console.WriteLine($"  Verificando processos na porta {port}...");

                var netstatProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c netstat -ano | findstr :{port}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                netstatProcess.Start();
                string output = netstatProcess.StandardOutput.ReadToEnd();
                netstatProcess.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains("LISTENING") || line.Contains("ESTABLISHED"))
                        {
                            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 5)
                            {
                                string pid = parts[parts.Length - 1];

                                try
                                {
                                    Console.WriteLine($"  Finalizando PID {pid} na porta {port}...");

                                    var killProcess = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "taskkill",
                                            Arguments = $"/F /PID {pid}",
                                            UseShellExecute = false,
                                            CreateNoWindow = true,
                                            RedirectStandardOutput = true,
                                            RedirectStandardError = true
                                        }
                                    };

                                    killProcess.Start();
                                    killProcess.WaitForExit();
                                    Console.WriteLine($"  ✓ Processo {pid} finalizado");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"  ⚠ Erro ao finalizar PID {pid}: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(2000);

                var javaKillProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c for /f \"tokens=5\" %a in ('netstat -ano ^| findstr :{port}') do taskkill /F /PID %a 2>nul",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                javaKillProcess.Start();
                javaKillProcess.WaitForExit();

                Thread.Sleep(1000);

                Console.WriteLine($"  ✓ Porta {port} liberada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Erro ao verificar/matar processos na porta {port}: {ex.Message}");
            }
        }

        public static void KillProcessOnPort(int port = 8080)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    KillProcessOnPortWindows(port);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    KillProcessOnPortLinux(port);
                }

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Aviso ao verificar processos na porta {port}: {ex.Message}");
            }
        }

        private static void KillProcessOnPortLinux(int port)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"lsof -ti:{port} | xargs kill -9 2>/dev/null || true\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"  ✓ Processos na porta {port} finalizados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Erro ao matar processos no Linux: {ex.Message}");
            }
        }

        public void Start()
        {
            int maxRetries = 2;
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"\n╔══════════════════════════════════════════════════════════╗");
                    Console.WriteLine($"║  INICIANDO SIKULI REST API (Tentativa {attempt}/{maxRetries})              ║");
                    Console.WriteLine($"╚══════════════════════════════════════════════════════════╝\n");
                    Console.WriteLine($"  • Porta: {_port}");
                    Console.WriteLine($"  • Java: {_javaExe}");
                    Console.WriteLine($"  • JAVA_HOME: {_javaHome}");
                    Console.WriteLine($"  • Sikuli REST JAR: {_sikuliJar}\n");

                    bool serverStarted = false;
                    bool hasFatalError = false;

                    _process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Console.WriteLine($"  [SIKULI] {e.Data}");
                    };

                    _process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine($"  [SIKULI ERRO] {e.Data}");

                            if (e.Data.Contains("Started ServerConnector"))
                            {
                                serverStarted = true;
                            }

                            if (e.Data.Contains("Fatal Error 110") ||
                                e.Data.Contains("WinUtil.dll") ||
                                e.Data.Contains("Terminating SikuliX"))
                            {
                                hasFatalError = true;
                            }
                        }
                    };

                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    Console.WriteLine("  Aguardando inicialização do Sikuli REST API...");

                    // Aguarda servidor iniciar
                    for (int i = 0; i < 15; i++)
                    {
                        Thread.Sleep(1000);

                        if (_process.HasExited)
                        {
                            Console.WriteLine($"  ⚠ Processo encerrou inesperadamente");
                            break;
                        }

                        if (serverStarted)
                        {
                            Console.WriteLine($"  ✓ Servidor detectado após {i + 1} segundo(s)");
                            Console.WriteLine("  Verificando estabilidade...");
                            Thread.Sleep(5000);
                            break;
                        }
                    }

                    if (hasFatalError && attempt < maxRetries)
                    {
                        Console.WriteLine($"  ⚠ Erro fatal detectado nas bibliotecas nativas");
                        Console.WriteLine($"  → Reiniciando (as DLLs serão recarregadas)...\n");
                        Stop();
                        Thread.Sleep(3000);
                        continue;
                    }

                    if (_process.HasExited && attempt < maxRetries)
                    {
                        Console.WriteLine($"  ⚠ Processo encerrou");
                        Console.WriteLine($"  → Reiniciando...\n");
                        Thread.Sleep(2000);
                        continue;
                    }

                    if (!_process.HasExited)
                    {
                        Console.WriteLine($"\n╔══════════════════════════════════════════════════════════╗");
                        Console.WriteLine($"║  SIKULI REST API INICIADA COM SUCESSO NA PORTA {_port,-18} ║");
                        Console.WriteLine($"╚══════════════════════════════════════════════════════════╝\n");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"  ❌ Erro na tentativa {attempt}: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"  → Tentando novamente em 2 segundos...\n");
                        Thread.Sleep(2000);
                    }
                }
            }

            throw new Exception($"Sikuli REST API falhou após {maxRetries} tentativas.", lastException);
        }

        public void Stop()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    Console.WriteLine("  Parando Sikuli REST API...");
                    _process.Kill();
                    _process.WaitForExit(3000);
                    Console.WriteLine("  ✓ Sikuli REST API parado");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Aviso ao parar Sikuli REST API: {ex.Message}");
                }
            }
        }

        public void Restart()
        {
            Console.WriteLine("\n  Reiniciando Sikuli REST API...");
            Stop();
            KillProcessOnPort(_port);
            Thread.Sleep(2000);
            Start();
        }

        private void SetupNativeLibs()
        {
            Console.WriteLine($"\n[4/4] VERIFICANDO BIBLIOTECAS NATIVAS...");

            string libsFolder = Path.Combine(_jdkFolder, "libs");
            Directory.CreateDirectory(libsFolder);

            // Adiciona libs ao PATH do sistema para que as DLLs sejam encontradas
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (!currentPath.Contains(libsFolder))
            {
                Environment.SetEnvironmentVariable("PATH", $"{libsFolder};{currentPath}", EnvironmentVariableTarget.Process);
                Console.WriteLine($"  ✓ Pasta libs adicionada ao PATH");
            }

            // Dicionário com as DLLs necessárias (URL principal e fallback)
            var requiredDlls = new Dictionary<string, (string url, string fallbackUrl, bool required)>
            {
                { "WinUtil.dll", (WINUTIL_DLL_URL, WINUTIL_DLL_MEDIA_URL, true) },
                { "VisionProxy.dll", (VISIONPROXY_DLL_URL, VISIONPROXY_DLL_MEDIA_URL, true) }
            };

            Console.WriteLine($"  Verificando bibliotecas nativas...");

            int foundCount = 0;
            int totalRequired = requiredDlls.Count(d => d.Value.required);

            foreach (var dll in requiredDlls)
            {
                string dllPath = Path.Combine(libsFolder, dll.Key);

                if (File.Exists(dllPath))
                {
                    Console.WriteLine($"  ✓ {dll.Key} já existe");
                    foundCount++;
                    continue;
                }

                try
                {
                    Console.WriteLine($"  Baixando {dll.Key}...");
                    DownloadFileWithRetry(dll.Value.url, dllPath, dll.Key, dll.Value.fallbackUrl);
                    Console.WriteLine($"  ✓ {dll.Key} baixado com sucesso");
                    foundCount++;
                }
                catch (Exception ex)
                {
                    if (dll.Value.required)
                    {
                        Console.WriteLine($"  ❌ ERRO CRÍTICO ao baixar {dll.Key}: {ex.Message}");
                        Console.WriteLine($"  💡 Baixe manualmente de: {dll.Value.url}");
                        Console.WriteLine($"     E salve em: {dllPath}");
                        throw new Exception($"DLL obrigatória {dll.Key} não pôde ser baixada");
                    }
                    else
                    {
                        Console.WriteLine($"  ⚠ {dll.Key} não encontrada (opcional): {ex.Message}");
                        Console.WriteLine($"  💡 Se houver erros, baixe de: {dll.Value.url}");
                    }
                }
            }

            // Verifica se o OpenCV está instalado no sistema
            if (!File.Exists(Path.Combine(libsFolder, "opencv_java2413.dll")))
            {
                Console.WriteLine($"\n  ⚠ OpenCV não encontrado - tentando localizar no sistema...");
                TryFindSystemOpenCV(libsFolder);
            }

            Console.WriteLine($"\n  ✓ Configuração de bibliotecas nativas concluída ({foundCount} DLLs disponíveis)\n");
        }

        private void TryFindSystemOpenCV(string libsFolder)
        {
            // Procura OpenCV instalado no sistema
            string[] possiblePaths = {
                @"C:\opencv\build\java\x64\opencv_java2413.dll",
                @"C:\opencv\build\java\x86\opencv_java2413.dll",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "opencv", "build", "java", "x64", "opencv_java2413.dll"),
                Path.Combine(_javaHome, "bin", "opencv_java2413.dll")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        string destPath = Path.Combine(libsFolder, "opencv_java2413.dll");
                        File.Copy(path, destPath, true);
                        Console.WriteLine($"  ✓ OpenCV copiado de: {path}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ⚠ Erro ao copiar OpenCV: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"  → OpenCV não encontrado - o Sikuli pode funcionar sem ele para operações básicas");
        }

        private void SetupEnvironment()
        {
            Console.WriteLine($"\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  CONFIGURANDO AMBIENTE SIKULI REST API - {_platform,-26} ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════╝\n");

            try
            {
                _sikuliJar = Path.Combine(_jdkFolder, "sikulirestapi-1.0.jar");
                string jdkInstallerPath = GetJdkInstallerPath();

                // 1. Baixar Sikuli REST JAR (API)
                if (!File.Exists(_sikuliJar))
                {
                    Console.WriteLine($"[2/4] BAIXANDO SIKULI REST API JAR...");
                    DownloadFileWithRetry(SIKULI_REST_JAR_URL, _sikuliJar, "Sikuli REST API JAR", SIKULI_REST_JAR_MEDIA_URL);
                }
                else
                {
                    long fileSize = new FileInfo(_sikuliJar).Length / 1024;
                    Console.WriteLine($"[2/4] ✓ Sikuli REST API JAR já existe ({fileSize} KB)\n");
                }

                // 2. VERIFICAR SE TEM JDK 8 REAL
                Console.WriteLine($"[3/4] VERIFICANDO JDK 8...");

                _javaExe = FindRealJava8Windows();
                bool needsJdkInstall = false;

                if (_javaExe != null)
                {
                    _javaHome = GetJavaHomeFromJavaExe(_javaExe);
                    Console.WriteLine($"  ✓ Java 8 encontrado!");
                    Console.WriteLine($"  • Java Home: {_javaHome}");
                    Console.WriteLine($"  • Java Exe: {_javaExe}");

                    string javacPath = _javaExe.Replace("java.exe", "javac.exe");
                    if (!File.Exists(javacPath))
                    {
                        Console.WriteLine($"  ⚠ Aviso: Java encontrado parece ser apenas JRE, não JDK");
                        Console.WriteLine($"  ⚠ Baixando JDK completo para garantir funcionamento...");
                        needsJdkInstall = true;
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ É um JDK completo (tem javac.exe)");
                    }
                }
                else
                {
                    Console.WriteLine($"  ⚠ Java 8 JDK não encontrado - será instalado");
                    needsJdkInstall = true;
                }

                // 3. SE PRECISAR INSTALAR JDK
                if (needsJdkInstall)
                {
                    Console.WriteLine($"\nINSTALANDO JDK 8u202 PARA {_platform}...\n");

                    bool needsDownload = true;
                    if (File.Exists(jdkInstallerPath))
                    {
                        long fileSizeMB = new FileInfo(jdkInstallerPath).Length / 1024 / 1024;
                        if (fileSizeMB > 100)
                        {
                            Console.WriteLine($"  ✓ JDK instalador já baixado ({fileSizeMB} MB)\n");
                            needsDownload = false;
                        }
                        else
                        {
                            Console.WriteLine($"  ⚠ Arquivo JDK inválido ({fileSizeMB} MB) - será baixado novamente\n");
                            File.Delete(jdkInstallerPath);
                        }
                    }

                    if (needsDownload)
                    {
                        Console.WriteLine($"BAIXANDO JDK 8 PARA {_platform}...");
                        string jdkUrl = _isWindows ? WINDOWS_JDK_URL : LINUX_JDK_URL;
                        string jdkMediaUrl = _isWindows ? WINDOWS_JDK_MEDIA_URL : LINUX_JDK_MEDIA_URL;
                        DownloadFileWithRetry(jdkUrl, jdkInstallerPath, $"JDK 8 {_platform}", jdkMediaUrl);
                    }

                    // Instalar JDK
                    if (_isWindows)
                    {
                        _javaHome = InstallJdkWindows(jdkInstallerPath);
                    }
                    else if (_isLinux)
                    {
                        _javaHome = InstallJdkLinux(jdkInstallerPath);
                    }

                    _javaExe = FindJava8WithRetry();
                    _javaHome = _javaHome ?? GetJavaHomeFromJavaExe(_javaExe);

                    if (_javaExe == null)
                    {
                        throw new Exception($"JDK 8 não foi instalado corretamente no {_platform}");
                    }

                    SetupNativeLibs();
                    UpdateEnvironmentVariables();

                    Console.WriteLine($"\n  ✓ JDK 8 instalado com sucesso!");
                    Console.WriteLine($"  • Java Home: {_javaHome}");
                    Console.WriteLine($"  • Java Exe: {_javaExe}\n");
                }
                else
                {
                    SetupNativeLibs();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERRO NO SETUP: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}