using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAP.Middleware.Connector;

namespace SapDataCopyApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime, IConfiguration configuration)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // "SAMPLE" という名前で登録したSAP接続情報を取得
                RfcDestination destination = RfcDestinationManager.GetDestination("SAMPLE");
                
                // PINGを送信してSAPに接続できるかを確認
                destination.Ping();
                
                // BAPI_CURRENCY_GETLISTの関数を取得
                IRfcFunction function = destination.Repository.CreateFunction("BAPI_CURRENCY_GETLIST");
                
                // BAPI_CURRENCY_GETLISTを実行
                function.Invoke(destination);
                
                // BAPI_CURRENCY_GETLISTの結果を取得
                IRfcTable table = function.GetTable("CURRENCY_LIST");
                _logger.LogInformation("取得した通貨の数: {0}", table.RowCount);
                
                // 最初の1行を取得してログに出力
                IRfcStructure firstRow = table.First();
                foreach (IRfcField col in firstRow)
                {
                    _logger.LogInformation("{0}: {1}", col.Metadata.Name, col.GetString());
                }

                // DB接続文字列を取得してSQL接続を作成
                string connectionString = _configuration.GetConnectionString("Database");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync(stoppingToken);

                    // データ登録用のDataTable作成
                    DataTable dataTable = new DataTable();
                    
                    // カラム定義の登録
                    dataTable.Columns.Add("CURRENCY", typeof(string));
                    dataTable.Columns.Add("CURRENCY_ISO", typeof(string));
                    dataTable.Columns.Add("ALT_CURR", typeof(string));
                    dataTable.Columns.Add("VALID_TO", typeof(DateTime));
                    dataTable.Columns.Add("LONG_TEXT", typeof(string));

                    // SAPから取得したデータをDataTableに登録
                    foreach (IRfcStructure row in table)
                    {
                        DataRow dataRow = dataTable.NewRow();

                        dataRow["CURRENCY"] = row.GetString("CURRENCY");
                        dataRow["CURRENCY_ISO"] = row.GetString("CURRENCY_ISO");
                        dataRow["ALT_CURR"] = row.GetString("ALT_CURR");
                        dataRow["VALID_TO"] = row.GetValue("VALID_TO");
                        dataRow["LONG_TEXT"] = row.GetString("LONG_TEXT");

                        dataTable.Rows.Add(dataRow);
                    }

                    // DataTableのデータをDBに登録
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn);
                    sqlBulkCopy.DestinationTableName = "CURRENCY_LIST";
                    await sqlBulkCopy.WriteToServerAsync(dataTable, stoppingToken);
                }
            }
            finally
            {
                // アプリケーションの停止を通知
                _applicationLifetime.StopApplication();
            }
        }
    }
}
