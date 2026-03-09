using Microsoft.Extensions.Logging;
using Quartz;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class CleanupCancelledMeetingsJob : IJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<CleanupCancelledMeetingsJob> _logger;

    public CleanupCancelledMeetingsJob(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ILogger<CleanupCancelledMeetingsJob> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Iptal edilmis toplanti temizleme job'i basladi.");

        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var cancelledMeetings = await _unitOfWork.Meetings.GetCancelledMeetingsOlderThanAsync(cutoffDate);

        if (cancelledMeetings.Count == 0)
        {
            _logger.LogInformation("Silinecek iptal edilmis toplanti bulunamadi.");
            return;
        }

        _logger.LogInformation("{Count} adet iptal edilmis toplanti silinecek.", cancelledMeetings.Count);

        foreach (var meeting in cancelledMeetings)
        {
            foreach (var doc in meeting.Documents)
            {
                try
                {
                    await _fileService.DeleteFileAsync(doc.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Dosya silinemedi: {FilePath}", doc.FilePath);
                }
            }

            _unitOfWork.Meetings.Delete(meeting);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("{Count} adet iptal edilmis toplanti silindi. MSSQL Trigger ile log kaydedildi.",
            cancelledMeetings.Count);
    }
}
