using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

namespace CoffeeShop
{
    internal class Program
    {
        static string conn = null;
        static DataSet dataSet = new DataSet("CoffeeShopDataSet");
        static DataTable table = null;
        static SqlDataAdapter adapter = null;
        static SqlCommandBuilder commandBuilder = null;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            conn = ConfigurationManager.ConnectionStrings["CoffeeShopConnection"].ConnectionString;
            bool isConnected = false;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Головне меню ---");
                Console.WriteLine($"Статус підключення: {(isConnected ? "Під'єднано" : "Від'єднано")}");
                Console.WriteLine(" 1. Під'єднатися до БД");
                Console.WriteLine(" 2. Виведення");
                Console.WriteLine(" 3. Додати");
                Console.WriteLine(" 4. Редагувати");
                Console.WriteLine(" 5. Видалити");
                Console.WriteLine(" 0. Вихід");
                Console.Write(" > ");

                string choice = Console.ReadLine() ?? "";
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(conn))
                            {
                                connection.Open();
                            }

                            adapter = new SqlDataAdapter("SELECT * FROM Coffee", conn);
                            commandBuilder = new SqlCommandBuilder(adapter);

                            if (dataSet.Tables.Contains("Coffee"))
                                dataSet.Tables["Coffee"].Clear();

                            adapter.Fill(dataSet, "Coffee");
                            table = dataSet.Tables["Coffee"];
                            table.PrimaryKey = new DataColumn[] { table.Columns["Id"] };

                            isConnected = true;
                            Console.WriteLine("Підключення успішне!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка підключення: {ex.Message}");
                        }
                        break;

                    case "2":
                        CheckConnectionAndDo(isConnected, () => {
                            SelectMenu();
                        });
                        break;

                    case "3":
                        CheckConnectionAndDo(isConnected, () => {
                            DataRow newRow = table.NewRow();
                            Console.Write("Назва: ");
                            newRow["Name"] = Console.ReadLine();

                            Console.Write("Країна: ");
                            newRow["Country"] = Console.ReadLine();

                            Console.Write("Вид: ");
                            newRow["Type"] = Console.ReadLine();

                            Console.Write("Опис: ");
                            newRow["Description"] = Console.ReadLine();

                            Console.Write("Вага (г): ");
                            newRow["Weight"] = int.Parse(Console.ReadLine() ?? "0");

                            Console.Write("Ціна: ");
                            newRow["Price"] = decimal.Parse(Console.ReadLine() ?? "0");

                            table.Rows.Add(newRow);
                            adapter.Update(table);
                            Console.WriteLine("Запис успішно додано!");
                        });
                        break;

                    case "4":
                        CheckConnectionAndDo(isConnected, () => {
                            PrintTable(table);
                            Console.Write("\nВведіть ID: ");
                            if (int.TryParse(Console.ReadLine(), out int idToUpdate))
                            {
                                DataRow rowToEdit = table.Rows.Find(idToUpdate);
                                if (rowToEdit != null)
                                {
                                    Console.Write($"Нова назва ({rowToEdit["Name"]}): ");
                                    string name = Console.ReadLine() ?? "";
                                    if (!string.IsNullOrEmpty(name)) rowToEdit["Name"] = name;

                                    Console.Write($"Нова вага ({rowToEdit["Weight"]}): ");
                                    string wStr = Console.ReadLine() ?? "";
                                    if (!string.IsNullOrEmpty(wStr)) rowToEdit["Weight"] = int.Parse(wStr);

                                    Console.Write($"Нова ціна ({rowToEdit["Price"]}): ");
                                    string pStr = Console.ReadLine() ?? "";
                                    if (!string.IsNullOrEmpty(pStr)) rowToEdit["Price"] = decimal.Parse(pStr);

                                    adapter.Update(table);
                                    Console.WriteLine("Запис оновлено!");
                                }
                                else
                                {
                                    Console.WriteLine("Запис не знайдено");
                                }
                            }
                        });
                        break;

                    case "5":
                        CheckConnectionAndDo(isConnected, () => {
                            PrintTable(table);
                            Console.Write("\nВведіть ID: ");
                            if (int.TryParse(Console.ReadLine(), out int idToDelete))
                            {
                                DataRow rowToDelete = table.Rows.Find(idToDelete);
                                if (rowToDelete != null)
                                {
                                    rowToDelete.Delete();
                                    adapter.Update(table);
                                    Console.WriteLine("Запис видалено!");
                                }
                                else
                                {
                                    Console.WriteLine("Запис не знайдено");
                                }
                            }
                        });
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Невірний пункт меню");
                        break;
                }

                Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                Console.ReadKey();
            }
        }

        static void SelectMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("------- Меню виведення -------");
                Console.WriteLine(" 1. Вся інформація з таблиці");
                Console.WriteLine(" 2. Назви усіх сортів кави");
                Console.WriteLine(" 3. Кава виду «арабіка»");
                Console.WriteLine(" 4. Кава виду «робуста»");
                Console.WriteLine(" 5. Кава виду «купаж/бленд»");
                Console.WriteLine(" 6. Назви сортів вагою <= 200 грамів");
                Console.WriteLine(" 7. Мінімальна ціна кави");
                Console.WriteLine(" 8. Максимальна ціна кави");
                Console.WriteLine(" 9. Середня ціна кави");
                Console.WriteLine("10. Кількість сортів з мінімальною ціною");
                Console.WriteLine("11. Кількість сортів з максимальною ціною");
                Console.WriteLine("12. Кількість арабіки, робусти, купажу");
                Console.WriteLine("13. Кава, в описі якої є «вишня»");
                Console.WriteLine("14. Кава в заданому діапазоні ціни");
                Console.WriteLine("15. Кава в заданому діапазоні ваги");
                Console.WriteLine("16. Кава із зазначених країн");
                Console.WriteLine("17. Кількість сортів кави з кожної країни");
                Console.WriteLine("18. Середня вага кави по кожній країні");
                Console.WriteLine("19. Топ-3 найдешевші сорти з певної країни");
                Console.WriteLine("20. Топ-3 найдорожчі сорти з певної країни");
                Console.WriteLine("21. Топ-3 найдешевші сорти по всіх країнах");
                Console.WriteLine("22. Топ-3 найдорожчі сорти по всіх країнах");
                Console.WriteLine("23. Топ-3 країн за кількістю сортів");
                Console.WriteLine("24. Топ-3 країн за кількістю грамів");
                Console.WriteLine("25. Топ-3 арабіки за вагою");
                Console.WriteLine("26. Топ-3 робусти за вагою");
                Console.WriteLine("27. Топ-3 купажу за вагою");
                Console.WriteLine("28. Топ-3 кави по кожному виду за вагою");
                Console.WriteLine(" 0. Назад у головне меню");
                Console.Write(" >] ");

                string subChoice = Console.ReadLine() ?? "";
                Console.WriteLine();

                if (subChoice == "0") break;

                switch (subChoice)
                {
                    case "1":
                        ExecuteAndPrintSql("SELECT * FROM Coffee");
                        break;
                    case "2":
                        ExecuteAndPrintSql("SELECT Name FROM Coffee");
                        break;
                    case "3":
                        ExecuteAndPrintSql("SELECT * FROM Coffee WHERE Type = N'арабіка'");
                        break;
                    case "4":
                        ExecuteAndPrintSql("SELECT * FROM Coffee WHERE Type = N'робуста'");
                        break;
                    case "5":
                        ExecuteAndPrintSql("SELECT * FROM Coffee WHERE Type = N'купаж/бленд'");
                        break;
                    case "6":
                        ExecuteAndPrintSql("SELECT Name, Weight FROM Coffee WHERE Weight <= 200");
                        break;
                    case "7":
                        ExecuteAndPrintSql("SELECT MIN(Price) AS MinPrice FROM Coffee");
                        break;
                    case "8":
                        ExecuteAndPrintSql("SELECT MAX(Price) AS MaxPrice FROM Coffee");
                        break;
                    case "9":
                        ExecuteAndPrintSql("SELECT AVG(Price) AS AvgPrice FROM Coffee");
                        break;
                    case "10":
                        ExecuteAndPrintSql("SELECT COUNT(*) AS CountMinPrice FROM Coffee WHERE Price = (SELECT MIN(Price) FROM Coffee)");
                        break;
                    case "11":
                        ExecuteAndPrintSql("SELECT COUNT(*) AS CountMaxPrice FROM Coffee WHERE Price = (SELECT MAX(Price) FROM Coffee)");
                        break;
                    case "12":
                        ExecuteAndPrintSql("SELECT Type, COUNT(*) AS Count FROM Coffee GROUP BY Type");
                        break;
                    case "13":
                        ExecuteAndPrintSql("SELECT * FROM Coffee WHERE Description LIKE N'%вишня%'");
                        break;
                    case "14":
                        Console.Write("Мін. ціна: ");
                        string minP = Console.ReadLine() ?? "0";
                        Console.Write("Макс. ціна: ");
                        string maxP = Console.ReadLine() ?? "999999";
                        ExecuteAndPrintSql($"SELECT * FROM Coffee WHERE Price >= {minP} AND Price <= {maxP}");
                        break;
                    case "15":
                        Console.Write("Мін. вага (г): ");
                        string minW = Console.ReadLine() ?? "0";
                        Console.Write("Макс. вага (г): ");
                        string maxW = Console.ReadLine() ?? "999999";
                        ExecuteAndPrintSql($"SELECT * FROM Coffee WHERE Weight >= {minW} AND Weight <= {maxW}");
                        break;
                    case "16":
                        Console.Write("Введіть країни через кому (наприклад: Бразилія, Ефіопія): ");
                        string countryInput = Console.ReadLine() ?? "";
                        string formattedCountries = string.Join(",", countryInput.Split(',').Select(c => $"N'{c.Trim()}'"));
                        ExecuteAndPrintSql($"SELECT * FROM Coffee WHERE Country IN ({formattedCountries})");
                        break;
                    case "17":
                        ExecuteAndPrintSql("SELECT Country, COUNT(*) AS CoffeeCount FROM Coffee GROUP BY Country");
                        break;
                    case "18":
                        ExecuteAndPrintSql("SELECT Country, AVG(CAST(Weight AS FLOAT)) AS AvgWeight FROM Coffee GROUP BY Country");
                        break;
                    case "19":
                        Console.Write("Введіть країну: ");
                        string cTarget = Console.ReadLine() ?? "";
                        ExecuteAndPrintSql($"SELECT TOP 3 * FROM Coffee WHERE Country = N'{cTarget}' ORDER BY Price ASC");
                        break;
                    case "20":
                        Console.Write("Введіть країну: ");
                        string cTarget2 = Console.ReadLine() ?? "";
                        ExecuteAndPrintSql($"SELECT TOP 3 * FROM Coffee WHERE Country = N'{cTarget2}' ORDER BY Price DESC");
                        break;
                    case "21":
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee ORDER BY Price ASC");
                        break;
                    case "22":
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee ORDER BY Price DESC");
                        break;
                    case "23":
                        ExecuteAndPrintSql("SELECT TOP 3 Country, COUNT(*) AS CoffeeCount FROM Coffee GROUP BY Country ORDER BY COUNT(*) DESC");
                        break;
                    case "24":
                        ExecuteAndPrintSql("SELECT TOP 3 Country, SUM(Weight) AS TotalWeight FROM Coffee GROUP BY Country ORDER BY SUM(Weight) DESC");
                        break;
                    case "25":
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'арабіка' ORDER BY Weight DESC");
                        break;
                    case "26":
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'робуста' ORDER BY Weight DESC");
                        break;
                    case "27":
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'купаж/бленд' ORDER BY Weight DESC");
                        break;
                    case "28":
                        Console.WriteLine("--- Арабіка ---");
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'арабіка' ORDER BY Weight DESC");
                        Console.WriteLine("--- Робуста ---");
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'робуста' ORDER BY Weight DESC");
                        Console.WriteLine("--- Купаж/бленд ---");
                        ExecuteAndPrintSql("SELECT TOP 3 * FROM Coffee WHERE Type = N'купаж/бленд' ORDER BY Weight DESC");
                        break;
                    default:
                        Console.WriteLine("Невірний вибір");
                        break;
                }

                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
            }
        }

        static void ExecuteAndPrintSql(string query)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            PrintTable(dt);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка: {ex.Message}");
                    }
                }
            }
        }

        static void CheckConnectionAndDo(bool isConnected, Action action)
        {
            if (!isConnected)
            {
                Console.WriteLine("Спочатку під'єднайтеся до БД!");
                return;
            }
            action();
        }

        static void PrintTable(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine("Нічого не знайдено");
                return;
            }

            Console.WriteLine($"\nРядків: {dt.Rows.Count}");
            foreach (DataColumn col in dt.Columns)
                Console.Write($"{col.ColumnName,-14} | ");
            Console.WriteLine("\n----------------------------------------------------------------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write($"{item,-14} | ");
                Console.WriteLine();
            }
        }
    }
}


//CREATE TABLE[dbo].[Coffee]
//(
//    [Id] INT IDENTITY PRIMARY KEY,
//    [Name] NVARCHAR(100) NOT NULL,
//    [Country] NVARCHAR(100) NOT NULL,
//    [Type] NVARCHAR(50) NOT NULL,
//    [Description] NVARCHAR(MAX),
//    [Weight] INT NOT NULL,
//    [Price] DECIMAL(18,2) NOT NULL
//)
//GO


//INSERT INTO Coffee(Name, Country, Type, Description, Weight, Price) VALUES 
//(N'Ethiopia', N'Ефіопія', N'арабіка', N'вишня', 250, 150.00),
//(N'Brazil', N'Бразилія', N'арабіка', N'шоколад', 500, 180.00),
//(N'Vietnam', N'В''єтнам', N'робуста', N'міцність', 250, 110.00),
//(N'Italian', N'Італія', N'купаж/бленд', N'пінка', 1000, 210.00),
//(N'Uganda', N'Уганда', N'арабіка', N'ягоди', 250, 190.00),
//(N'Colombia', N'Колумбія', N'арабіка', N'карамель', 500, 220.00),
//(N'Morning', N'Італія', N'купаж/бленд', N'бадьорість', 250, 160.00),
//(N'Kenya', N'Кенія', N'арабіка', N'смородина', 250, 260.00),
//(N'Guatemala', N'Гватемала', N'арабіка', N'прянощі', 500, 240.00),
//(N'CostaRica', N'Коста-Рика', N'арабіка', N'мел', 250, 230.00),
//(N'India', N'Індія', N'арабіка', N'землистий', 500, 200.00),
//(N'Sumatra', N'Індонезія', N'робуста', N'тютюн', 250, 140.00),
//(N'Java', N'Індонезія', N'арабіка', N'горіх', 500, 215.00),
//(N'Classic', N'Бразилія', N'купаж/бленд', N'класика', 1000, 195.00),
//(N'Rwanda', N'Руанда', N'арабіка', N'квіти', 250, 270.00);