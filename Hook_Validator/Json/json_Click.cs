/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Rest;
using System;

namespace Hook_Validator.Json
{
    public class json_Click
    {
        public json_Pattern jPattern { get; set; }
        public String jKeyModifier { get; set; }

        public json_Click(json_Pattern ptrn, KeyModifier kmod = KeyModifier.NONE)
        {
            jPattern = ptrn;
            jKeyModifier = kmod.ToString();
        }
    }
}
