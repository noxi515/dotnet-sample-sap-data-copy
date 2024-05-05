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
                // "SAMPLE" �Ƃ������O�œo�^����SAP�ڑ������擾
                RfcDestination destination = RfcDestinationManager.GetDestination("SAMPLE");

                // PING�𑗐M����SAP�ɐڑ��ł��邩���m�F
                destination.Ping();
            }
            finally
            {
                // �A�v���P�[�V�����̒�~��ʒm
                _applicationLifetime.StopApplication();
            }
        }
    }
}
