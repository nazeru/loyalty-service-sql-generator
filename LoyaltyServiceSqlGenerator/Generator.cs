using System.Text.RegularExpressions;

namespace LoyaltyServiceSqlGenerator;

public static class Generator
{
    public static void Generate(List<Mcc> entries, string? filePath = "./output.txt")
    {
        var mccMap = new Dictionary<string, (Guid MccId, Guid MccHistId)>();
        var categoryMap = new Dictionary<string, (Guid CategoryId, Guid CategoryHistId)>();
        
        using var writer = new StreamWriter(filePath, append: false);

        void Write(string text)
        {
            Console.WriteLine(text);
            writer.WriteLine(text);
        }
        
        string NormalizeWhitespace(string input) =>
            string.IsNullOrWhiteSpace(input) ? "" : Regex.Replace(input.Trim(), @"\s+", " ");

        foreach (var entry in entries)
        {
            var mccCode = NormalizeWhitespace(entry.Code);
            var mccName = NormalizeWhitespace(entry.Name);
            var mccCategoryName = NormalizeWhitespace(entry.Category);
            
            var mccCategoryKey = mccCategoryName.ToLowerInvariant();
            
            if (!categoryMap.ContainsKey(mccCategoryKey))
            {
                var categoryId = Guid.NewGuid();
                var categoryHistId = Guid.NewGuid();
                categoryMap[mccCategoryKey] = (categoryId, categoryHistId);

                Write($"""
                       INSERT INTO mcc_category (id, name, description) VALUES
                       ('{categoryId}', '{mccCategoryName}', '');

                       INSERT INTO mcc_category_hist (id, mcc_cat_id, name, description, start_date) VALUES
                       ('{categoryHistId}', '{categoryId}', '{mccCategoryName}', '', NOW());
                       """);
            }
            
            if (!mccMap.ContainsKey(mccCode))
            {
                var mccId = Guid.NewGuid();
                var mccHistId = Guid.NewGuid();
                mccMap[mccCode] = (mccId, mccHistId);

                Write($"""
                       INSERT INTO mcc (id, mcc, name) VALUES
                       ('{mccId}', '{mccCode}', '{mccName}');

                       INSERT INTO mcc_hist (id, mcc_id, mcc, name, start_date) VALUES
                       ('{mccHistId}', '{mccId}', '{mccCode}', '{mccName}', NOW());
                       """);
            }

            var (mccGuid, _) = mccMap[mccCode];
            var (categoryGuid, _) = categoryMap[mccCategoryKey];

            var linkGuid = Guid.NewGuid();
            var linkHistGuid = Guid.NewGuid();

            Write($"""
                   INSERT INTO mcc_category_mcc (id, mcc_cat_id, mcc_id) VALUES
                   ('{linkGuid}', '{categoryGuid}', '{mccGuid}');

                   INSERT INTO mcc_category_mcc_hist (id, mcc_cat_mcc_id, mcc_cat_id, mcc_id, start_date) VALUES
                   ('{linkHistGuid}', '{linkGuid}', '{categoryGuid}', '{mccGuid}', NOW());
                   """);

            Console.WriteLine();
            Console.WriteLine($"Всего MCC-категорий: {categoryMap.Count}");
            Console.WriteLine($"Всего MCC-кодов: {mccMap.Count}");
        }
    }
}
