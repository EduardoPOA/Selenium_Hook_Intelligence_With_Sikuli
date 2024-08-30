/*
 * @author Eduardo Oliveira
 */
namespace Hook_Validator.Json
{
    public class json_Find
    {
        public json_Pattern jPattern { get; set; }
        public bool highlight { get; set; }

        public json_Find(json_Pattern pattrn, bool hghlght = false)
        {
            jPattern = pattrn;
            highlight = hghlght;
        }
    }
}
