/*
 * @author Eduardo Oliveira
 */
using System;
using Hook_Validator.Util;
using Newtonsoft.Json;

namespace Hook_Validator.Json
{
	/// <summary>
	/// Descrição do json_Result.
	/// </summary>
	public class json_Result
	{
		public json_Result()
		{
		}
		
		public String message {get; set;}
		public String result {get; set;}
        public String stacktrace { get; set; }
		
		public ActionResult ToActionResult()
		{
			if(result.Equals(ActionResult.FAIL.ToString()))
			{
				return ActionResult.FAIL;
			}
			else if(result.Equals(ActionResult.PASS.ToString()))
			{
				return ActionResult.PASS;
			}
			else
			{
				return ActionResult.UNKNOWN;
			}
		}

        public static json_Result getJResult(String json)
        {
            json_Result jResult = JsonConvert.DeserializeObject<json_Result>(json);
            return jResult;
        }
	}
}
