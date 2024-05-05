using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAP.Middleware.Connector;

namespace SapDataCopyApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
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
                IRfcStructure row = table.First();
                foreach (IRfcField col in row)
                {
                    _logger.LogInformation("{0}: {1}", col.Metadata.Name, col.GetString());
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
