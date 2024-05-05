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
            }
            finally
            {
                // アプリケーションの停止を通知
                _applicationLifetime.StopApplication();
            }
        }
    }
}
