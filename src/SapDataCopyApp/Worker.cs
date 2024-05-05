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
                // "SAMPLE" �Ƃ������O�œo�^����SAP�ڑ������擾
                RfcDestination destination = RfcDestinationManager.GetDestination("SAMPLE");
                
                // PING�𑗐M����SAP�ɐڑ��ł��邩���m�F
                destination.Ping();
                
                // BAPI_CURRENCY_GETLIST�̊֐����擾
                IRfcFunction function = destination.Repository.CreateFunction("BAPI_CURRENCY_GETLIST");
                
                // BAPI_CURRENCY_GETLIST�����s
                function.Invoke(destination);
                
                // BAPI_CURRENCY_GETLIST�̌��ʂ��擾
                IRfcTable table = function.GetTable("CURRENCY_LIST");
                _logger.LogInformation("�擾�����ʉ݂̐�: {0}", table.RowCount);
                
                // �ŏ���1�s���擾���ă��O�ɏo��
                IRfcStructure firstRow = table.First();
                foreach (IRfcField col in firstRow)
                {
                    _logger.LogInformation("{0}: {1}", col.Metadata.Name, col.GetString());
                }

                // DB�ڑ���������擾����SQL�ڑ����쐬
                string connectionString = _configuration.GetConnectionString("Database");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync(stoppingToken);

                    // �f�[�^�o�^�p��DataTable�쐬
                    DataTable dataTable = new DataTable();
                    
                    // �J������`�̓o�^
                    dataTable.Columns.Add("CURRENCY", typeof(string));
                    dataTable.Columns.Add("CURRENCY_ISO", typeof(string));
                    dataTable.Columns.Add("ALT_CURR", typeof(string));
                    dataTable.Columns.Add("VALID_TO", typeof(DateTime));
                    dataTable.Columns.Add("LONG_TEXT", typeof(string));

                    // SAP����擾�����f�[�^��DataTable�ɓo�^
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

                    // DataTable�̃f�[�^��DB�ɓo�^
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn);
                    sqlBulkCopy.DestinationTableName = "CURRENCY_LIST";
                    await sqlBulkCopy.WriteToServerAsync(dataTable, stoppingToken);
                }
            }
            finally
            {
                // �A�v���P�[�V�����̒�~��ʒm
                _applicationLifetime.StopApplication();
            }
        }
    }
}
