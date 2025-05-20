namespace Pretia
{
    public class PretiaDocs
    {
        private const string DOCS_ROOT = "https://docs.pretiaar.com";
        private const string COMPONENTS_SUB_PATH = "/docs/category/pretia-components";
        private const string UNITY_EXPORTER_SUB_PATH = "/docs/unity-exporter";

        public const string COMPONENTS_ROOT = DOCS_ROOT + COMPONENTS_SUB_PATH;

        public static string UnityExporterUrl()
        {
            return GetDocRoot() + UNITY_EXPORTER_SUB_PATH; 
        }

        public static string GetDocRoot()
        {
            string docRoot = DOCS_ROOT;
            string locString = Localization.GetLocString();
            if (locString != "en")
                docRoot += "/" + locString;
            return docRoot;
        }
    }
}