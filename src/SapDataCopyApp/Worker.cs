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
                IRfcStructure row = table.First();
                foreach (IRfcField col in row)
                {
                    _logger.LogInformation("{0}: {1}", col.Metadata.Name, col.GetString());
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
