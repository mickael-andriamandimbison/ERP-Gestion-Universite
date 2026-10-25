namespace erp_univ_back.Shared.Utils
{
    public static class QueryHelper
    {
        /// <summary>
        /// Échappe les caractères réservés LIKE/ILIKE (\, %, _) pour sécuriser les requêtes de recherche.
        /// </summary>
        public static string Echapper(string? valeur)
        {
            if (string.IsNullOrEmpty(valeur))
                return string.Empty;

            return valeur.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        }

        /// <summary>
        /// Échappe et formate la valeur sous forme de motif LIKE pour une recherche partielle : "%valeur%".
        /// </summary>
        public static string VersMotifRecherche(string? valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur))
                return "%";

            return $"%{Echapper(valeur.Trim())}%";
        }
    }
}
