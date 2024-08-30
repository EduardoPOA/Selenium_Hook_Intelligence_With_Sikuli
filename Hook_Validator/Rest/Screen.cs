﻿/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Json;
using Hook_Validator.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace Hook_Validator.Rest
{
    /// <summary>
    /// Descrição do Screen.
    /// </summary>
    public class Screen
	{
		private String serviceURL;
		
		public Screen()
		{
			serviceURL = "http://localhost:8080/sikuli/api/";
		}

        /// <summary>
        /// Método para encontrar o padrão especificado na tela
        /// </summary>
        /// <param name="pattern">O objeto padrão passado para a ferramenta de pesquisa</param>
        public void Find(Pattern pattern, bool highlight = false)
		{
            json_Find jFind = new json_Find(pattern.ToJsonPattern(), highlight);
            String jFindS = JsonConvert.SerializeObject(jFind);
            json_Result jResult = json_Result.getJResult(MakeRequest("find", jFindS));
            FailIfResultNotPASS(jResult);
		}
        /// <summary>
        /// Método para clicar no padrão especificado
        /// </summary>
        /// <param name="pattern">O objeto padrão passou para a ferramenta para clicar</param>
        /// <param name="kmod">Quaisquer modificadores de tecla para pressionar enquanto clica. exemplo: Control, Shift, Enter, etc ...</param>
        public void Click(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            if (highlight)
            {
                Find(pattern, highlight);
            }
            json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
            String jClickS = JsonConvert.SerializeObject(jClick);
            json_Result jResult = json_Result.getJResult(MakeRequest("click", jClickS));
            FailIfResultNotPASS(jResult);
        }
        public void Click(Pattern pattern, bool highlight)
        {
            Click(pattern, KeyModifier.NONE, highlight);
        }
        /// <summary>
        /// Método para clicar duas vezes no padrão especificado
        /// </summary>
        /// <param name="pattern">O objeto padrão passou para a ferramenta para clicar</param>
        public void DoubleClick(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
			if (highlight)
            {
                Find(pattern, highlight);
            }
            json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
            String jClickS = JsonConvert.SerializeObject(jClick);
            json_Result jResult = json_Result.getJResult(MakeRequest("doubleclick", jClickS));
            FailIfResultNotPASS(jResult);
        }
		public void DoubleClick(Pattern pattern, bool highlight)
		{
			DoubleClick(pattern,KeyModifier.NONE, highlight);
		}
        /// <summary>
        /// Método para clicar com o botão direito em um padrão especificado
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="kmod"></param>
        public void RightClick(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
		{
			if (highlight)
            {
                Find(pattern, highlight);
            }
			json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
			String jClickS = JsonConvert.SerializeObject(jClick);
			json_Result jResult = json_Result.getJResult(MakeRequest("rightclick",jClickS));
			FailIfResultNotPASS(jResult);
		}
		public void RightClick(Pattern pattern, bool highlight)
		{
			RightClick(pattern, KeyModifier.NONE, highlight);
		}
        /// <summary>
        /// Método para esperar que um padrão específico apareça na tela.
        /// Se não aparecer no tempo limite especificado (em segundos), a ação falhará.
        /// </summary>
        /// <param name="pattern">O objeto padrão passou para a ferramenta para esperar</param>
        /// <param name="timeout">O tempo limite, em segundos, antes que a ação falhe</param>
        public void Wait(Pattern pattern, Double timeout = 15)
        {
            json_Wait jWait = new json_Wait(pattern.ToJsonPattern(), timeout);
            String jWaitS = JsonConvert.SerializeObject(jWait);
            json_Result jResult = json_Result.getJResult(MakeRequest("wait", jWaitS));
            FailIfResultNotPASS(jResult);
        }
        /// <summary>
        /// Método para esperar que um padrão específico desapareça da tela.
        /// Se não desaparecer no tempo limite especificado, um valor false é retornado.
        /// Caso contrário, true é retornado.
        /// </summary>
        /// <param name="pattern">O objeto padrão passou para a ferramenta para esperar</param>
        /// <param name="timeout">O tempo limite, em segundos, antes que a ação retorne falso</param>
        /// <returns>Verdadeiro se o objeto desaparecer, falso caso contrário</returns>
        public bool WaitVanish(Pattern pattern, Double timeout = 15)
        {
            json_WaitVanish jWaitVanish = new json_WaitVanish(pattern.ToJsonPattern(), timeout);
            String jWaitVanishS = JsonConvert.SerializeObject(jWaitVanish);
            json_WaitVanish jWaitVanish_Result = json_WaitVanish.getJWaitVanish(MakeRequest("waitvanish", jWaitVanishS));
            FailIfResultNotPASS(jWaitVanish_Result.jResult);
            return jWaitVanish_Result.patternDisappeared;
        }
        /// <summary>
        /// Método para verificar se existe um padrão na tela, aguardando o tempo limite especificado para que o objeto apareça.
        /// Retorna verdadeiro se o objeto existe, ou então falso.
        /// </summary>
        /// <param name="pattern">O objeto padrão passado para a ferramenta de pesquisa</param>
        /// <param name="timeout">O tempo limite, em segundos, antes que a ação retorne falso</param>
        /// <returns>Verdadeiro se o objeto existir, falso caso contrário</returns>
        public bool Exists(Pattern pattern, Double timeout = 15)
        {
            json_Exists jExists = new json_Exists(pattern.ToJsonPattern(), timeout);
            String jExistsS = JsonConvert.SerializeObject(jExists);
            json_Exists jExists_Result = json_Exists.getJExists(MakeRequest("exists", jExistsS));
            FailIfResultNotPASS(jExists_Result.jResult);
            return jExists_Result.patternExists;
        }
        /// <summary>
        /// Método para digitar o texto especificado no padrão especificado após localizá-lo na tela
        /// </summary>
        /// <param name="pattern">O objeto padrão passado para a ferramenta de digitação</param>
        /// <param name="text">O texto a ser digitado no padrão, se for encontrado</param>
        /// <param name="kmod">Quaisquer modificadores de tecla a serem pressionados durante a digitação. exemplo: Control, Shift, Enter, etc ...</param>
        public void Type(Pattern pattern, String text, KeyModifier kmod = KeyModifier.NONE)
        {
            json_Type jType = new json_Type(pattern.ToJsonPattern(), text, kmod);
            String jTypeS = JsonConvert.SerializeObject(jType);
            json_Result jResult = json_Result.getJResult(MakeRequest("type", jTypeS));
            FailIfResultNotPASS(jResult);
        }
        /// <summary>
        /// Método para clicar e arrastar de um padrão para outro
        /// </summary>
        /// <param name="clickPattern">O padrão a partir do qual iniciar o arrasto</param>
        /// <param name="dropPattern">O padrão para cair em</param>
        public void DragDrop(Pattern clickPattern, Pattern dropPattern)
        {
            json_DragDrop jDragDrop = new json_DragDrop(clickPattern.ToJsonPattern(), dropPattern.ToJsonPattern());
            String jDragDropS = JsonConvert.SerializeObject(jDragDrop);
            json_Result jResult = json_Result.getJResult(MakeRequest("dragdrop",jDragDropS));
            FailIfResultNotPASS(jResult);
        }

        /// <summary>
        /// Método para fazer uma solicitação ao serviço com a extensão de URL especificada e o objeto Json especificado.
        /// </summary>
        /// <param name="requestURLExtension">A extensão de URL para a qual a solicitação é enviada. exemplo: "find"</param>
        /// <param name="jsonObject">O objeto Json, geralmente um padrão, que está sendo passado por meio da solicitação POST</param>
        /// <returns></returns>
        private String MakeRequest(String requestURLExtension, String jsonObject)
		{                     
			Util.Util.Log.WriteLine("Making Request to Service: " + serviceURL + requestURLExtension + " POST: "+jsonObject);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceURL + requestURLExtension);
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonObject);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String resultString;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                resultString = reader.ReadToEnd();
            }
            Util.Util.Log.WriteLine(resultString);
            return resultString;
		}
        /// <summary>
        /// Método para verificar o objeto json_Result e lançar uma exceção se o resultado não for PASSADO
        /// </summary>
        /// <param name="jResult">o json_Result para verificar</param>
        public void FailIfResultNotPASS(json_Result jResult)
        {
            Util.Util.Log.WriteLine("Result: " + jResult.result + " Message: " + jResult.message + " Stacktrace: " + jResult.stacktrace);
            if (!jResult.ToActionResult().Equals(ActionResult.PASS))
            {
                throw new SikuliActionException(jResult.ToActionResult(), jResult.message);
            }
        }
	}
}
