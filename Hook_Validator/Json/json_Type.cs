/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Rest;
using System;

namespace Hook_Validator.Json
{
    public class json_Type
    {
        public json_Pattern jPattern { get; set; }
        public String jKeyModifier { get; set; }
        public String text { get; set; }

        public json_Type(json_Pattern ptrn, String txt, KeyModifier kmod = KeyModifier.NONE)
        {
            jPattern = ptrn;
            jKeyModifier = kmod.ToString();
            text = txt;
        }
    }
}
