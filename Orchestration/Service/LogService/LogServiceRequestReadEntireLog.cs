namespace Database
{
    public class LogServiceRequestReadEntireLog : LogServiceRequest<LogServiceResponseResultReadEntireLog>
    {
        public override ServiceResponseResult Process()
        {
            return new LogServiceResponseResultReadEntireLog(LogService.Get().LogManager.LogRecords);
        }
    }
}
