using Microsoft.Data.Sqlite;
var connection = new SqliteConnection("Data Source=C:/Endava/EndevLocal/Apps/Fibi_Insurance/API/insurances.db");
connection.Open();
using var command = connection.CreateCommand();
command.CommandText = "SELECT BrokerCode, Name, Email, BrokerStatus FROM Brokers ORDER BY BrokerCode;";
using var reader = command.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"{reader.GetString(0)} | {reader.GetString(1)} | {reader.GetString(2)} | {reader.GetInt32(3)}");
}
