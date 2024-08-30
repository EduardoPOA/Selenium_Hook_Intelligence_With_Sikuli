/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Rest;

namespace Hook_Validator.Util
{
    /// <summary>
    /// Descrição do Util.
    /// </summary>
    public static class Util
	{
		private static ActionLog _Log;
		public static ActionLog Log
		{
			get
			{
				if(_Log == null)
				{
					_Log = new ActionLog();
				}
				return _Log;
			}
		}
	}
}
