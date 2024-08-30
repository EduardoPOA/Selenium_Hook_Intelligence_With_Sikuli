/*
 * @author Eduardo Oliveira
 */
using System;

namespace Hook_Validator.Json
{
    public class json_Wait
    {
        public json_Pattern jPattern { get; set; }
        public Double timeout { get; set; }

        public json_Wait(json_Pattern ptrn, Double tmout)
        {
            jPattern = ptrn;
            timeout = tmout;
        }
    }
}
