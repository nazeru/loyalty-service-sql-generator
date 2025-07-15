// See https://aka.ms/new-console-template for more information

using LoyaltyServiceSqlGenerator;

var path = "C:\\Rider\\LoyaltyServiceSqlGenerator\\LoyaltyServiceSqlGenerator\\mcc.xlsx";

var entries = Parser.Parse(path, "Лист1");

Generator.Generate(entries);