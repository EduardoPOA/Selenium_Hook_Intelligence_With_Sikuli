/*
 * @author Eduardo Oliveira
 */
using Hook_Validator.Rest;
using System;

namespace Hook_Validator.Json
{
    public class json_DragDrop
    {
        public json_Pattern jClickPattern { get; set; }
        public json_Pattern jDropPattern { get; set; }
        public String jKeyModifier { get; set; }

        public json_DragDrop(json_Pattern clickPattern, json_Pattern dropPattern, KeyModifier kmod = KeyModifier.NONE)
        {
            jClickPattern = clickPattern;
            jDropPattern = dropPattern;
            jKeyModifier = kmod.ToString();
        }
    }
}
