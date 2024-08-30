/*
 * @author Eduardo Oliveira
 */
using System;

namespace Hook_Validator.Util
{
	/// <summary>
	/// Uma exceção lançada quando um json_Result com um Resultado de FALHA é retornado do serviço, contendo a mensagem de erro.
	/// </summary>
	public class SikuliActionException : Exception
	{
		public SikuliActionException() : base()
		{
		}
		
		public SikuliActionException(ActionResult result, String message) : base("Result: " + result.ToString() + message)
		{
		}
	}
}
