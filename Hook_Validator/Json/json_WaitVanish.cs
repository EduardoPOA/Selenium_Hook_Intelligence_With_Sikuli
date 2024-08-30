/*
 * @author Eduardo Oliveira
 */
using Newtonsoft.Json;
using System;

namespace Hook_Validator.Json
{
    public class json_WaitVanish
    {
        public json_Pattern jPattern { get; set; }
        public Double timeout { get; set; }
        public bool patternDisappeared { get; set; }
        public json_Result jResult { get; set; }

        public json_WaitVanish(json_Pattern ptrn, Double tmout)
        {
            jPattern = ptrn;
            timeout = tmout;
        }

        public static json_WaitVanish getJWaitVanish(String json)
        {
            json_WaitVanish jWaitVanish = JsonConvert.DeserializeObject<json_WaitVanish>(json);
            return jWaitVanish;
        }
    }
}
