using NUnit.Framework;
using LoginSignup;
using MySql.Data.MySqlClient;

[TestFixture]
public class WebApiTest
{
    [Test]
    public void TestMySQLConnection()
    {
        string connectionString = "server=localhost;port=3306;database=userdatabase;user=root;password=root";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Assert.IsTrue(connection.State == System.Data.ConnectionState.Open);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to establish connection to MySQL: " + ex.Message);
            }
        }
    }
}