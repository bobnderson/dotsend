using System.Data;
using System.Data.SqlClient;
using dotsend.Utility;
using Microsoft.Extensions.Configuration;

namespace dotsend.Utility
{

    public class SqlDataAccess
    {
        private readonly string connectionString;

        public SqlDataAccess()
        {
            this.connectionString = GetConnectionString();
        }

        private static string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = Security.Decrypt(configuration.GetSection("ConnectionStrings")["ConnectionString"]);

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string not found in appsettings.json.");
            }

            return connectionString;
        }

        public DataTable ExecuteQuery(string query, object? parameters)
        {
            DataTable dataTable = new();

            using (SqlConnection connection = new(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var prop in parameters.GetType().GetProperties())
                            {
                                var value = prop.GetValue(parameters);

                                if (value == null)
                                {
                                    query = query.Replace($"@{prop.Name}", "NULL");
                                    value = DBNull.Value;
                                }

                                command.Parameters.Add(new SqlParameter("@" + prop.Name, value));
                            }
                        }
                        using (SqlDataAdapter adapter = new(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }

                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }

            return dataTable;
        }

        public DataSet ExecuteQueryWithMultipleResultSets(string procedureName, SqlParameter[]? parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataSet dataset = new();
                SqlDataAdapter adapter = new();

                using (SqlCommand command = new SqlCommand(procedureName, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {

                        command.Parameters.AddRange(parameters);
                    }

                    adapter.SelectCommand = command;

                    try
                    {
                        adapter.Fill(dataset);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing stored procedure: {ex.Message}");
                    }
                }

                return dataset;
            }
        }

        public DataSet ExecuteMultipleQuery(string query, SqlParameter[]? parameters)
        {
            DataSet dataSet = new();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataSet);
                    }

                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return dataSet;
        }

    }


}
