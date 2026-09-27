using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

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
                Console.WriteLine("====================== МЕНЮ ======================");
                Console.WriteLine($"Статус з'єднання: {(isConnected ? "ПІД'ЄДНАНО" : "ВІД'ЄДНАНО")}");  
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(" 1. Під'єднатися до бази даних «Магазин кави»");
                Console.WriteLine(" 2. Відображення всієї інформації з таблиці з кавою");
                Console.WriteLine(" 3. Відображення назв усіх сортів кави");
                Console.WriteLine(" 4. Відображення кави усіх видів арабіки");
                Console.WriteLine(" 5. Відображення кави усіх видів робусти");
                Console.WriteLine(" 6. Відображення купажу/бленду");
                Console.WriteLine(" 7. Показати назви сортів кави ( <= 200 грамів )");
                Console.WriteLine(" 8. Показати мінімальну собівартість кави");
                Console.WriteLine(" 9. Показати максимальну собівартість кави");
                Console.WriteLine("10. Показати середню собівартість кави");
                Console.WriteLine("11. Кількість сортів з мінімальною собівартістю");
                Console.WriteLine("12. Кількість сортів з максимальною собівартістю");
                Console.WriteLine("13. Кількість арабіки, робусти, купажу/бленду");
                Console.WriteLine(" 0. Вихід");
                Console.WriteLine("==================================================");
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
                                Console.WriteLine("Успішно під'єднано до БД!");
                            }

                            adapter = new SqlDataAdapter("SELECT * FROM Coffee", conn);
                            commandBuilder = new SqlCommandBuilder(adapter);

                            if (dataSet.Tables.Contains("Coffee"))
                                dataSet.Tables["Coffee"].Clear();

                            adapter.Fill(dataSet, "Coffee");
                            table = dataSet.Tables["Coffee"];

                            isConnected = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Помилка: {ex.Message}");
                        }
                        break;

                    case "2":
                        CheckConnectionAndDo(isConnected, () => {
                            PrintTable(table);
                        });
                        break;

                    case "3":
                        CheckConnectionAndDo(isConnected, () => {
                            foreach (DataRow row in table.Rows)
                                Console.WriteLine($"- {row["Name"]}");
                        });
                        break;

                    case "4":
                        CheckConnectionAndDo(isConnected, () => {;
                            DataRow[] rows = table.Select("Type = 'арабіка'");
                            foreach (var r in rows)
                                Console.WriteLine($"- {r["Name"]} ({r["Country"]}) | Вага: {r["Weight"]}г | Ціна: {r["Price"]} грн");
                        });
                        break;

                    case "5":
                        CheckConnectionAndDo(isConnected, () => {
                            DataRow[] rows = table.Select("Type = 'робуста'");
                            foreach (var r in rows)
                                Console.WriteLine($"- {r["Name"]} ({r["Country"]}) | Вага: {r["Weight"]}г | Ціна: {r["Price"]} грн");
                        });
                        break;

                    case "6":
                        CheckConnectionAndDo(isConnected, () => {
                            DataRow[] rows = table.Select("Type = 'купаж/бленд'");
                            foreach (var r in rows)
                                Console.WriteLine($"- {r["Name"]} ({r["Country"]}) | Вага: {r["Weight"]}г | Ціна: {r["Price"]} грн");
                        });
                        break;

                    case "7":
                        CheckConnectionAndDo(isConnected, () => {
                            DataRow[] rows = table.Select("Weight <= 200");
                            foreach (var r in rows)
                                Console.WriteLine($"- {r["Name"]} | Залишок: {r["Weight"]}г");
                        });
                        break;

                    case "8":
                        CheckConnectionAndDo(isConnected, () => {
                            decimal minCost = GetMinCost(table);
                            Console.WriteLine($"Мінімальна собівартість кави: {minCost} грн");
                        });
                        break;

                    case "9":
                        CheckConnectionAndDo(isConnected, () => {
                            decimal maxCost = GetMaxCost(table);
                            Console.WriteLine($"Максимальна собівартість кави: {maxCost} грн");
                        });
                        break;

                    case "10":
                        CheckConnectionAndDo(isConnected, () => {
                            decimal avgCost = GetAvgCost(table);
                            Console.WriteLine($"Середня собівартість кави: {avgCost:F2} грн");
                        });
                        break;

                    case "11":
                        CheckConnectionAndDo(isConnected, () => {
                            decimal minCost = GetMinCost(table);
                            int count = 0;
                            foreach (DataRow row in table.Rows)
                            {
                                if (Convert.ToDecimal(row["Price"]) == minCost) count++;
                            }
                            Console.WriteLine($"Кількість сортів з мінімальною собівартістю ({minCost} грн): {count}");
                        });
                        break;

                    case "12":
                        CheckConnectionAndDo(isConnected, () => {
                            decimal maxCost = GetMaxCost(table);
                            int count = 0;
                            foreach (DataRow row in table.Rows)
                            {
                                if (Convert.ToDecimal(row["Price"]) == maxCost) count++;
                            }
                            Console.WriteLine($"Кількість сортів з максимальною собівартістю ({maxCost} грн): {count}");
                        });
                        break;

                    case "13":
                        CheckConnectionAndDo(isConnected, () => {
                            int arabica = 0, robusta = 0, blend = 0;
                            foreach (DataRow row in table.Rows)
                            {
                                string type = row["Type"].ToString()?.ToLower() ?? "";
                                if (type == "арабіка") arabica++;
                                else if (type == "робуста") robusta++;
                                else if (type == "купаж/бленд") blend++;
                            }
                            Console.WriteLine($"Арабіка: {arabica}");
                            Console.WriteLine($"Робуста: {robusta}");
                            Console.WriteLine($"Купаж/бленд: {blend}");
                        });
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }

                Console.WriteLine("\nНатисни будь-яку клавішу, щоб продовжити...");
                Console.ReadKey();
            }
        }

        static void CheckConnectionAndDo(bool isConnected, Action action)
        {
            if (!isConnected)
            {
                Console.WriteLine("Спочатку під'єднайся до бази даних!");
                return;
            }
            action();
        }

        static decimal GetMinCost(DataTable dt)
        {
            decimal min = decimal.MaxValue;
            foreach (DataRow row in dt.Rows)
            {
                decimal price = Convert.ToDecimal(row["Price"]);
                if (price < min) min = price;
            }
            return min;
        }

        static decimal GetMaxCost(DataTable dt)
        {
            decimal max = decimal.MinValue;
            foreach (DataRow row in dt.Rows)
            {
                decimal price = Convert.ToDecimal(row["Price"]);
                if (price > max) max = price;
            }
            return max;
        }

        static decimal GetAvgCost(DataTable dt)
        {
            if (dt.Rows.Count == 0) return 0;
            decimal sum = 0;
            foreach (DataRow row in dt.Rows)
            {
                sum += Convert.ToDecimal(row["Price"]);
            }
            return sum / dt.Rows.Count;
        }

        static void PrintTable(DataTable dt)
        {
            Console.WriteLine($"\nТаблиця: {dt.TableName} (Всього рядків: {dt.Rows.Count})");
            foreach (DataColumn col in dt.Columns)
                Console.Write($"{col.ColumnName,-15} | ");
            Console.WriteLine("\n---------------------------------------------------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write($"{item,-15} | ");
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


//INSERT INTO Coffee([Name], [Country], [Type], [Description], [Weight], [Price]) VALUES
//('Ethiopia Yirgacheffe', 'Ефіопія', 'арабіка', 'Яскравий квітковий аромат з нотами бергамоту', 150, 250.00),
//('Brazil Santos', 'Бразилія', 'арабіка', 'М''який смак з відтінком молочного шоколаду', 500, 180.00),
//('Vietnam Robusta', 'В''єтнам', 'робуста', 'Міцна кава з вираженою гірчинкою та горіховим смаком', 120, 110.00),
//('Italian Espresso Blend', 'Італія', 'купаж/бленд', 'Ідеальна суміш для насиченого еспресо з густою пінкою', 200, 210.00),
//('Uganda Bugisu', 'Уганда', 'робуста', 'Землистий профіль з пряними нотками', 190, 125.00),
//('Colombia Supremo', 'Колумбія', 'арабіка', 'Збалансований карамельний смак з фруктовою кислинкою', 300, 220.00),
//('Morning Blend', 'Бразилія/В''єтнам', 'купаж/бленд', 'Бадьорий мікс арабіки та робусти', 180, 160.00);
//GO