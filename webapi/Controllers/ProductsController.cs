using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Configuration;
using System.Data;
using webapi.DBContext;
using webapi.Entities;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }

        #region Methods with Oracle DB 
        /// <summary>
        /// GET: products
        /// Call oracle SP EF
        /// Get Product data using OFFSET FETCH NEXT ROW
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(int pageNumber, int pageSize, string sortField, string sortOrder)
        {
            var products = await _context.Set<Product>().FromSqlRaw("BEGIN SP_PRODUCTSPAGINGOFFSET(:p_pageNumber, :p_pageSize, :p_sortColumn, " +
                                                        ":p_sortOrder, :p_recordSet); END;",
                                    new OracleParameter("p_pageNumber", OracleDbType.Int32, pageNumber, ParameterDirection.Input),
                                    new OracleParameter("p_pageSize", OracleDbType.Int32, pageSize, ParameterDirection.Input),
                                    new OracleParameter("p_sortColumn", OracleDbType.Varchar2, sortField, ParameterDirection.Input),
                                    new OracleParameter("p_sortOrder", OracleDbType.Varchar2, sortOrder, ParameterDirection.Input),
                                    new OracleParameter("p_recordSet", OracleDbType.RefCursor, ParameterDirection.Output))
                            .ToListAsync();

            //var products = await _context.Set<Product>().FromSqlRaw("BEGIN SP_PRODUCTSPAGING(:p_pageNumber, :p_pageSize, :p_sortColumn, :p_sortDirection, :p_totalRows, :PROD_DETAIL_CURSOR); END;",
            //            new OracleParameter("p_pageNumber", OracleDbType.Int32, pageNumber, ParameterDirection.Input),
            //            new OracleParameter("p_pageSize", OracleDbType.Int32, pageSize, ParameterDirection.Input),
            //            new OracleParameter("p_sortColumn", OracleDbType.Varchar2, sortField, ParameterDirection.Input),
            //            new OracleParameter("p_sortDirection", OracleDbType.Varchar2, sortOrder, ParameterDirection.Input),
            //            new OracleParameter("p_totalRows", OracleDbType.Int32, ParameterDirection.Output),
            //            new OracleParameter("PROD_DETAIL_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output))
            //           .ToListAsync();


            //var products = await _context.Set<Product>().FromSqlRaw("BEGIN SP_GETPRODUCTS(:PRODUCTCSR); END;",
            //             new OracleParameter("PRODUCTCSR", OracleDbType.RefCursor, ParameterDirection.Output))
            //            .ToListAsync();


            //await _context.Products.ToListAsync();

            return products;
        }

        /// <summary>
        /// GET: ProductsData
        /// Call SP with ODP data provider
        /// Get data in dataset and read by datatable using Row_Number
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ProductsData")]
        public async Task<ActionResult<IEnumerable<Product>>> ProductsData(int pageNumber, int pageSize, string sortField, string sortOrder)
        {
            var products = new List<Product>();
            var connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=system;Password=System1234#;";
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "SP_PRODUCTSPAGING";

                    // Add input parameter
                    command.Parameters.Add("p_pageNumber", OracleDbType.Int32).Value = pageNumber;
                    command.Parameters.Add("p_pageSize", OracleDbType.Int32).Value = pageSize;
                    command.Parameters.Add("p_sortColumn", OracleDbType.Varchar2).Value = sortField;
                    command.Parameters.Add("p_sortDirection", OracleDbType.Varchar2).Value = sortOrder;

                    command.Parameters.Add("p_totalRows", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    // Add OUT SYS_REFCURSOR parameter
                    command.Parameters.Add("PRODUCTCSR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    // Create the data adapter
                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    DataSet dataSet = new DataSet();

                    // Fill the DataTable with the results
                    dataAdapter.Fill(dataSet);

                    dataAdapter.Dispose();
                    command.Dispose();

                    DataTable dataTable = dataSet.Tables[0];

                    foreach (DataRow row in dataTable.Rows)
                    {
                        Product product = new Product();
                        product.Id = Convert.ToInt32(row["Id"]);
                        product.Name = row["Name"].ToString();
                        product.Description = row["Description"].ToString();
                        product.Category = row["Category"].ToString();
                        product.Price = Convert.ToInt32(row["Price"]);

                        products.Add(product);
                    }
                }
            }
            return products;
        }

        /// <summary>
        /// GET: GetWithoutEFDataReaderProducts
        /// Call SP by ODP data provider
        /// Fatch data by Data Reader using select query
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetWithoutEFDataReaderProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetWithoutEFDataReaderProducts()
        {
            var products = new List<Product>();
            var connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=system;Password=System1234#;";
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "SP_GETPRODUCTS";

                    // Add input parameter
                    //command.Parameters.Add("id", OracleDbType.Int32).Value = 6;

                    // Add OUT SYS_REFCURSOR parameter
                    command.Parameters.Add("PRODUCTCSR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    // Execute the command
                    command.ExecuteNonQuery();

                    // Retrieve the result from the OUT SYS_REFCURSOR parameter
                    var refCursor = (OracleRefCursor)command.Parameters["PRODUCTCSR"].Value;
                    using (var reader = refCursor.GetDataReader())
                    {
                        // Process the result set
                        while (reader.Read())
                        {
                            Product product = new Product();
                            // Access the values from the result set
                            product.Id = reader.GetInt32(0);
                            product.Name = reader.GetString(1);
                            product.Description = reader.GetString(2);
                            product.Category = reader.GetString(3);
                            product.Price = reader.GetInt32(4);

                            products.Add(product);
                        }
                    }


                }
            }
            return products;
        }

        // GET: products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Set<Product>().FromSqlRaw("BEGIN SP_GETPRODUCTDETAILS(:PROD_ID, :PROD_DETAIL_CURSOR); END;",
                         new OracleParameter("PROD_ID", OracleDbType.Int32, id, ParameterDirection.Input),
                         new OracleParameter("PROD_DETAIL_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output))
                        .ToListAsync();

            //var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product.FirstOrDefault();
        }

        // POST: products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        #endregion
    }
}
