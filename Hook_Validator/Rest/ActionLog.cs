/*
 * @author Eduardo Oliveira
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace Hook_Validator.Rest
{
	/// <summary>
	/// Descrição do Log.
	/// </summary>
	public class ActionLog
	{
		public static readonly String LogFileName = "log";
		public static readonly String LogFolder = "Sikuli4Net.Client.Logs";
		
		private String WorkingDir;
		private String LogFolderPath;
        public readonly String LogPath;
		
		public ActionLog()
		{
			WorkingDir = Directory.GetCurrentDirectory();
			LogFolderPath = Path.Combine(WorkingDir,LogFolder);
			if(!Directory.Exists(LogFolderPath))
			{
				Directory.CreateDirectory(LogFolderPath);
			}
			DateTime now = DateTime.Now;
			LogPath = Path.Combine(LogFolderPath,LogFileName + "." +now.ToShortDateString().Replace("/","") + now.ToShortTimeString().Replace(":","") + ".txt");
			Console.WriteLine("--Log for this test run can be found at: " + LogPath + "--");
			File.Create(LogPath).Close();
		}
		
		/// <summary>
		/// Método escrever uma linha para o logfile e para o console.
		/// </summary>
		/// <param name="message"></param>
		public void WriteLine(String message)
		{
			List<String> line = new List<String>();
			if(File.Exists(LogPath))
			{
				String [] currentLines = File.ReadAllLines(LogPath);
				line.AddRange(currentLines);
			}
			line.Add(":::" + message + ":::");
			File.WriteAllLines(LogPath,line.ToArray());
			Console.WriteLine(":::" + message + ":::");
		}
	}
}
